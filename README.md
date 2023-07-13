# PULSE - Post-Processing Utilities, LUTs, and Special Effects

Pulse is a comprehensive post-processing pipeline designed to enhance the visual quality of rendered images. It offers a wide range of customizable effects and filters to apply to the final rendered frames. Pulse enables developers to add depth of field, motion blur, color grading, tone mapping, and many other post-processing effects to achieve the desired aesthetic for their projects. Also you can create you custom post-processing effects with shaders (Probably it will be the Unity's Full Screen Renderer Feature).

**Note:** Only tested on Windows platform in Unity Editor (2022.3.4f1). I will test on mobile platform (android) soon.
This repository is part of [C.A.T.S. - Cutting-edge Adaptive Technology Stack](https://github.com/Warwlock/C.A.T.S).

**Note:** This tool is still in development, there can be code errors and performance drops.

## Installation

Just put `Pulse` folder into `Assets` directory in your Unity project.

## Usage

* First completely disable default pos processing. You can disable it from `Universal Render Data` asset.
* Also in any case disable it in all cameras.
* Go to your `Universal Render Data` asset and add `Pulse Post Processing` Render Feature.
* Create an empty game object and add `Pulse Volume` component.
* Now you can add any effect you want.

## Create your own effects

* **Note:** Custom effects weren't tested with shader graph.
* You can follow that script to create effect:

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class PulseGammaCorrection : IPulseEffect // Instead of "Mono Behaviour" write "IPulseEffect"
{
    // You variables, those will be show up in inspector.
    public float GammaValue = 1;

    Material material;
    
    private void OnEnable()
    {
        name = "Custom/My Custom Gamma Effect"; // Your effect name that will appear in the list that appears when adding an effect.
    }

    public override void OnSetup()
    {
        material = new Material(Shader.Find("Hidden/_MyCustomGammaMaterial")); // Effect material
    }

    public override void OnRender(ref RTHandle src, ref RTHandle dst)
    {
        // Set you material properties
        material.SetTexture("_BlitTexture", src);
        material.SetFloat("_Gamma", GammaValue);

        // And lastly blit source texture to destination texture with your custom material
        Blit(ref src, ref dst, material);
    }
}
```

And this is basic shader code:

```hlsl
Shader "Hidden/_MyCustomGammaMaterial" // Your effect material name
{
    HLSLINCLUDE

        // Include neccessary libraries
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Assets/Pulse/Shaders/PulseCommon.hlsl"

        // Porperties from renderer feature
        half _Gamma;

        // The fragment shader definition. Also using "half" instead of "float" is for mobile platform.
        half4 myFragment(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord); // Sample source texture
            return half4(pow(saturate(col.xyz), _Gamma), col.a); // Return pixel color
        }

    ENDHLSL

    SubShader
    {
        Tags
        { 
            "RenderType" = "Opaque" "RenderPipeline"="UniversalPipeline"
        }
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "MyCustom_Pass"
        
            HLSLPROGRAM

            // Pragmas
            #pragma vertex vert
            #pragma fragment myFragment

            ENDHLSL
        }
    }
}
```

## Overlay Cameras

* Setup your overlay camera as usually.
* In renderer features, add `Pulse Multiple Camera` feature.
* In volume settings, add cameras you don't want to render effects.

## How it Works

* `Pulse Post Processing` renderer feature iterates over all pulse volumes.
* Then it controls if camera is in disabled cameras.
* If it is local volume then renderer feature uses blending material.
* If it is global volume, directly renders to screen.

## Limitations:

* Overlay camera only preserves alpha channel with 0 and 1. That's why using effects like FXAA can cause some pixel bleeding. Also chromatic aberration is another problem with overlay cameras.
* Each effect adds new set pass call. That can cause performance drop especially on mobile platform.
* Duplicating volumes also duplicates same effects. That means if you change an effect in duplicated volume, it will change effect in original volume too. That's why all effects must removed in duplicated volume before changing values.
* In HDR mode, overlay camera uses high brightness values to create overlay camera mask. If you use too bright light source (intensity higher than around 300), it will start to mask bright areas.

Overlay problem with Chromatic Aberration:
![](https://github.com/Warwlock/PULSE/blob/main/Images/overlayProblem.png)

## Done:

- [X] Color Adjustment
- [X] Chromatic Aberration
- [X] Tonemapping (Thumblin Rushmeier, Reinhard, Hable, Uchimura, ACES)
- [X] Film Grain
- [X] White Balance
- [X] Vignette
- [X] Gamma Correction
- [X] FXAA

## ToDo:

- [ ] FSR 2.0 `WIP`
- [ ] Depth of Field
- [ ] Bloom `WIP`
- [ ] Blur `WIP`
- [ ] Lens Distortion
- [ ] Motion Blur
- [ ] LUTs

Less priority:
- [ ] Sharpness
- [ ] Pixel Art Style Effects
- [ ] Cartoon Style Effects
- [ ] Blend Modes
- [ ] Lens Flare

Under Consideration:
- [ ] Blob Shadows
- [ ] Antialiasing
- [ ] SSSR - [https://github.com/GPUOpen-Effects/FidelityFX-SSSR](https://github.com/GPUOpen-Effects/FidelityFX-SSSR)
- [ ] CACAO - [CACAO - https://github.com/GPUOpen-Effects/FidelityFX-CACAO](https://github.com/GPUOpen-Effects/FidelityFX-CACAO)

* Newly made effects (`Will Be Added` tagged ones) will be added to this repository soon. I can't promise it will be regular and organised.
* Yes, Unity has some of the Post Processing effetcs. But those are Renderer Feature based effects.
* References for scripts and documentation will be added.

## References

* https://www.youtube.com/@Acerola_t //Thanks for Youtube tutorials
* https://github.com/GarrettGunnell/Post-Processing
* https://www.shadertoy.com/view/lsKSWR //Vignette
* https://www.shadertoy.com/view/lslGzl //Tonemapping
* https://www.shadertoy.com/view/WdjSW3 //Tonemapping
* https://lettier.github.io/3d-game-shaders-for-beginners/index.html
* https://github.com/GPUOpen-Effects/FidelityFX-FSR2
* https://github.com/ndepoel/FSR2Unity //FSR2 for BIRP
* https://www.shadertoy.com/view/stlSzf //FXAA
* http://blog.simonrodriguez.fr/articles/2016/07/implementing_fxaa.html
* https://www.cyanilux.com/tutorials/custom-renderer-features/

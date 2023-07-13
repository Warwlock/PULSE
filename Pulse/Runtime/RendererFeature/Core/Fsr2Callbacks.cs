﻿// Copyright (c) 2023 Nico de Poel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;

namespace FidelityFX
{
    /// <summary>
    /// A collection of callbacks required by the FSR2 process.
    /// This allows some customization by the game dev on how to integrate FSR2 into their own game setup.
    /// </summary>
    public interface IFsr2Callbacks
    {
        Shader LoadShader(string name);
        void UnloadShader(Shader shader);
        ComputeShader LoadComputeShader(string name);
        void UnloadComputeShader(ComputeShader shader);
        
        /// <summary>
        /// Apply a mipmap bias to in-game textures to prevent them from becoming blurry as the internal rendering resolution lowers.
        /// This will need to be customized on a per-game basis, as there is no clear universal way to determine what are "in-game" textures.
        /// The default implementation will simply apply a mipmap bias to all 2D textures, which will include things like UI textures and which might miss things like terrain texture arrays.
        /// 
        /// Depending on how your game organizes its assets, you will want to create a filter that more specifically selects the textures that need to have this mipmap bias applied.
        /// You may also want to store the bias offset value and apply it to any assets that are loaded in on demand.
        /// </summary>
        void ApplyMipmapBias(float biasOffset);
    }
    
    /// <summary>
    /// Default implementation of IFsr2Callbacks using simple Resources calls.
    /// These are fine for testing but a proper game will want to extend and override these methods.
    /// </summary>
    public class Fsr2CallbacksBase: IFsr2Callbacks
    {
        protected float CurrentBiasOffset = 0;
        
        public virtual Shader LoadShader(string name)
        {
            return Resources.Load<Shader>(name);
        }

        public virtual void UnloadShader(Shader shader)
        {
            Resources.UnloadAsset(shader);
        }

        public virtual ComputeShader LoadComputeShader(string name)
        {
            return Resources.Load<ComputeShader>(name);
        }

        public virtual void UnloadComputeShader(ComputeShader shader)
        {
            Resources.UnloadAsset(shader);
        }

        public virtual void ApplyMipmapBias(float biasOffset)
        {
            CurrentBiasOffset += biasOffset;
            
            foreach (var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                //texture.mipMapBias += biasOffset;
            }
        }
    }
}

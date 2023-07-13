using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[AddComponentMenu("Miscellaneous/Pulse Volume")]
public class PulseVolume : MonoBehaviour
{
    public enum modeList
    {
        Global,
        Local
    }

    public modeList Mode = modeList.Global;
    public float BlendDistance;

    [HideInInspector]
    public List<IPulseEffect> EffectList = new List<IPulseEffect>();

    [HideInInspector]
    public List<Camera> disableCamera = new List<Camera>();

    [Header("Debugging")]
    public bool showBlendRange;

    public void RemoveEffects()
    {
        EffectList.Clear();
    }

    public IEnumerable<Type> GetList()
    {
        // Scan for all types that inherit from IPulseEffect
        Type baseType = typeof(IPulseEffect);
        Assembly assembly = Assembly.GetExecutingAssembly(); // Replace with the appropriate assembly if needed
        return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
    }

    public void Add(Type effectType)
    {
        Undo.RecordObject(this, "Add Pulse Effect");
        IPulseEffect effect = ScriptableObject.CreateInstance(effectType) as IPulseEffect;
        EffectList.Add(effect);
    }

    public void Remove(IPulseEffect effect)
    {
        Undo.RecordObject(this, "Remove Pulse Effect");
        EffectList.Remove(effect);
    }

    public void MoveUp(int index)
    {
        Undo.RecordObject(this, "Move Up Pulse Effect");
        if (index > 0 && index < EffectList.Count)
        {
            var temp = EffectList[index - 1];
            EffectList[index - 1] = EffectList[index];
            EffectList[index] = temp;
        }
    }

    public void MoveDown(int index)
    {
        Undo.RecordObject(this, "Move Down Pulse Effect");
        if (index >= 0 && index < EffectList.Count - 1)
        {
            var temp = EffectList[index + 1];
            EffectList[index + 1] = EffectList[index];
            EffectList[index] = temp;
        }
    }

    public void OnSetupEffects(CameraData cameraData)
    {
        if (Mode == modeList.Local)
        {
            if(IsPointInsideCube(cameraData.camera.transform.position, GetOuterCube()))
            {

            }
            else
            {
                return;
            }

        }
        for(int i = 0; i < EffectList.Count; i++)
        {
            EffectList[i].OnSetup();
        }
    }

    public void OnRenderEffects(CameraData cameraData, CommandBuffer cmd, ref RTHandle firstTempTex, ref RTHandle secondTempTex, Material blendMat)
    {
        if (Mode == modeList.Local)
        {
            if (IsPointInsideCube(cameraData.camera.transform.position, GetOuterCube()))
            {
                blendMat.SetFloat("_BlendDistance", CalculateBlendDistance(cameraData.camera.transform.position));
            }
            else
            {
                return;
            }
        }
        else
        {
            blendMat.SetFloat("_BlendDistance", 1);
        }
        for (int i = 0; i < EffectList.Count; i++)
        {
            if (!EffectList[i].enabled)
            {
                continue;
            }
            EffectList[i].cmd = cmd;
            EffectList[i].OnRender(ref firstTempTex, ref secondTempTex);
            CoreUtils.Swap(ref firstTempTex, ref secondTempTex);
        }
    }

    public void OnTextureInitializeEffects(CameraData cameraData)
    {
        for (int i = 0; i < EffectList.Count; i++)
        {
            EffectList[i].OnTextureInitialize(cameraData);
        }
    }

    public void OnDisposeEffects()
    {
        for(int i = 0; i < EffectList.Count; i++)
        {
            EffectList[i].Dispose();
        }
    }

    public bool EffectNumber()
    {
        int index = 0;
        for (int i = 0; i < EffectList.Count; i++)
        {
            if (!EffectList[i].enabled)
            {
                continue;
            }
            index++;
        }
        if(index % 2 == 1)
        {
            return false;
        }
        return true;
    }

    public bool AnyEffectActive()
    {
        for(int i = 0; i < EffectList.Count; i++)
        {
            if (EffectList[i].enabled)
            {
                return true;
            }
        }
        return false;
    }

    Bounds GetInnerCube()
    {
        return new Bounds(transform.position, transform.localScale);
    }

    Bounds GetOuterCube()
    {
        return new Bounds(transform.position, transform.localScale + Vector3.one * BlendDistance);

    }

    bool IsPointInsideCube(Vector3 point, Bounds bounds)
    {
        Vector3 minBound = bounds.center - bounds.size * 0.5f;
        Vector3 maxBound = bounds.center + bounds.size * 0.5f;

        if (point.x >= minBound.x && point.x <= maxBound.x &&
            point.y >= minBound.y && point.y <= maxBound.y &&
            point.z >= minBound.z && point.z <= maxBound.z)
        {
            return true;
        }

        return false;
    }

    float CalculateBlendDistance(Vector3 cameraPosition)
    {
        Bounds innerCube = GetInnerCube();
        Bounds outerCube = GetOuterCube();

        // Calculate the closest point on the cube's surface to the given point
        Vector3 closestInnerPoint = innerCube.ClosestPoint(cameraPosition);

        // Calculate the distance between the point and the closest point on the cube
        float distanceToInner = Vector3.Distance(cameraPosition, closestInnerPoint);
        float distanceToOuter = CalculateDistanceToCubeBoundaries(outerCube, cameraPosition);

        return 1 - distanceToInner / (distanceToInner + distanceToOuter);
    }

    float CalculateDistanceToCubeBoundaries(Bounds cubeBounds, Vector3 pointInsideCube)
    {
        // Get the length of one side of the cube
        float cubeLength = cubeBounds.size.x;

        // Calculate the distances to each face of the cube
        float distanceToFront = Mathf.Abs(pointInsideCube.z - cubeBounds.center.z - cubeLength / 2);
        float distanceToBack = Mathf.Abs(pointInsideCube.z - cubeBounds.center.z + cubeLength / 2);
        float distanceToTop = Mathf.Abs(pointInsideCube.y - cubeBounds.center.y - cubeLength / 2);
        float distanceToBottom = Mathf.Abs(pointInsideCube.y - cubeBounds.center.y + cubeLength / 2);
        float distanceToLeft = Mathf.Abs(pointInsideCube.x - cubeBounds.center.x - cubeLength / 2);
        float distanceToRight = Mathf.Abs(pointInsideCube.x - cubeBounds.center.x + cubeLength / 2);

        // Find the minimum distance among all the calculated distances
        float minDistance = Mathf.Min(distanceToFront, distanceToBack, distanceToTop, distanceToBottom, distanceToLeft, distanceToRight);

        return minDistance;
    }

    public bool IsInsideInBox(CameraData cameraData)
    {
        if(Mode == modeList.Global)
        {
            return true;
        }

        if(Mode == modeList.Local && IsPointInsideCube(cameraData.camera.transform.position, GetOuterCube()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDestroy()
    {
        EffectList.Clear();
    }

    private void Update()
    {

    }
}
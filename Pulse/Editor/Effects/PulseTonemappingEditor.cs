using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PulseTonemapping))]
public class PulseTonemappingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PulseTonemapping obj = serializedObject.targetObject as PulseTonemapping;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("toneMapType"));
        EditorGUILayout.Space();

        if (obj.toneMapType == PulseTonemapping.ToneMappingTypes.TumblinRushmeier)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumDisplayLuminance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumContrast"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LuminanceChangeRate"));
        }

        if (obj.toneMapType == PulseTonemapping.ToneMappingTypes.ReinhardExtended)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WhitePoint"));
        }

        if (obj.toneMapType == PulseTonemapping.ToneMappingTypes.Hable)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShoulderStrength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LinearStrength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LinearAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ToeStrength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ToeNumerator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ToeDenominator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LinearWhitePoint"));
        }

        if (obj.toneMapType == PulseTonemapping.ToneMappingTypes.Uchimura)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxBrightness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Contrast"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LinearStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LinearLength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BlackTightnessShape"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BlackTightnessOffset"));
        }

        EditorGUILayout.Space();

        /*if (GUILayout.Button("Documentation"))
        {
            Application.OpenURL("https://github.com/Warwlock/blender-nodes-subgraph");
        }*/

        serializedObject.ApplyModifiedProperties();
        //base.OnInspectorGUI();
    }
}

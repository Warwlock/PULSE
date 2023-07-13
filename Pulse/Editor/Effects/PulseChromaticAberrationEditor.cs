using System.Drawing.Drawing2D;
using UnityEditor.Rendering.Universal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PulseChromaticAberration))]
public class PulseChromaticAberrationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PulseChromaticAberration obj = serializedObject.targetObject as PulseChromaticAberration;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ChromaticAberrationType"));
        EditorGUILayout.Space();

        if (obj.ChromaticAberrationType == PulseChromaticAberration.ChromaticAberrationTypes.Basic)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intensity"));
        }

        if (obj.ChromaticAberrationType == PulseChromaticAberration.ChromaticAberrationTypes.Advanced)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intensity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hardness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("focalOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("channelOffsets"));
        }

        serializedObject.ApplyModifiedProperties();
        //base.OnInspectorGUI();
    }
}
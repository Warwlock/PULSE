using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CATS.Pulse;

namespace CATS.PulseEditor
{
    [CustomEditor(typeof(IPulseEffect), true)]
    public class IPulseEffectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty effectProperty = serializedObject.GetIterator();

            while (effectProperty.NextVisible(true))
            {
                // Exclude some default properties if needed
                if (effectProperty.name == "m_Script" || effectProperty.name == "enabled" || effectProperty.name == "expanded")
                    continue;

                if (effectProperty.name == "x" || effectProperty.name == "y" || effectProperty.name == "z")
                    continue;

                EditorGUILayout.PropertyField(effectProperty, true); // Display the property in the inspector
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

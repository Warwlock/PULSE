using CATS.Pulse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace CATS.PulseEditor
{
    [CustomEditor(typeof(PulseVolume))]
    public class PulseVolumeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PulseVolume pv = target as PulseVolume;

            //base.OnInspectorGUI();
            SerializedProperty pvProperty = serializedObject.GetIterator();

            while (pvProperty.NextVisible(true))
            {
                // Exclude some default properties if needed
                if (pvProperty.name == "m_Script")
                    continue;

                EditorGUILayout.PropertyField(pvProperty, true); // Display the property in the inspector
            }

            if (GUILayout.Button("Remove All Effects"))
            {
                Undo.RecordObject(pv, "Remove All Effects");
                pv.RemoveEffects();
            }

            EditorGUILayout.Space();

            //Color oldCol = GUI.backgroundColor;
            //GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
            //EditorGUILayout.BeginFoldoutHeaderGroup(true, "AAA");
            //EditorGUILayout.EndFoldoutHeaderGroup();

            for (int i = 0; i < pv.EffectList.Count; i++)
            {
                IPulseEffect effect = pv.EffectList[i];

                if(effect == null)
                {
                    pv.EffectList.RemoveAt(i);
                    continue;
                }

                // Create a serialized object for the effect
                SerializedObject effectSerializedObject = new SerializedObject(effect);
                SerializedProperty effectSerializedProperty = effectSerializedObject.GetIterator();

                CoreEditorUtils.DrawSplitter();
                effect.expanded = CoreEditorUtils.DrawHeaderToggleFoldout(new GUIContent(effect.name.Split("/").Last()),
                    effect.expanded,
                    effectSerializedObject.FindProperty("enabled"),
                    pos => OnContextClick(pos, effect, i)
                    , null, null, "");

                if (effect.expanded)
                {
                    CreateEditor(effect).OnInspectorGUI();
                    EditorGUILayout.Space();
                }

                effectSerializedObject.ApplyModifiedProperties(); // Apply the modified properties to the effect object
            }
            CoreEditorUtils.DrawSplitter();

            serializedObject.ApplyModifiedProperties(); // Apply the modified properties to the target object

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("disableCamera"), true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Effect"))
            {
                AddOverride();
            }

        }

        void OnContextClick(Vector2 position, IPulseEffect effect, int index)
        {
            PulseVolume pv = target as PulseVolume;
            GenericMenu menu = new GenericMenu();

            menu.AddItem(EditorGUIUtility.TrTextContent("Remove"), false, () => { Undo.RecordObject(pv, "Remove Pulse Effect"); pv.Remove(effect); });
            menu.AddSeparator("");

            menu.AddItem(EditorGUIUtility.TrTextContent("Move Up"), false, () => { Undo.RecordObject(pv, "Move Up Pulse Effect"); pv.MoveUp(index); });
            menu.AddItem(EditorGUIUtility.TrTextContent("Move Down"), false, () => { Undo.RecordObject(pv, "Move Down Pulse Effect"); pv.MoveDown(index); });

            menu.DropDown(new Rect(position, Vector2.zero));
        }

        void AddOverride()
        {
            PulseVolume pv = target as PulseVolume;
            var mousePos = Event.current.mousePosition;
            var pos = new Vector2(EditorGUIUtility.currentViewWidth / 2, mousePos.y);
            FilterWindow.Show(pos, new PulseEffectProvider(GetList(), pv));
        }

        IEnumerable<Type> GetList()
        {
            // Scan for all types that inherit from IPulseEffect
            Type baseType = typeof(IPulseEffect);

            // Get all loaded assemblies in the current application domain
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Filter and return the types that inherit from IPulseEffect from all assemblies
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }
    }

    public class MyScriptGizmoDrawer
    {
        private const string s_IconsPath = "Assets/Pulse/Editor/Icons/";

        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawVolumeBox(PulseVolume scr, GizmoType gizmoType)
        {
            Color oldCol = Gizmos.color;

            if (scr.Mode == PulseVolume.modeList.Global)
            {
                Gizmos.color = new Color(1, 1, 0, 0.3f);
                Gizmos.DrawCube(scr.transform.position, Vector3.one * 0.5f);
                Gizmos.color = oldCol;
                return;
            }

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(scr.transform.position, scr.transform.localScale);

            if (scr.showBlendRange && scr.BlendDistance > 0)
            {
                Gizmos.color = new Color(0, 1, 1, 0.25f);
                Gizmos.DrawCube(scr.transform.position, scr.transform.localScale + Vector3.one * scr.BlendDistance);
            }

            Gizmos.color = oldCol;
        }

        /*[DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawMiddleIcon(PulseVolume scr, GizmoType gizmoType)
        {
            Color oldCol = Gizmos.color;
            Gizmos.color = new Color(1, 1, 1, 1f);

            Gizmos.DrawIcon(scr.transform.position, s_IconsPath + "pulseVolume.png", true);

            Gizmos.color = oldCol;
        }*/
    }
}
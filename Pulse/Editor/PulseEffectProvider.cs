using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;
using CATS.Pulse;

namespace CATS.PulseEditor
{
    public class PulseEffectProvider : IProvider
    {
        public Vector2 position { get; set; }

        Dictionary<Element, string> groupElementPairs = new Dictionary<Element, string>();
        IEnumerable<Type> effectTypes;
        PulseVolume pv;

        public PulseEffectProvider(IEnumerable<Type> effectTypes, PulseVolume pv)
        {
            this.effectTypes = effectTypes;
            this.pv = pv;
        }

        class EffectElement : Element
        {
            public Type type;

            public EffectElement(int level, string label, Type type)
            {
                this.level = level;
                this.type = type;
                // TODO: Add support for custom icons
                content = new GUIContent(label);
            }
        }

        public void CreateComponentTree(List<Element> tree)
        {
            tree.Add(new GroupElement(0, "Effects"));

            // rootNode = new PathNode();
            // Create instances of the found effect types and add them to the EffectList

            foreach (Type effectType in effectTypes)
            {
                IPulseEffect effect = ScriptableObject.CreateInstance(effectType) as IPulseEffect;

                var paths = effect.name.Split("/");
                for (int i = 0; i < paths.Length; i++)
                {
                    if (i == paths.Length - 1)
                    {
                        //tree.Add(new EffectElement(i + 1, paths[i], effectType));
                        groupElementPairs.Add(new EffectElement(i + 1, paths[i], effectType), paths[i]);
                    }
                    else
                    {
                        //tree.Add(new GroupElement(i + 1, paths[i]));
                        groupElementPairs.Add(new GroupElement(i + 1, paths[i]), paths[i]);
                    }
                }

                ScriptableObject.DestroyImmediate(effect);
            }

            // Create a new dictionary to store unique values
            Dictionary<Element, string> uniqueGroupElementPairs = new Dictionary<Element, string>();

            foreach (var pair in groupElementPairs)
            {
                // Check if the value already exists in the new dictionary
                if (!uniqueGroupElementPairs.ContainsValue(pair.Value))
                {
                    // If the value doesn't exist, add the key-value pair to the new dictionary
                    uniqueGroupElementPairs.Add(pair.Key, pair.Value);
                }
            }

            foreach (var pair in uniqueGroupElementPairs)
            {
                tree.Add(pair.Key);
            }
        }

        public bool GoToChild(Element element, bool addIfComponent)
        {
            if (element is EffectElement)
            {
                pv.Add((element as EffectElement).type);
                return true;
            }
            else
            {

                return false;
            }
        }
    }
}

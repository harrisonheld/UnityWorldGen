// using System;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;

// [CustomEditor(typeof(CustomTerrain))]
// public class CustomTerrain_Inspector : Editor
// {
//     public VisualTreeAsset m_InspectorXML;
//     public override VisualElement CreateInspectorGUI()
//     {
//         // Create a new VisualElement to be the root of our inspector UI
//         VisualElement root = new VisualElement();

//         // Load from default reference
//         m_InspectorXML.CloneTree(root);

//         root.AddToClassList("customInspectorRoot");

//         SerializedProperty biomesProperty = serializedObject.FindProperty("_biomes");

//         for (int i = 0; i < biomesProperty.arraySize; i++)
//         {
//             SerializedProperty biomeElement = biomesProperty.GetArrayElementAtIndex(i);

//             // Create a foldout for each biome
//             Foldout biomeFoldout = new Foldout();
            
//             // EditorGUILayout.PropertyField(biomeElement);
//             biomeFoldout.text = "Biome " + i;
//             biomeFoldout.AddToClassList("biomeFoldout");
//             root.Add(biomeFoldout);

//             // Create a property field for each property in the biome
//             SerializedProperty iterator = biomeElement.Copy();
//             bool hasNext = iterator.NextVisible(true);
//             while (hasNext)
//             {
//                 if (SerializedProperty.EqualContents(iterator, biomesProperty.GetArrayElementAtIndex(i)))
//                     break;

//                 PropertyField propertyField = new PropertyField(iterator.Copy(), iterator.displayName);
//                 biomeFoldout.Add(propertyField);
//                 Debug.Log(propertyField);
//                 hasNext = iterator.NextVisible(false);
//             }
//         }

//         // Return the finished inspector UI
//         return root;
//     }
// }

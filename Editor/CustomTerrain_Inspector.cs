using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrain_Inspector : Editor
{
    public VisualTreeAsset m_InspectorXML;
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement root = new VisualElement();

        root.AddToClassList("customInspectorRoot");

        SerializedProperty biomesProperty = serializedObject.FindProperty("_biomes");

        for (int i = 0; i < biomesProperty.arraySize; i++)
        {
            SerializedProperty biomeElement = biomesProperty.GetArrayElementAtIndex(i);
            // heightmapProperty.objectReferenceValue as HeightmapBase;
            SerializedProperty heightmapProperty = biomeElement.FindPropertyRelative("_heightmap");//.objectReferenceValue as HeightmapBase;
            Foldout heightmapFoldout = new Foldout();
            heightmapFoldout.text = "Heightmap";
            SerializedObject heightmapSerializedObject = new SerializedObject(heightmapProperty.objectReferenceValue);
            SerializedProperty iterator = heightmapSerializedObject.GetIterator();

            while (iterator.NextVisible(true))
            {
                // Debug.Log(iterator.displayName);
                if (iterator.name == "m_Script")
                {
                    continue;
                }
                // Create UI elements for each property here
                Debug.Log(iterator.Copy().displayName);
                PropertyField propertyField = new PropertyField(iterator.Copy());
                switch (iterator.propertyType) {
                    case SerializedPropertyType.Float:
                        // Define the range for your slider
                        float minValue = 0f; // Example minimum value
                        float maxValue = 100f; // Example maximum value

                        var slider = new Slider(iterator.displayName, minValue, maxValue)
                        {
                            value = iterator.floatValue
                        };
                        slider.RegisterValueChangedCallback(evt =>
                        {
                            iterator.floatValue = evt.newValue;
                            iterator.serializedObject.ApplyModifiedProperties();
                        });
                        heightmapFoldout.Add(slider);
                        break;

                    default:
                        var label = new Label($"{iterator.displayName}: {iterator.propertyType} not supported");
                        heightmapFoldout.Add(label);
                        break;
                }
                heightmapFoldout.Add(propertyField);
            }

            // Create a property field for each property in the biome
            // SerializedProperty iterator = heightmapProperty.Copy();
            // bool hasNext = iterator.NextVisible(true);
            // while (hasNext)
            // {
            //     // if (SerializedProperty.EqualContents(iterator, heightmapProperty.GetArrayElementAtIndex(i)))
            //     //     break;
            //     // Debug.Log(iterator.displayName);
            //     PropertyField propertyField = new PropertyField(iterator.Copy(), iterator.displayName);
            //     // Debug.Log(propertyField);
            //     heightmapFoldout.Add(propertyField);
            //     hasNext = iterator.NextVisible(false);
            // }


            // SerializedProperty nameProperty = biomeElement.FindPropertyRelative("_name");
            // Create a foldout for each biome
            Foldout biomeFoldout = new Foldout();

            // EditorGUILayout.PropertyField(biomeElement);
            biomeFoldout.text = "Biome " + i;

            // nameProperty.serializedObject.ApplyModifiedProperties(); // Ensure properties are up to date
            // nameProperty.serializedObject.Update(); // Ensure serialized object is up to date
            // nameProperty.ValueChanged += () => UpdateFoldoutText(biomeFoldout, nameProperty.stringValue);

            biomeFoldout.AddToClassList("biomeFoldout");

            biomeFoldout.Add(heightmapFoldout);
            root.Add(biomeFoldout);

            // // Determine the type of heightmap
            // if (heightmapProperty != null && heightmapProperty.objectReferenceValue != null)
            // {
            //     HeightmapBase heightmap = heightmapProperty.objectReferenceValue as HeightmapBase;

            //     if (heightmap != null)
            //     {
            //         // Show properties based on the type of heightmap
            //         if (heightmap is HeightmapFlat)
            //         {
            //             HeightmapFlat heightmapFlat = heightmap as HeightmapFlat;
            //             PropertyField heightPropertyField = new PropertyField(SerializedPropertyType.Float, heightmapFlat.GetSerializedProperty("Height"));
            //             heightPropertyField.Bind(heightmapFlat);
            //             biomeFoldout.Add(heightPropertyField);
            //         }
            //         // Add more conditions for other heightmap types if needed
            //     }
            // }

            // if (heightmapProperty != null && heightmapProperty.objectReferenceValue != null)
            // {
            //     // Get the type of the heightmap
            //     Type heightmapType = heightmapProperty.objectReferenceValue.GetType();
            //     Debug.Log(heightmapType);

            //     // Iterate over properties of the heightmap type
            //     foreach (var property in heightmapType.GetProperties())
            //     {
            //         Debug.Log("HERE");

            //         // Exclude properties inherited from base classes
            //         // Debug.Log(JsonUtility.ToJson(property.displayName));

            //         // if (property.DeclaringType == heightmapType)
            //         // {
            //             // Create a property field for each property of the heightmap
            //             PropertyField propertyField = new PropertyField(property, property.displayN);
            //             heightmapFoldout.Add(propertyField);
            //         // }
            //     }
            // }

            // if (heightmapProperty != null && heightmapProperty.objectReferenceValue != null)
            // {
            //     HeightmapBase heightmap = heightmapProperty.objectReferenceValue as HeightmapBase;
            //     Debug.Log(heightmap);

            //     if (heightmap != null && heightmap is HeightmapSimplex)
            //     {
            //         Debug.Log("HERE");

            // HeightmapSimplex heightmapSimplex = heightmap as HeightmapSimplex;

            // Create a serialized object for the heightmap instance
            // SerializedObject heightmapSerializedObject = new SerializedObject(heightmap);

            // Get all serialized properties of the heightmap instance
            // SerializedProperty property = heightmapSerializedObject.GetIterator();
            // property.Next(true);
            // Debug.Log(property.displayName);


            // Create a property field for each property in the biome
            // SerializedProperty heightmapIterator = heightmap.Copy();
            // bool hasNext = heightmapIterator.NextVisible(true);
            // while (hasNext)
            // {
            //     // if (SerializedProperty.EqualContents(iterator, biomesProperty.GetArrayElementAtIndex(i)))
            //     //     break;
            //     PropertyField propertyField = new PropertyField(heightmapIterator.Copy(), heightmapIterator.displayName);
            //     heightmapFoldout.Add(propertyField);
            //     hasNext = heightmapIterator.NextVisible(false);
            // }

            // // Iterate over properties of the heightmap instance
            // while (property.NextVisible(false))
            // {
            //     // Create a property field for each property of the heightmap
            //     PropertyField propertyField = new PropertyField(property,property.displayName);
            //     heightmapFoldout.Add(propertyField);
            // }
            //     }
            // }



            //     SerializedProperty iterator = heightmapProperty.Copy();
            // bool hasNext = iterator.NextVisible(true);
            // while (hasNext)
            // {
            //     PropertyField propertyField = new PropertyField(iterator.Copy(), iterator.displayName);
            //     heightmapFoldout.Add(propertyField);
            //     hasNext = iterator.NextVisible(false);
            // }


            // Create a property field for each property in the biome
            // SerializedProperty iterator = biomeElement.Copy();
            // bool hasNext = iterator.NextVisible(true);
            // while (hasNext)
            // {
            //     if (SerializedProperty.EqualContents(iterator, biomesProperty.GetArrayElementAtIndex(i)))
            //         break;

            //     PropertyField propertyField = new PropertyField(iterator.Copy(), iterator.displayName);
            //     biomeFoldout.Add(propertyField);
            //     hasNext = iterator.NextVisible(false);
            // }
        }

        // Return the finished inspector UI
        return root;
    }
}

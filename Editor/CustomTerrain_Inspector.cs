using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic; 

[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrain_Inspector : Editor
{
    public VisualTreeAsset m_InspectorXML;

    //Dictionary for biome dropdown
    private Dictionary<string, (string heightmap, string texture)> biomePresets = new Dictionary<string, (string, string)>
    {
        { "Desert", ("Desert_Heightmap", "Sand") },
        { "Hills", ("Hills_Heightmap", "Grass") },
        { "Plains", ("Plains_Heightmap", "Grass") },
        { "Mountain", ("Mountain_Heightmap", "Stone") },
        { "Valley", ("Valley_Heightmap", "Grass") },
        { "Custom", ("Flat0", "Grass") }
    };

    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement root = new VisualElement();
        root.AddToClassList("customInspectorRoot");

        // Access the CustomTerrain target object
        CustomTerrain terrain = (CustomTerrain)target;

        // Biome Selection Dropdown
        var biomeDropdown = new PopupField<string>("New Biome", new List<string>(biomePresets.Keys), 0);

        biomeDropdown.RegisterValueChangedCallback(evt =>
        {
            // evt.newValue contains the newly selected option as a string
            Debug.Log("Selected biome: " + evt.newValue);
            // Here you can handle the selection change. For example, updating a property or variable.
        });

        // Add Biome Button
        Button addBiomeButton = new Button(() =>
        {
            string selectedBiomeName = biomeDropdown.value;
            if (biomePresets.TryGetValue(selectedBiomeName, out var preset))
            {
                // Access the CustomTerrain target object
                CustomTerrain terrain = (CustomTerrain)target;

                // Assuming Biome is a class you can instantiate and has SetHeightMap and SetTexture methods
                Biome newBiome = new Biome();

                // Load the assets based on the preset names
                HeightmapBase heightmap = Resources.Load<HeightmapBase>(preset.heightmap);
                Texture2D texture = Resources.Load<Texture2D>(preset.texture);

                // Set the properties on the new biome
                // This assumes the existence of such methods or properties to set these values
                newBiome.SetName("Change my name.");
                newBiome.SetHeightMap(heightmap);
                newBiome.SetTexture(texture);

                // Add the new biome to the terrain
                terrain.AddBiome(newBiome);

                // Mark the terrain object as dirty to ensure changes are saved
                EditorUtility.SetDirty(terrain);
            }
            else
            {
                Debug.LogError("Unrecognized Biome Option");
            }
        })
        {
            text = "Add Biome"
        };
        

        SerializedProperty biomesProperty = serializedObject.FindProperty("_biomes");
        for (int i = 0; i < biomesProperty.arraySize; i++)
        {
            //Find each properties of a biome
            SerializedProperty biomeElement = biomesProperty.GetArrayElementAtIndex(i);
            SerializedProperty nameProperty = biomeElement.FindPropertyRelative("_name");
            SerializedProperty heightmapProperty = biomeElement.FindPropertyRelative("_heightmap");
            SerializedProperty textureProperty = biomeElement.FindPropertyRelative("_texture");

            //Create Main Foldout for a Biome
            Foldout biomeFoldout = new Foldout();
            biomeFoldout.text = string.IsNullOrEmpty(nameProperty.stringValue) ? "Biome " + i : nameProperty.stringValue;
            biomeFoldout.AddToClassList("biomeFoldout");
            root.Add(biomeFoldout);


            //GUI for nameProperty
            TextField nameField = new TextField("Biome Name")
            {
                value = nameProperty.stringValue
            };
            nameField.RegisterValueChangedCallback(evt =>
            {
                nameProperty.stringValue = evt.newValue;
                biomeElement.serializedObject.ApplyModifiedProperties();
                Debug.Log($"Biome name changed to: {nameProperty.stringValue}");
                biomeFoldout.text = string.IsNullOrEmpty(evt.newValue) ? "Biome " + i : evt.newValue;
            });

            //GUI for Delete Button
            Button deleteButton = new Button(() =>
            {
                CustomTerrain terrain = (CustomTerrain)target;
                terrain.DeleteBiome(i); 
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(terrain);
            })
            {
                text = "delete",
            };
            deleteButton.style.width = 100;

            //GUI for each properties of heightmapProperty
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
                // heightmapFoldout.Add(propertyField);
            }

            //Add Element to Biome Foldout
            biomeFoldout.Add(nameField); 
            biomeFoldout.Add(heightmapFoldout);
            biomeFoldout.Add(deleteButton);
            // biomeFoldout.Add(texture);
        }
        root.Add(biomeDropdown);
        root.Add(addBiomeButton);
        // Return the finished inspector UI
        return root;
    }
}

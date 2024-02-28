using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace WorldGenerator
{
    [CustomEditor(typeof(CustomTerrain))]
    public class CustomTerrain_Inspector : Editor
    {
        public VisualTreeAsset m_InspectorXML;

        //Dictionary for biomes dropdown
        private Dictionary<string, (string heightmap, string texture)> biomePresets = new Dictionary<string, (string, string)>
    {
        { "Desert", ("Desert_Heightmap", "Sand") },
        { "Hills", ("Hills_Heightmap", "Grass") },
        { "Plains", ("Plains_Heightmap", "Grass") },
        { "Mountain", ("Mountain_Heightmap", "Stone") },
        { "Valley", ("Valley_Heightmap", "Grass") },
        { "Custom", ("Flat0", "Grass") }
    };

        //Dictionary for texture dropdown
        private Dictionary<string, string> texturePresets = new Dictionary<string, string>
    {
        { "Sand", "Sand" },
        { "Grass", "Grass" },
        { "Stone", "Stone" },
        { "Metallic", "Metallic" },
        { "Import Custom", "Custom" }
    };

        //Dictionary for Skybox dropdown
        private Dictionary<string, string> skyboxPresets = new Dictionary<string, string>
    {
        { "Cloudy", "Cloudy" },
        { "Sunny", "Sunny" },
        { "Import Custom", "Custom" }
    };

        private Dictionary<string, string> biomeFeaturePresets = new Dictionary<string, string>
    {
        { "Trees", "Trees" },
        { "Rocks", "Rocks" },
        { "Rivers", "Rivers" },
        { "Import Custom", "Custom" }
    };

    //Refresh GUI and generate terrain in real-time
    private void UpdateUI (VisualElement root, CustomTerrain terrain) 
    {
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        root.Clear();
        BuildUI(root);
        // terrain.GenerateTerrain();
    }

        private void BuildUI(VisualElement root)
        {
            //Access the CustomTerrain target object
            CustomTerrain terrain = (CustomTerrain)target;

            //Biome Selection Dropdown
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
                // Assuming Biome is a class you can instantiate and has SetHeightMap and SetTexture methods
                Biome newBiome = new Biome();

                    string biomeId = System.Guid.NewGuid().ToString();
                    Debug.Log(biomeId);
                    newBiome.SetBiomeId(biomeId);

                    // Load the assets based on the preset names
                    HeightmapBase heightmap = Resources.Load<HeightmapBase>(preset.heightmap);
                    Texture2D texture = Resources.Load<Texture2D>(preset.texture);

                    // Set the properties on the new biome
                    newBiome.SetName("Change my name.");
                    newBiome.SetHeightMap(heightmap);
                    newBiome.SetTexture(texture);

                    // Add the new biome to the terrain
                    terrain.AddBiome(newBiome);

                UpdateUI(root, terrain);
            }
            else
            {
                Debug.LogError("Unrecognized Biome Option");
            }
        })
        {
            text = "Add Biome"
        };

        //GUI for Generate Terrain Button
        Button generateButton = new Button(() =>
        {
            terrain.GenerateTerrain();
        })
        {
            text = "Generate Terrain",
        };

            SerializedProperty biomesProperty = serializedObject.FindProperty("_biomes");
            for (int i = 0; i < biomesProperty.arraySize; i++)
            {
                //Find each properties of a biome
                SerializedProperty biomeElement = biomesProperty.GetArrayElementAtIndex(i);
                SerializedProperty biomeIdProperty = biomeElement.FindPropertyRelative("_biomeId");
                string biomeId = biomeIdProperty.stringValue;
                SerializedProperty nameProperty = biomeElement.FindPropertyRelative("_name");
                SerializedProperty heightmapProperty = biomeElement.FindPropertyRelative("_heightmap");
                SerializedProperty textureProperty = biomeElement.FindPropertyRelative("_texture");

                //Create Main Foldout for a Biome
                Foldout biomeFoldout = new Foldout();
                biomeFoldout.text = string.IsNullOrEmpty(nameProperty.stringValue) ? "Biome " + i : nameProperty.stringValue;
                biomeFoldout.AddToClassList("biomeFoldout");

                //Set default status as fold and add biomes foldout to the root
                biomeFoldout.value = false;
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
                terrain.DeleteBiome(biomeId);
                UpdateUI(root, terrain);
            })
            {
                text = "Delete Biome",
            };

                //GUI for each properties of heightmapProperty
                Foldout heightmapFoldout = new Foldout();
                heightmapFoldout.text = "Heightmap";
                heightmapFoldout.value = false;
                SerializedObject heightmapSerializedObject = new SerializedObject(heightmapProperty.objectReferenceValue);
                SerializedProperty iterator = heightmapSerializedObject.GetIterator();

                while (iterator.NextVisible(true))
                {
                    if (iterator.name == "m_Script")
                    {
                        continue;
                    }
                    // Create UI elements for each heightmap property here
                    Debug.Log(iterator.Copy().displayName);
                    PropertyField propertyField = new PropertyField(iterator.Copy());
                    switch (iterator.propertyType)
                    {
                        case SerializedPropertyType.Float:
                            // Define the range of slider
                            float minValue = 0f; //minimum value
                            float maxValue = 100f; //maximum value


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
                }

                //GUI for Texture
                var textureDropdown = new PopupField<string>("Texture", new List<string>(texturePresets.Keys), 0);
                textureDropdown.RegisterValueChangedCallback(evt =>
                {
                    // Placeholder for future functionality
                    Debug.Log($"Selected texture for Biome {i + 1}: {evt.newValue}");
                });

                //GUI for skybox
                var skyboxDropdown = new PopupField<string>("Skybox", new List<string>(skyboxPresets.Keys), 0);
                skyboxDropdown.RegisterValueChangedCallback(evt =>
                {
                    // Placeholder for future functionality
                    Debug.Log($"Selected skybox for Biome {i + 1}: {evt.newValue}");
                });

                //GUI for Size
                var sizeSlider = new Slider("Size", 0, 100) //Assuming a range of 0 to 100 for size
                {
                    value = 50 //Default value
                };

                //GUI for Frequency
                var frequencySlider = new Slider("Frequency", 0, 100) //Assuming a range of 0 to 100 for frequency
                {
                    value = 20 //Default value
                };

                //GUI for Biome Feature foldout
                Foldout biomeFeatureFoldout = new Foldout()
                {
                    text = "Biome Feature",
                    value = false
                };

                //GUI for New Feature dropdown
                var newFeatureDropdown = new PopupField<string>("New Feature", new List<string>(biomeFeaturePresets.Keys), 0);
                newFeatureDropdown.RegisterValueChangedCallback(evt =>
                {
                    // Placeholder for future functionality
                    Debug.Log($"Selected new feature for Biome {i + 1}: {evt.newValue}");
                });

                //GUI for Add Feature Button
                Button addFeatureButton = new Button(() =>
                {
                    //Get the selected feature name
                    string selectedFeatureName = newFeatureDropdown.value;

                    //Create a new foldout for the selected feature
                    Foldout newFeatureFoldout = new Foldout
                    {
                        text = selectedFeatureName,
                        value = true
                    };

                    //Add elements to the new feature foldout here
                    //Add "Feature Size" slider
                    var featureSizeSlider = new Slider("Feature Size", 0, 100)
                    {
                        value = 50
                    };
                    newFeatureFoldout.Add(featureSizeSlider);

                    //Add "Feature Frequency" slider
                    var featureFrequencySlider = new Slider("Feature Frequency", 0, 100)
                    {
                        value = 20
                    };
                    newFeatureFoldout.Add(featureFrequencySlider);

                    // Add the new feature foldout to the biomeFeaturesFoldout
                    biomeFeatureFoldout.Add(newFeatureFoldout);
                })
                {
                    text = "Add Feature"
                };

                //Add Element to Biome Feature foldout
                biomeFeatureFoldout.Add(newFeatureDropdown);
                biomeFeatureFoldout.Add(addFeatureButton);

                //Styling for each elements in the biome foldout
                heightmapFoldout.style.marginTop = 5;
                heightmapFoldout.style.marginBottom = 5;
                textureDropdown.style.marginTop = 5;
                skyboxDropdown.style.marginTop = 5;
                sizeSlider.style.marginTop = 5;
                frequencySlider.style.marginTop = 5;
                biomeFeatureFoldout.style.marginTop = 5;
                deleteButton.style.marginTop = 10;
                addFeatureButton.style.marginBottom = 10;
                addFeatureButton.style.width = 100;

                //Add Element to Biome Foldout
                biomeFoldout.Add(nameField);
                biomeFoldout.Add(heightmapFoldout);
                biomeFoldout.Add(textureDropdown);
                biomeFoldout.Add(skyboxDropdown);
                biomeFoldout.Add(sizeSlider);
                biomeFoldout.Add(frequencySlider);
                biomeFoldout.Add(biomeFeatureFoldout);
                biomeFoldout.Add(deleteButton);
                // biomeFoldout.Add(texture);
            }

            //Styling for each elements outside of biome foldout
            biomeDropdown.style.marginTop = 10;

        //Add elements to the root
        root.Add(biomeDropdown);
        root.Add(addBiomeButton);
        root.Add(generateButton);
    }

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our inspector UI
            VisualElement root = new VisualElement();
            root.AddToClassList("customInspectorRoot");
            BuildUI(root);
            return root;
        }
    }
}
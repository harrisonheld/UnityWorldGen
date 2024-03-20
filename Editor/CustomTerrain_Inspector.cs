using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace WorldGenerator
{
    [CustomEditor(typeof(CustomTerrain))]
    public class CustomTerrain_Inspector : Editor
    {
        public VisualTreeAsset m_InspectorXML;

        //Dictionary for biomes dropdown
        private Dictionary<string, (string heightmap, string texture)> biomePresets = new Dictionary<string, (string, string)>
        {
           { "Desert", ("DesertHeightmap", "Sand") },
            { "Hills", ("HillsHeightmap", "Grass") },
            { "Plains", ("plains_simplex_heightmap", "Grass") },
            { "Mountain", ("MountainHeightmap", "Stone") },
            { "Valley", ("valley_simplex_heightmap", "Grass") },
            { "Custom", ("Flat0", "Grass") }
        };

        //Dictionary for texture dropdown
        private Dictionary<string, string> texturePresets = new Dictionary<string, string>
        {
            { "Import Custom", "Custom" },
            { "Sand", "Sand" },
            { "Grass", "grass" },
            { "Stone", "Stone" },
        };

        //Dictionary for Skybox dropdown
        private Dictionary<string, string > skyboxPresets = new Dictionary<string, string>
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

            SerializedProperty worldSeedStringProperty = serializedObject.FindProperty("_worldSeedString");
            SerializedProperty chunkSizeProperty = serializedObject.FindProperty("_chunkSize");
            SerializedProperty chunkResolutionProperty = serializedObject.FindProperty("_chunkResolution");
            SerializedProperty biomesPerChunkProperty = serializedObject.FindProperty("_biomesPerChunk");

            //World Seeds
            TextField worldSeeds = new TextField("World Seed")
            {
                value = worldSeedStringProperty.stringValue
            };
            worldSeeds.RegisterValueChangedCallback(evt =>
            {
                worldSeedStringProperty.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            // Create sliders and int fields for TerrainSize, BiomesFrequency and BiomesResolution
            var sizeSlider = new Slider("Terrain Size", 10, 1000) { value = chunkSizeProperty.intValue };
            var sizeField = new IntegerField { value = chunkSizeProperty.intValue };

            var frequencySlider = new Slider("Biomes Frequency", 1, 10) { value = biomesPerChunkProperty.intValue };
            var frequencyField = new IntegerField { value = biomesPerChunkProperty.intValue };

            var resolutionSlider = new Slider("Biomes Resolution", 2, 250) { value = chunkResolutionProperty.intValue };
            var resolutionField = new IntegerField { value = chunkResolutionProperty.intValue };
            
            // Sync slider with int field
            void SyncSliderAndField(Slider slider, IntegerField field, SerializedProperty property, bool enforceEven = false) {
                slider.RegisterValueChangedCallback(evt => {
                    int newValue = (int)evt.newValue;
                    if (enforceEven) {
                        newValue -= newValue % 2;
                    }
                    property.intValue = newValue;
                    field.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                });
                field.RegisterValueChangedCallback(evt => {
                    int newValue = evt.newValue;
                    if (enforceEven) {
                        newValue -= newValue % 2;
                    }
                    property.intValue = newValue;
                    slider.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            // Apply synchronization
            SyncSliderAndField(sizeSlider, sizeField, chunkSizeProperty, true);
            SyncSliderAndField(frequencySlider, frequencyField, biomesPerChunkProperty);
            SyncSliderAndField(resolutionSlider, resolutionField, chunkResolutionProperty);

            //Add elements to the root on the top
            root.Add(worldSeeds);
            root.Add(sizeSlider);
            root.Add(sizeField); 
            root.Add(frequencySlider);
            root.Add(frequencyField);
            root.Add(resolutionSlider);
            root.Add(resolutionField);


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
                    newBiome.SetName(selectedBiomeName + " (ID: " + biomeId + ")");
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
                SerializedProperty frequencyWeightProperty = biomeElement.FindPropertyRelative("_frequencyWeight");
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

                //GUI for frequencyWeightProperty
                var frequencyWeightSlider = new Slider("Frequency: ", 1, 1000)
                {
                    value = frequencyWeightProperty.intValue
                };
                var frequencyWeightIntField = new IntegerField
                {
                    value = frequencyWeightProperty.intValue
                };

                frequencyWeightSlider.RegisterValueChangedCallback(evt =>
                {
                    int newValue = (int)evt.newValue;
                    frequencyWeightProperty.intValue = newValue;
                    frequencyWeightIntField.value = newValue;
                    biomeElement.serializedObject.ApplyModifiedProperties();
                });

                frequencyWeightIntField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue >= 1 && evt.newValue <= 1000)
                    {
                        frequencyWeightProperty.intValue = evt.newValue;
                        frequencyWeightSlider.value = evt.newValue; 
                        biomeElement.serializedObject.ApplyModifiedProperties();
                    } else {
                        Debug.Log("Out of Range");
                    }
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
                    SerializedProperty currentProperty = iterator.Copy();
                    switch (currentProperty.propertyType)
                    {
                        case SerializedPropertyType.Float:
                            float minValue = 0f; // Minimum value
                            float maxValue = 100f; // Maximum value

                            var slider = new Slider(currentProperty.displayName, minValue, maxValue)
                            {
                                value = currentProperty.floatValue
                            };

                            var floatField = new FloatField
                            {
                                value = currentProperty.floatValue
                            };

                            slider.RegisterValueChangedCallback(evt =>
                            {
                                currentProperty.floatValue = evt.newValue;
                                floatField.value = evt.newValue; // Update the floatField when slider changed
                                currentProperty.serializedObject.ApplyModifiedProperties();
                            });

                            floatField.RegisterValueChangedCallback(evt =>
                            {
                                if (evt.newValue >= minValue && evt.newValue <= maxValue)
                                {
                                    currentProperty.floatValue = evt.newValue;
                                    slider.value = evt.newValue; // Update the slider when floatField changed
                                    currentProperty.serializedObject.ApplyModifiedProperties();
                                } else {
                                    Debug.Log("Out of Range");
                                }
                            });

                            heightmapFoldout.Add(slider);
                            heightmapFoldout.Add(floatField);
                            break;
                        
                        default:
                            var label = new Label($"{currentProperty.displayName}: {currentProperty.propertyType} not supported");
                            heightmapFoldout.Add(label);
                            break;
                    }
                }

                //GUI for Texture
                Texture2D currentTexture = terrain.GetBiome(biomeId).GetTexture();
                //Handle the default Selection for Texture GUI
                string currentTexturePath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentTexture));
                string currentTextureName = texturePresets.FirstOrDefault(x => x.Value == currentTexturePath).Key;
                int defaultTextureIndex = currentTextureName != null ? new List<string>(texturePresets.Keys).IndexOf(currentTextureName) : 0;
                var textureDropdown = new PopupField<string>("Texture", new List<string>(texturePresets.Keys), defaultTextureIndex);
                PropertyField textureField = new PropertyField(textureProperty, "Import Texture: ");
                if (textureDropdown.value == "Import Custom") {
                    textureField.style.display = DisplayStyle.Flex;
                } else {
                    textureField.style.display = DisplayStyle.None; 
                }
                //Texture dropdown
                textureDropdown.RegisterValueChangedCallback(evt =>
                {
                    Debug.Log($"Selected texture for Biome {biomeId}: {evt.newValue}");
                    string selectedTextureName = evt.newValue;              
                    if (selectedTextureName == "Import Custom")
                    {
                        textureField.style.display = DisplayStyle.Flex;
                    } 
                    else 
                    { 
                        textureField.style.display = DisplayStyle.None; 
                        var biome = terrain.GetBiome(biomeId);
                        string texturePath = texturePresets[selectedTextureName];
                        Texture2D texture = Resources.Load<Texture2D>(texturePath);
                        biome.SetTexture(texture);
                    }
                });
                

                //GUI for skybox
                var skyboxDropdown = new PopupField<string>("Skybox", new List<string>(skyboxPresets.Keys), 0);
                skyboxDropdown.RegisterValueChangedCallback(evt =>
                {
                    // Placeholder for future functionality
                    Debug.Log($"Selected skybox for Biome {i + 1}: {evt.newValue}");
                });

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
                biomeFeatureFoldout.style.marginTop = 5;
                deleteButton.style.marginTop = 5;
                addFeatureButton.style.marginBottom = 10;
                addFeatureButton.style.width = 100;

                //Add Element to Biome Foldout
                
                biomeFoldout.Add(nameField);
                biomeFoldout.Add(frequencyWeightSlider);
                biomeFoldout.Add(frequencyWeightIntField);
                biomeFoldout.Add(heightmapFoldout);
                biomeFoldout.Add(textureDropdown);
                biomeFoldout.Add(textureField);
                biomeFoldout.Add(skyboxDropdown);
                biomeFoldout.Add(biomeFeatureFoldout);
                biomeFoldout.Add(deleteButton);
                // biomeFoldout.Add(texture);
            }

            //Styling for each elements outside of biome foldout
            sizeSlider.style.marginTop = 5;
            resolutionSlider.style.marginTop = 5;
            frequencySlider.style.marginTop = 5;
            resolutionField.style.marginBottom = 20;
            biomeDropdown.style.marginTop = 20;

            //Add elements to the root on the bottom
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
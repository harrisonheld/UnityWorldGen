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

        // presets for biomes dropdown
        private Dictionary<string, (string heightmap, string texture)> biomePresets = new Dictionary<string, (string, string)>
        {
            { "Desert", ("DesertHeightmap", "Sand") },
            { "Hills", ("HillsHeightmap", "Grass") },
            { "Plains", ("plains_simplex_heightmap", "Grass") },
            { "Mountain", ("MountainHeightmap", "Stone") },
            { "Valley", ("valley_simplex_heightmap", "Grass") },
            { "Custom", ("Flat0", "Grass") }
        };

        // presets for heightmaps dropdown
        private Dictionary<string, string> heightmapPresets = new Dictionary<string, string>
        {
            { "Desert", "DesertHeightmap" },
            { "Hills", "HillsHeightmap" },
            { "Plains", "plains_simplex_heightmap" },
            { "Mountain", "MountainHeightmap" },
            { "Valley", "valley_simplex_heightmap" },
            { "Import Custom", "Custom" }
        };

        // presets for texture dropdown
        private Dictionary<string, string> texturePresets = new Dictionary<string, string>
        {
            { "Sand", "Sand" },
            { "Grass", "grass" },
            { "Stone", "Stone" },
            { "Import Custom", "Custom" }
        };

        // presets for skybox dropdown
        private Dictionary<string, string> skyboxPresets = new Dictionary<string, string>
        {
            { "Cloudy", "Cloudy" },
            { "Sunny", "Sunny" },
            { "Import Custom", "Custom" }
        };

        // presets for features dropdown
        private Dictionary<string, string> biomeFeaturePresets = new Dictionary<string, string>
        {
            { "Trees", "tree" },
            { "Rocks", "horse" },
            { "Rivers", "tree_b" },
            { "Import Custom", "Custom" }
        };

        // refresh GUI after changes
        private void UpdateUI(VisualElement root, CustomTerrain terrain)
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            root.Clear();
            BuildUI(root);
            // for updating in real time
            // terrain.GenerateTerrain();
        }

        // builds all aspects of the UI
        private void BuildUI(VisualElement root)
        {
            /*
                WORLD OPTIONS
            */

            // access the CustomTerrain target object and get all of its properties
            CustomTerrain terrain = (CustomTerrain)target;

            SerializedProperty drawGizmosProperty = serializedObject.FindProperty("_drawChunkGizmos");
            SerializedProperty worldSeedStringProperty = serializedObject.FindProperty("_worldSeedString");
            SerializedProperty featureSeedStringProperty = serializedObject.FindProperty("_featureSeedString");
            SerializedProperty chunkSizeProperty = serializedObject.FindProperty("_chunkSize");
            SerializedProperty chunkResolutionProperty = serializedObject.FindProperty("_chunkResolution");
            SerializedProperty biomesPerChunkProperty = serializedObject.FindProperty("_biomesPerChunk");

            /*
                DEBUG SECTION
            */
            Label debugSectionLabel = new Label("Debug Options");
            debugSectionLabel.AddToClassList("h1");
            root.Add(debugSectionLabel);

            Box debugSection = new Box();
            var drawGizmosField = new Toggle("Draw Chunk Gizmos")
            {
                value = drawGizmosProperty.boolValue
            };
            drawGizmosField.RegisterValueChangedCallback(evt =>
            {
                drawGizmosProperty.boolValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            debugSection.Add(drawGizmosField);
            root.Add(debugSection);

            /* 
                WORLD OPTIONS SECTION
            */
            Label worldOptSectionLabel = new Label("World Options");
            worldOptSectionLabel.AddToClassList("h1");
            root.Add(worldOptSectionLabel);

            Box worldOptSection = new Box();

            TextField worldSeed = new TextField("World Seed")
            {
                value = worldSeedStringProperty.stringValue
            };
            worldSeed.RegisterValueChangedCallback(evt =>
            {
                worldSeedStringProperty.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            TextField featureSeed = new TextField("Feature Seed")
            {
                value = featureSeedStringProperty.stringValue
            };
            featureSeed.RegisterValueChangedCallback(evt =>
            {
                featureSeedStringProperty.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            // create sliders and int fields for TerrainSize, BiomesFrequency and BiomesResolution
            var sizeSlider = new Slider(10, 1000) { value = chunkSizeProperty.intValue };
            var sizeField = new IntegerField { value = chunkSizeProperty.intValue };
            VisualElement sizeContainer = GroupSliderAndInt(sizeSlider, sizeField, "Terrain Size");

            var frequencySlider = new Slider(1, 10) { value = biomesPerChunkProperty.intValue };
            var frequencyField = new IntegerField { value = biomesPerChunkProperty.intValue };
            VisualElement frequencyContainer = GroupSliderAndInt(frequencySlider, frequencyField, "Biomes Frequency");

            var resolutionSlider = new Slider(2, 250) { value = chunkResolutionProperty.intValue };
            var resolutionField = new IntegerField { value = chunkResolutionProperty.intValue };
            VisualElement resolutionContainer = GroupSliderAndInt(resolutionSlider, resolutionField, "Chunk Resolution");

            // sync slider with int field
            void SyncSliderAndField(Slider slider, IntegerField field, SerializedProperty property, bool enforceEven = false)
            {
                slider.RegisterValueChangedCallback(evt =>
                {
                    int newValue = (int)evt.newValue;
                    if (enforceEven)
                    {
                        newValue -= newValue % 2;
                    }
                    property.intValue = newValue;
                    field.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                });
                field.RegisterValueChangedCallback(evt =>
                {
                    int newValue = evt.newValue;
                    if (enforceEven)
                    {
                        newValue -= newValue % 2;
                    }
                    property.intValue = newValue;
                    slider.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            // apply synchronization
            SyncSliderAndField(sizeSlider, sizeField, chunkSizeProperty, true);
            SyncSliderAndField(frequencySlider, frequencyField, biomesPerChunkProperty);
            SyncSliderAndField(resolutionSlider, resolutionField, chunkResolutionProperty);

            // add elements to the root on the top
            worldSeed.AddToClassList("options-field");
            featureSeed.AddToClassList("options-field");
            sizeContainer.AddToClassList("options-field");
            frequencyContainer.AddToClassList("options-field");
            resolutionContainer.AddToClassList("options-field");
//
            worldOptSection.Add(worldSeed);
            worldOptSection.Add(featureSeed);
            worldOptSection.Add(sizeContainer);
            worldOptSection.Add(frequencyContainer);
            worldOptSection.Add(resolutionContainer);

            root.Add(worldOptSection);

            /*
                ADD BIOME SECTION
            */
            Label addBiomeSectionLabel = new Label("Add Biome");
            addBiomeSectionLabel.AddToClassList("h1");
            root.Add(addBiomeSectionLabel);

            Box addBiomeSection = new Box();

            var biomeDropdown = new PopupField<string>("New Biome", new List<string>(biomePresets.Keys), 0);
            biomeDropdown.RegisterValueChangedCallback(evt =>
            {
                // evt.newValue contains the newly selected option as a string
                Debug.Log("Selected biome: " + evt.newValue);
                // Here you can handle the selection change. For example, updating a property or variable.
            });
            biomeDropdown.AddToClassList("add-biome-field");
//
            // create add biome button
            Button addBiomeButton = new Button(() =>
            {
                string selectedBiomeName = biomeDropdown.value;
                if (biomePresets.TryGetValue(selectedBiomeName, out var preset))
                {
                    Biome newBiome = new Biome();

                    string biomeId = System.Guid.NewGuid().ToString();
                    newBiome.SetBiomeId(biomeId);

                    // load the assets based on the preset names
                    HeightmapBase heightmap = Resources.Load<HeightmapBase>(preset.heightmap);
                    Texture2D texture = Resources.Load<Texture2D>(preset.texture);

                    // set the properties on the new biome
                    newBiome.SetName(selectedBiomeName);
                    newBiome.SetHeightMap(heightmap);
                    newBiome.SetTexture(texture);

                    // add the new biome to the terrain
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

            addBiomeSection.Add(biomeDropdown);
            addBiomeSection.Add(addBiomeButton);
            root.Add(addBiomeSection);

            /*
                BIOMES SECTION
            */
            Label biomesSectionLabel = new Label("Biomes");
            biomesSectionLabel.AddToClassList("h1");
            root.Add(biomesSectionLabel);

            // Box biomesSection = new Box();

            // for each biome, add its properties to the GUI
            SerializedProperty biomesProperty = serializedObject.FindProperty("_biomes");
            for (int i = 0; i < biomesProperty.arraySize; i++)
            {
                // get current biome
                SerializedProperty biomeElement = biomesProperty.GetArrayElementAtIndex(i);
                string biomeId = biomeElement.FindPropertyRelative("_biomeId").stringValue;

                // get properties of biome
                SerializedProperty nameProperty = biomeElement.FindPropertyRelative("_name");
                SerializedProperty heightmapProperty = biomeElement.FindPropertyRelative("_heightmap");
                SerializedProperty textureProperty = biomeElement.FindPropertyRelative("_texture");
                SerializedProperty frequencyWeightProperty = biomeElement.FindPropertyRelative("_frequencyWeight");
                SerializedProperty featuresProperty = biomeElement.FindPropertyRelative("_features");

                // create main foldout for biome
                Foldout biomeFoldout = new Foldout()
                {
                    text = string.IsNullOrEmpty(nameProperty.stringValue) ? "Biome " + i : nameProperty.stringValue,
                    value = false
                };
                biomeFoldout.AddToClassList("biome-foldout");

                // biomesSection.Add(biomeFoldout);

                Box biomeProperties = new Box();

                /*
                    BIOME NAME
                */
                TextField nameField = new TextField("Biome Name") { value = nameProperty.stringValue };

                nameField.RegisterValueChangedCallback(evt =>
                {
                    nameProperty.stringValue = evt.newValue;
                    biomeElement.serializedObject.ApplyModifiedProperties();
                    Debug.Log($"Biome name changed to: {nameProperty.stringValue}");
                    biomeFoldout.text = string.IsNullOrEmpty(evt.newValue) ? "Biome " + i : evt.newValue;
                });
                nameField.AddToClassList("biome-field");
                biomeProperties.Add(nameField);

                /*
                    TEXTURE
                */
                Texture2D currentTexture = terrain.GetBiome(biomeId).GetTexture();
                string currentTexturePath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentTexture));
                string currentTextureName = texturePresets.FirstOrDefault(x => x.Value == currentTexturePath).Key;
                int defaultTextureIndex = currentTextureName != null ? new List<string>(texturePresets.Keys).IndexOf(currentTextureName) : 0;
                var textureDropdown = new PopupField<string>("Texture", new List<string>(texturePresets.Keys), defaultTextureIndex);
                PropertyField textureField = new PropertyField(textureProperty, "Custom Texture");
                if (textureDropdown.value == "Import Custom")
                {
                    textureField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    textureField.style.display = DisplayStyle.None;
                }

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

                VisualElement textureContainer = new VisualElement();
                textureContainer.Add(textureDropdown);
                textureContainer.Add(textureField);
                textureContainer.AddToClassList("biome-field");

                biomeProperties.Add(textureContainer);

                /*
                    SKYBOX
                */
                var skyboxDropdown = new PopupField<string>("Skybox", new List<string>(skyboxPresets.Keys), 0);
                skyboxDropdown.RegisterValueChangedCallback(evt =>
                {
                    // Placeholder for future functionality
                    Debug.Log($"Selected skybox for Biome {i + 1}: {evt.newValue}");
                });
                skyboxDropdown.AddToClassList("biome-field");
                biomeProperties.Add(skyboxDropdown);

                /*
                    FREQUENCY
                */
                var frequencyWeightSlider = new Slider(1, 1000)
                {
                    value = frequencyWeightProperty.intValue
                };
                var frequencyWeightIntField = new IntegerField
                {
                    value = frequencyWeightProperty.intValue
                };

                VisualElement frequencyWeightContainer = GroupSliderAndInt(frequencyWeightSlider, frequencyWeightIntField, "Frequency");
                frequencyWeightContainer.AddToClassList("biome-field");

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

                biomeProperties.Add(frequencyWeightContainer);

                /*  
                    HEIGHTMAP
                */
                Foldout heightmapFoldout = new Foldout()
                {
                    text = "Heightmap",
                    value = false
                };
                heightmapFoldout.AddToClassList("heightmap-foldout");
                heightmapFoldout.AddToClassList("biome-field");

                Box heightmapProperties = new Box();

                // add heightmaps dropdown
                HeightmapBase currentHeightmap = terrain.GetBiome(biomeId).GetHeightmap();
                string currentHeightmapPath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentHeightmap));
                string currentHeightmapName = heightmapPresets.FirstOrDefault(x => x.Value == currentHeightmapPath).Key;
                int defaultHeightmapIndex = currentHeightmapName != null ? new List<string>(heightmapPresets.Keys).IndexOf(currentHeightmapName) : 0;
                var heightmapDropdown = new PopupField<string>("Type", new List<string>(heightmapPresets.Keys), defaultHeightmapIndex);
                PropertyField heightmapField = new PropertyField(heightmapProperty, "Custom Heightmap");
                if (heightmapDropdown.value == "Import Custom")
                {
                    heightmapField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    heightmapField.style.display = DisplayStyle.None;
                }

                heightmapDropdown.RegisterValueChangedCallback(evt =>
                {
                    Debug.Log($"Selected heightmap for Biome {biomeId}: {evt.newValue}");
                    string selectedHeightmapName = evt.newValue;
                    if (selectedHeightmapName == "Import Custom")
                    {
                        heightmapField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        heightmapField.style.display = DisplayStyle.None;
                        var biome = terrain.GetBiome(biomeId);
                        string heightmapPath = heightmapPresets[selectedHeightmapName];
                        HeightmapBase heightmap = Resources.Load<HeightmapBase>(heightmapPath);
                        biome.SetHeightMap(heightmap);
                    }
                });

                VisualElement heightmapType = new VisualElement();
                heightmapType.Add(heightmapDropdown);
                heightmapType.Add(heightmapField);
                heightmapType.AddToClassList("heightmap-field");

                heightmapProperties.Clear();
                heightmapProperties.Add(heightmapType);
            
                // add other properties based on selected heightmap
                SerializedObject heightmapSerializedObject = new SerializedObject(heightmapProperty.objectReferenceValue);
                SerializedProperty iterator = heightmapSerializedObject.GetIterator();

                while (iterator.NextVisible(true))
                {
                    if (iterator.name == "m_Script")
                    {
                        continue;
                    }

                    // create UI elements for each heightmap property
                    SerializedProperty currentProperty = iterator.Copy();
                    switch (currentProperty.propertyType)
                    {
                        case SerializedPropertyType.Float:
                            float minValue = 0f;
                            float maxValue = 100f;

                            var slider = new Slider(minValue, maxValue) { value = currentProperty.floatValue };
                            var floatField = new FloatField { value = currentProperty.floatValue };

                            VisualElement sliderContainer = GroupSliderAndFloat(slider, floatField, currentProperty.displayName);
                            sliderContainer.AddToClassList("heightmap-field");

                            slider.RegisterValueChangedCallback(evt =>
                            {
                                currentProperty.floatValue = evt.newValue;
                                floatField.value = evt.newValue;
                                currentProperty.serializedObject.ApplyModifiedProperties();
                            });

                            floatField.RegisterValueChangedCallback(evt =>
                            {
                                if (evt.newValue >= minValue && evt.newValue <= maxValue)
                                {
                                    currentProperty.floatValue = evt.newValue;
                                    slider.value = evt.newValue;
                                    currentProperty.serializedObject.ApplyModifiedProperties();
                                }
                                else
                                {
                                    Debug.Log("Out of Range");
                                }
                            });

                            heightmapProperties.Add(sliderContainer);
                            break;

                        default:
                            var label = new Label($"{currentProperty.displayName}: {currentProperty.propertyType} not supported");
                            heightmapProperties.Add(label);
                            break;
                    }
                }
                heightmapFoldout.Add(heightmapProperties);
                biomeProperties.Add(heightmapFoldout);

                /* 
                    FEATURES
                */
                BuildBiomeFeaturesField(root, terrain, i, biomeId, featuresProperty, biomeProperties);

                /*
                    DELETE BIOME BUTTON
                */
                Button deleteButton = new Button(() =>
                {
                    terrain.DeleteBiome(biomeId);
                    UpdateUI(root, terrain);
                })
                {
                    text = "Delete Biome",
                };

                biomeFoldout.Add(biomeProperties);
                biomeFoldout.Add(deleteButton);
                root.Add(biomeFoldout);
            }

            // root.Add(biomesSection);

            /*
                GENERATE BUTTONS
            */
            // generate terrain button
            Button generateButton = new Button(() =>
            {
                terrain.GenerateTerrain();
            })
            {
                text = "Generate Terrain"
            };

            // regenerate features button
            Button regenerateFeatures = new Button(() =>
            {
                terrain.GenerateAllFeatures();
            })
            {
                text = "Regenerate Features"
            };

            root.Add(generateButton);
            root.Add(regenerateFeatures);
        }

        private void BuildBiomeFeaturesField(VisualElement root, CustomTerrain terrain, int i, string biomeId, SerializedProperty featuresProperty, Box biomeProperties)
        {
            Foldout biomeFeaturesFoldout = new Foldout()
            {
                text = "Features",
                value = false
            };

            biomeFeaturesFoldout.AddToClassList("features-foldout");
            biomeFeaturesFoldout.AddToClassList("biome-field");

            Box featuresProperties = new Box();


            /*
                INDIVIDUAL FEATURES
            */
            // for each feature, add its properties to the GUI
            for (int j = 0; j < featuresProperty.arraySize; j++)
            {
                // get current feature
                SerializedProperty featureElement = featuresProperty.GetArrayElementAtIndex(j);
                string featureId = featureElement.FindPropertyRelative("_featureId").stringValue;

                // get properties of biome
                SerializedProperty featureNameProperty = featureElement.FindPropertyRelative("Name");
                SerializedProperty featureFrequencyProperty = featureElement.FindPropertyRelative("Frequency");
                SerializedProperty featureScaleProperty = featureElement.FindPropertyRelative("Scale");
                SerializedProperty featureNormalProperty = featureElement.FindPropertyRelative("SetNormal");
                SerializedProperty featurePrefabProperty = featureElement.FindPropertyRelative("Prefab");

                Foldout featureFoldout = new Foldout()
                {
                    text = string.IsNullOrEmpty(featureNameProperty.stringValue) ? "Feature " + j : featureNameProperty.stringValue,
                    value = false
                };

                /*
                    FEATURE NAME
                */
                TextField featureNameField = new TextField("Feature Name")
                {
                    value = featureNameProperty.stringValue
                };

                featureNameField.RegisterValueChangedCallback(evt =>
                {
                    featureNameProperty.stringValue = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                    Debug.Log($"Feature name changed to: {featureNameProperty.stringValue}");
                    featureFoldout.text = string.IsNullOrEmpty(evt.newValue) ? "Feature " + j : evt.newValue;
                });

                /*
                    FREQUENCY
                */
                var featureFrequencySlider = new Slider("Frequency: ", 0, 100)
                {
                    value = featureFrequencyProperty.intValue
                };
                var featureFrequencyIntField = new IntegerField
                {
                    value = featureFrequencyProperty.intValue
                };

                featureFrequencySlider.RegisterValueChangedCallback(evt =>
                {
                    int newValue = (int)evt.newValue;
                    featureFrequencyProperty.intValue = newValue;
                    featureFrequencyIntField.value = newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                });

                featureFrequencyIntField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue >= 0 && evt.newValue <= 100)
                    {
                        featureFrequencyProperty.intValue = evt.newValue;
                        featureFrequencySlider.value = evt.newValue; 
                        featureElement.serializedObject.ApplyModifiedProperties();
                    } else {
                        Debug.Log("Out of Range");
                    }
                });

                /*
                    SCALE
                */
                var featureScaleField = new Vector3Field("Scale")
                {
                    value = featureScaleProperty.vector3Value
                };

                featureScaleField.RegisterValueChangedCallback(evt =>
                {
                    featureScaleProperty.vector3Value = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                });

                /*
                    NORMALITY
                */
                var featureNormalField = new Toggle("Set Normal")
                {
                    value = featureNormalProperty.boolValue
                };

                featureNormalField.RegisterValueChangedCallback(evt =>
                {
                    featureNormalProperty.boolValue = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                });

                /*
                    PREFAB
                */
                GameObject currentPrefab = terrain.GetBiome(biomeId).GetFeature(featureId).Prefab;
                string currentPrefabPath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentPrefab));
                string currentPrefabName = biomeFeaturePresets.FirstOrDefault(x => x.Value == currentPrefabPath).Key;
                int defaultPrefabIndex = currentPrefabName != null ? new List<string>(biomeFeaturePresets.Keys).IndexOf(currentPrefabName) : 0;
                var prefabDropdown = new PopupField<string>("Feature", new List<string>(biomeFeaturePresets.Keys), defaultPrefabIndex);
                PropertyField prefabField = new PropertyField(featurePrefabProperty, "Custom Feature");
                if (prefabDropdown.value == "Import Custom")
                {
                    prefabField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    prefabField.style.display = DisplayStyle.None;
                }

                prefabDropdown.RegisterValueChangedCallback(evt =>
                {
                    string selectedPrefabName = evt.newValue;
                    if (selectedPrefabName == "Import Custom")
                    {
                        prefabField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        prefabField.style.display = DisplayStyle.None;
                        var feature = terrain.GetBiome(biomeId).GetFeature(featureId);
                        string prefabPath = biomeFeaturePresets[selectedPrefabName];
                        GameObject prefab = Resources.Load<GameObject>(prefabPath);
                        feature.Prefab = prefab;
                    }
                });

                /*
                    DELETE FEATURE BUTTON
                */
                Button deleteFeatureButton = new Button(() =>
                {
                    terrain.GetBiome(biomeId).DeleteFeature(featureId);
                    UpdateUI(root, terrain);
                })
                {
                    text = "Delete Feature",
                };

                featureFoldout.Add(featureNameField);
                featureFoldout.Add(featureFrequencySlider);
                featureFoldout.Add(featureFrequencyIntField);
                featureFoldout.Add(featureScaleField);
                featureFoldout.Add(featureNormalField);
                featureFoldout.Add(prefabDropdown);
                featureFoldout.Add(prefabField);
                featureFoldout.Add(deleteFeatureButton);
                biomeFeaturesFoldout.Add(featureFoldout);
            }

            /*
                ADD FEATURE SECTION
            */
            var newFeatureDropdown = new PopupField<string>("New Feature", new List<string>(biomeFeaturePresets.Keys), 0);

            // add feature button
            Button addFeatureButton = new Button(() =>
            {
                string selectedFeatureName = newFeatureDropdown.value;

                BiomeFeature newFeature = new BiomeFeature();
                string featureId = System.Guid.NewGuid().ToString();
                
                newFeature.SetFeatureId(featureId);
                newFeature.Name = selectedFeatureName;
                string featurePath = biomeFeaturePresets[selectedFeatureName];
                GameObject featurePrefab = Resources.Load<GameObject>(featurePath);
                newFeature.Prefab = featurePrefab;

                terrain.GetBiome(biomeId).AddFeature(newFeature);
                UpdateUI(root, terrain);
            })
            {
                text = "Add Feature"
            };

            biomeFeaturesFoldout.Add(newFeatureDropdown);
            biomeFeaturesFoldout.Add(addFeatureButton);
            biomeProperties.Add(biomeFeaturesFoldout);
        }

        private VisualElement GroupSliderAndInt(Slider slider, IntegerField field, string label)
        {
            // create parent container
            VisualElement container = new VisualElement();
            Label containerLabel = new Label(label);

            // group fields
            VisualElement fields = new VisualElement();
            fields.Add(slider);
            fields.Add(field);
            fields.AddToClassList("slider-int-field");

            // add to parent container
            container.Add(containerLabel);
            container.Add(fields);

            container.AddToClassList("slider-int-container");

            return container;
        }

        private VisualElement GroupSliderAndFloat(Slider slider, FloatField field, string label)
        {
            // create parent container
            VisualElement container = new VisualElement();
            Label containerLabel = new Label(label);

            // group fields
            VisualElement fields = new VisualElement();
            fields.Add(slider);
            fields.Add(field);
            fields.AddToClassList("slider-float-field");

            // add to parent container
            container.Add(containerLabel);
            container.Add(fields);

            container.AddToClassList("slider-float-container");

            return container;
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our inspector UI
            VisualElement root = new VisualElement();

            // load stylesheet and add to root
            StyleSheet uss = Resources.Load<StyleSheet>("styles");
            root.styleSheets.Add(uss);
    
            root.AddToClassList("root");
            BuildUI(root);
            
            return root;
        }
    }
}

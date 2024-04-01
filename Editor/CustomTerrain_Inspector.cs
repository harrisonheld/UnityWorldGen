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

        private Dictionary<string, Foldout> foldoutStates = new Dictionary<string, Foldout>();

        // Foldout test = new Foldout();
        

        // presets for biomes dropdown
        private readonly Dictionary<string, (string heightmap, string texture, string skybox)> biomePresets = new Dictionary<string, (string, string, string)>
        {
            { "Desert",         ("Heightmaps/Desert",   "Textures/Sand",  "Skyboxes/Dusk"   ) },
            { "Hills",          ("Heightmaps/Hills",    "Textures/Grass", "Skyboxes/Default") },
            { "Plains",         ("Heightmaps/Plains",   "Textures/Grass", "Skyboxes/Sky"    ) },
            { "Mountain",       ("Heightmaps/Mountain", "Textures/Stone", "Skyboxes/Thin"   ) },
            { "Valley",         ("Heightmaps/Valley",   "Textures/Grass", "Skyboxes/Default") },
            { "Custom Biome",   ("Heightmaps/Flat",     "Textures/Grass", "Skyboxes/Default") }
        };

        // presets for heightmaps dropdown
        private readonly Dictionary<string, string> heightmapPresets = new Dictionary<string, string>
        {
            { "Desert",        "Heightmaps/Desert"   },
            { "Hills",         "Heightmaps/Hills"    },
            { "Plains",        "Heightmaps/Plains"   },
            { "Mountain",      "Heightmaps/Mountain" },
            { "Valley",        "Heightmaps/Valley"   },
            { "Import Custom", "Custom"              }
        };

        // presets for texture dropdown
        private readonly Dictionary<string, string> texturePresets = new Dictionary<string, string>
        {
            { "Sand",          "Textures/Sand"  },
            { "Grass",         "Textures/Grass" },
            { "Stone",         "Textures/Stone" },
            { "Import Custom", "Custom"         }
        };

        // presets for skybox dropdown
        private readonly Dictionary<string, string> skyboxPresets = new Dictionary<string, string>
        {
            { "Default",       "Skyboxes/Default" },
            { "Thin",          "Skyboxes/Thing"   },
            { "Dusk",          "Skyboxes/Dusk"    },
            { "Import Custom", "Custom"           }
        };

        // presets for features dropdown
        private readonly Dictionary<string, string> biomeFeaturePresets = new Dictionary<string, string>
        {
            { "Trees",         "Features/tree"  },
            { "Horses",        "Features/horse" },
            { "Import Custom", "Custom"         }
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
            VisualElement sizeContainer = GroupSliderAndInt(sizeSlider, sizeField, "Chunk Size");

            var resolutionSlider = new Slider(2, 250) { value = chunkResolutionProperty.intValue };
            var resolutionField = new IntegerField { value = chunkResolutionProperty.intValue };
            VisualElement resolutionContainer = GroupSliderAndInt(resolutionSlider, resolutionField, "Chunk Resolution");

            var frequencySlider = new Slider(1, 10) { value = biomesPerChunkProperty.intValue };
            var frequencyField = new IntegerField { value = biomesPerChunkProperty.intValue };
            VisualElement frequencyContainer = GroupSliderAndInt(frequencySlider, frequencyField, "Biomes Frequency");

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
            generateButton.AddToClassList("generate-button");

            // regenerate features button
            Button regenerateFeatures = new Button(() =>
            {
                terrain.GenerateAllFeatures();
            })
            {
                text = "Regenerate Features"
            };

            // export button
            Button exportButton = new Button(() =>
            {
                terrain.ExportChunks("Assets/ExportedTerrainChunks");
            })
            {
                text = "Export All Chunks"
            };


            VisualElement generateBtnsContainer = new VisualElement();
            generateBtnsContainer.Add(generateButton);
            generateBtnsContainer.Add(regenerateFeatures);
            generateBtnsContainer.Add(exportButton);
            generateBtnsContainer.AddToClassList("generate-buttons");

            // change class of generate buttons container depending on inspector width
            root.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (evt.newRect.width < 350) { generateBtnsContainer.AddToClassList("column"); }
                else { generateBtnsContainer.RemoveFromClassList("column"); }
            });

            // add elements to the root on the top
            worldSeed.AddToClassList("options-field");
            featureSeed.AddToClassList("options-field");
            sizeContainer.AddToClassList("options-field");
            frequencyContainer.AddToClassList("options-field");
            resolutionContainer.AddToClassList("options-field");

            worldOptSection.Add(worldSeed);
            worldOptSection.Add(featureSeed);
            worldOptSection.Add(sizeContainer);
            worldOptSection.Add(frequencyContainer);
            worldOptSection.Add(resolutionContainer);
            worldOptSection.Add(generateBtnsContainer);

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
                // Here you can handle the selection change. For example, updating a property or variable.
            });
            biomeDropdown.AddToClassList("add-biome-field");

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
                    Material skybox = Resources.Load<Material>(preset.skybox);

                    // set the properties on the new biome
                    newBiome.SetName(selectedBiomeName);
                    newBiome.SetHeightMap(heightmap);
                    newBiome.SetTexture(texture);
                    newBiome.SetSkybox(skybox);

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
                SerializedProperty skyboxProperty = biomeElement.FindPropertyRelative("_skybox");
                SerializedProperty frequencyWeightProperty = biomeElement.FindPropertyRelative("_frequencyWeight");
                SerializedProperty featuresProperty = biomeElement.FindPropertyRelative("_features");

                VisualElement biomeContainer = new VisualElement();
                biomeContainer.AddToClassList("biome-container");

                // create main foldout for biome
                Foldout biomeFoldout = new Foldout()
                {
                    text = string.IsNullOrEmpty(nameProperty.stringValue) ? "Unnamed Biome" : nameProperty.stringValue,
                    value = false
                };

                // if foldout already exists, load from dictionary
                if (foldoutStates.ContainsKey(biomeId)) 
                {
                    Debug.Log(biomeId);
                    biomeFoldout = foldoutStates[biomeId];
                    biomeFoldout.Clear();
                } 
                // if doesn't exist, add to dictionary
                else
                {
                    foldoutStates[biomeId] = biomeFoldout;
                }
                biomeFoldout.AddToClassList("biome-foldout");

                /*
                    DELETE BIOME BUTTON
                */
                Texture2D test = Resources.Load<Texture2D>("Sand");
                GUIContent buttonTest = new GUIContent("Delete Biome", test);

                Button deleteButton = new Button(() =>
                {
                    terrain.DeleteBiome(biomeId);
                    UpdateUI(root, terrain);
                });
                deleteButton.AddToClassList("delete-biome");

                biomeContainer.Add(biomeFoldout);
                biomeContainer.Add(deleteButton);


                Box biomeProperties = new Box();

                /*
                    BIOME NAME
                */
                TextField nameField = new TextField("Biome Name") { value = nameProperty.stringValue };

                nameField.RegisterValueChangedCallback(evt =>
                {
                    nameProperty.stringValue = evt.newValue;
                    biomeElement.serializedObject.ApplyModifiedProperties();
                    biomeFoldout.text = string.IsNullOrEmpty(evt.newValue) ? "Unnamed Biome" : evt.newValue;
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
                Material currentSkybox = terrain.GetBiome(biomeId).GetSkybox();
                string currentSkyboxPath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentSkybox));
                string currentSkyboxName = skyboxPresets.FirstOrDefault(x => x.Value == currentSkyboxPath).Key;
                int defaultSkyboxIndex = currentSkyboxName != null ? new List<string>(skyboxPresets.Keys).IndexOf(currentSkyboxName) : 0;
                var skyboxDropdown = new PopupField<string>("Skybox", new List<string>(skyboxPresets.Keys), defaultSkyboxIndex);
                PropertyField skyboxField = new PropertyField(skyboxProperty, "Custom Skybox");
                if (skyboxDropdown.value == "Import Custom")
                {
                    skyboxField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    skyboxField.style.display = DisplayStyle.None;
                }

                skyboxDropdown.RegisterValueChangedCallback(evt =>
                {
                    // Placeholder for future functionality
                    string selectedSkyboxName = evt.newValue;
                    if (selectedSkyboxName == "Import Custom")
                    {
                        skyboxField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        skyboxField.style.display = DisplayStyle.None;
                        var biome = terrain.GetBiome(biomeId);
                        string skyboxPath = skyboxPresets[selectedSkyboxName];
                        Material skybox = Resources.Load<Material>(skyboxPath);
                        biome.SetSkybox(skybox);
                    }
                });

                VisualElement skyboxContainer = new VisualElement();
                skyboxContainer.Add(skyboxDropdown);
                skyboxContainer.Add(skyboxField);
                skyboxContainer.AddToClassList("biome-field");

                biomeProperties.Add(skyboxContainer);

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

                // if foldout already exists, load from dictionary
                string heightmapId = "heightmap" + biomeId;
                if (foldoutStates.ContainsKey(heightmapId))
                {
                    heightmapFoldout = foldoutStates[heightmapId];
                    heightmapFoldout.Clear();
                }
                // if doesn't exist, add to dictionary
                else
                {
                    foldoutStates[heightmapId] = heightmapFoldout;
                }

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

                biomeFoldout.Add(biomeProperties);
                biomeContainer.Add(biomeFoldout);
                root.Add(biomeContainer);
                deleteButton.BringToFront();
            }
        }

        private void BuildBiomeFeaturesField(VisualElement root, CustomTerrain terrain, int i, string biomeId, SerializedProperty featuresProperty, Box biomeProperties)
        {
            Label featuresSectionLabel = new Label("Features");
            featuresSectionLabel.AddToClassList("h2");
            biomeProperties.Add(featuresSectionLabel);

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
                    text = string.IsNullOrEmpty(featureNameProperty.stringValue) ? "Unnamed Feature" : featureNameProperty.stringValue,
                    value = false
                };

                // if foldout already exists, load from dictionary
                if (foldoutStates.ContainsKey(featureId))
                {
                    featureFoldout = foldoutStates[featureId];
                    featureFoldout.Clear();
                }
                // if doesn't exist, add to dictionary
                else
                {
                    foldoutStates[featureId] = featureFoldout;
                }

                featureFoldout.AddToClassList("feature-foldout");
                featureFoldout.AddToClassList("feature-field");

                Box featuresProperties = new Box();

                /*
                    FEATURE NAME
                */
                TextField featureNameField = new TextField("Feature Name") { value = featureNameProperty.stringValue };
                featureNameField.AddToClassList("feature-field");

                featureNameField.RegisterValueChangedCallback(evt =>
                {
                    featureNameProperty.stringValue = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                    featureFoldout.text = string.IsNullOrEmpty(evt.newValue) ? "Unnamed Feature" : evt.newValue;
                });

                /*
                    FREQUENCY
                */
                var featureFrequencySlider = new Slider(0, 100)
                {
                    value = featureFrequencyProperty.intValue
                };
                var featureFrequencyIntField = new IntegerField
                {
                    value = featureFrequencyProperty.intValue
                };

                VisualElement featureFrequencyField = GroupSliderAndInt(featureFrequencySlider, featureFrequencyIntField, "Frequency");
                featureFrequencyField.AddToClassList("feature-field");

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
                var featureScaleField = new Vector3Field("Scale") { value = featureScaleProperty.vector3Value };
                featureScaleField.AddToClassList("feature-field");

                featureScaleField.RegisterValueChangedCallback(evt =>
                {
                    featureScaleProperty.vector3Value = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                });

                /*
                    NORMALITY
                */
                var featureNormalField = new Toggle("Set Normal") { value = featureNormalProperty.boolValue };
                featureNormalField.AddToClassList("feature-field");

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

                VisualElement prefabContainer = new VisualElement();
                prefabContainer.Add(prefabDropdown);
                prefabContainer.Add(prefabField);
                prefabContainer.AddToClassList("feature-field");

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

                featuresProperties.Add(featureNameField);
                featuresProperties.Add(featureFrequencyField);
                featuresProperties.Add(featureScaleField);
                featuresProperties.Add(featureNormalField);
                featuresProperties.Add(prefabContainer);
                featuresProperties.Add(deleteFeatureButton);
                featureFoldout.Add(featuresProperties);
                biomeProperties.Add(featureFoldout);
            }

            /*
                ADD FEATURE SECTION
            */
            Label addFeatureLabel = new Label("Add Feature");
            addFeatureLabel.AddToClassList("h2");
            addFeatureLabel.AddToClassList("add-feature-label");//

            biomeProperties.Add(addFeatureLabel);

            var newFeatureDropdown = new PopupField<string>("New Feature", new List<string>(biomeFeaturePresets.Keys), 0);
            newFeatureDropdown.AddToClassList("feature-field");

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

            biomeProperties.Add(newFeatureDropdown);
            biomeProperties.Add(addFeatureButton);
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

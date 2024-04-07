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
            { "Import Custom", "Heightmaps/Flat"     }
        };

        // presets for texture dropdown
        private readonly Dictionary<string, string> texturePresets = new Dictionary<string, string>
        {
            { "Sand",          "Textures/Sand"  },
            { "Grass",         "Textures/grass" },
            { "Stone",         "Textures/Stone" },
            { "Import Custom", "Textures/grass" }
        };

        // presets for skybox dropdown
        private readonly Dictionary<string, string> skyboxPresets = new Dictionary<string, string>
        {
            { "Default",       "Skyboxes/Default" },
            { "Thin",          "Skyboxes/Thin"   },
            { "Dusk",          "Skyboxes/Dusk"    },
            { "Import Custom", "Skyboxes/Default" }
        };

        // presets for features dropdown
        private readonly Dictionary<string, string> biomeFeaturePresets = new Dictionary<string, string>
        {
            { "Trees",         "Features/tree"  },
            { "Horses",        "Features/horse" },
            { "Import Custom", "Features/default" }
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
            VisualElement gizmosContainer = CreateField(drawGizmosField, "If true, a box will be drawn around each chunk. Make sure Gizmos are enabled in the Unity Editor.");
            gizmosContainer.AddToClassList("checkbox-field");

            debugSection.Add(gizmosContainer);
            root.Add(debugSection);

            /* 
                WORLD OPTIONS SECTION
            */
            Label worldOptSectionLabel = new Label("World Options");
            worldOptSectionLabel.AddToClassList("h1");
            root.Add(worldOptSectionLabel);

            Box worldOptSection = new Box();

            TextField worldSeed = new TextField("World Seed") { value = worldSeedStringProperty.stringValue };
            worldSeed.RegisterValueChangedCallback(evt =>
            {
                worldSeedStringProperty.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            VisualElement worldSeedContainer = CreateField(worldSeed, "The seed string used to generate the terrain. If left empty, a random seed will be used.");

            TextField featureSeed = new TextField("Feature Seed") { value = featureSeedStringProperty.stringValue };
            featureSeed.RegisterValueChangedCallback(evt =>
            {
                featureSeedStringProperty.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            VisualElement featureSeedContainer = CreateField(featureSeed, "The seed string used to generate the features. If left empty, a random seed will be used.");

            // create sliders and int fields for TerrainSize, BiomesFrequency and BiomesResolution
            var sizeSlider = new Slider(10, 1000) { value = chunkSizeProperty.intValue };
            var sizeField = new IntegerField { value = chunkSizeProperty.intValue };
            VisualElement sizeContainer = GroupSliderAndInt(sizeSlider, sizeField, "Chunk Size", "The size of each chunk in world units. This is the number of units along each side of the chunk.");

            var resolutionSlider = new Slider(2, 250) { value = chunkResolutionProperty.intValue };
            var resolutionField = new IntegerField { value = chunkResolutionProperty.intValue };
            VisualElement resolutionContainer = GroupSliderAndInt(resolutionSlider, resolutionField, "Chunk Resolution", "The resolution of each chunk. This is the number of vertices along each side of the chunk.");

            var frequencySlider = new Slider(1, 10) { value = biomesPerChunkProperty.intValue };
            var frequencyField = new IntegerField { value = biomesPerChunkProperty.intValue };
            VisualElement frequencyContainer = GroupSliderAndInt(frequencySlider, frequencyField, "Biomes Per Chunk", "The number of biomes that will be placed per chunk. Increasing this will generally make your biomes smaller.");

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

            worldSeedContainer.AddToClassList("options-field");
            featureSeedContainer.AddToClassList("options-field");
            sizeContainer.AddToClassList("options-field");
            frequencyContainer.AddToClassList("options-field");
            resolutionContainer.AddToClassList("options-field");

            worldOptSection.Add(worldSeedContainer);
            worldOptSection.Add(featureSeedContainer);
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
                VisualElement nameContainer = CreateField(nameField, "Name of the current biome.");
                nameContainer.AddToClassList("biome-field");
                biomeProperties.Add(nameContainer);

                /*
                    TEXTURE
                */
                Texture2D currentTexture = terrain.GetBiome(biomeId).GetTexture();
                string currentTexturePath = AssetDatabase.GetAssetPath(currentTexture).Split(new[] { "/Resources/" }, StringSplitOptions.None).LastOrDefault()?.Split('.').FirstOrDefault();
                string currentTextureName = texturePresets.FirstOrDefault(x => x.Value == currentTexturePath).Key;  // Set Name to the value in the dictionary or null.
                if (currentTextureName == null) { currentTextureName = "Import Custom"; }
                int defaultTextureIndex = currentTextureName != null ? new List<string>(texturePresets.Keys).IndexOf(currentTextureName) : 0;
                var textureDropdown = new PopupField<string>("Texture", new List<string>(texturePresets.Keys), defaultTextureIndex);
                VisualElement textureContainer = CreateField(textureDropdown, "The texture that will be used to paint this biome.");

                ObjectField textureField = new ObjectField("Custom Texture");
                textureField.BindProperty(textureProperty);
                textureField.objectType= typeof (Texture2D);
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
                    string texturePath = "";
                    if (selectedTextureName == "Import Custom")
                    {
                        textureField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        textureField.style.display = DisplayStyle.None;
                    }

                    var biome = terrain.GetBiome(biomeId);
                    texturePath = texturePresets[selectedTextureName];
                    Texture2D texture = Resources.Load<Texture2D>(texturePath);
                    biome.SetTexture(texture);
                });
                textureContainer.Add(textureField);
                textureContainer.AddToClassList("biome-field");

                biomeProperties.Add(textureContainer);

                /*
                    SKYBOX
                */
                Material currentSkybox = terrain.GetBiome(biomeId).GetSkybox();
                string currentSkyboxPath = AssetDatabase.GetAssetPath(currentSkybox).Split(new[] { "/Resources/" }, StringSplitOptions.None).LastOrDefault()?.Split('.').FirstOrDefault();
                string currentSkyboxName = skyboxPresets.FirstOrDefault(x => x.Value == currentSkyboxPath).Key; // Set Name to the value in the dictionary or null.
                if (currentSkyboxName == null) { currentSkyboxName = "Import Custom"; }
                int defaultSkyboxIndex = currentSkyboxName != null ? new List<string>(skyboxPresets.Keys).IndexOf(currentSkyboxName) : 0;
                var skyboxDropdown = new PopupField<string>("Skybox", new List<string>(skyboxPresets.Keys), defaultSkyboxIndex);
                VisualElement skyboxContainer = CreateField(skyboxDropdown, "The type of sky that will be seen in this biome.");
                
                ObjectField skyboxField = new ObjectField("Custom Skybox");
                skyboxField.BindProperty(skyboxProperty);
                skyboxField.objectType = typeof(Material);
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
                    // placeholder for future functionality
                    string selectedSkyboxName = evt.newValue;
                    if (selectedSkyboxName == "Import Custom")
                    {
                        skyboxField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        skyboxField.style.display = DisplayStyle.None;
                    }
                    var biome = terrain.GetBiome(biomeId);
                    string skyboxPath = skyboxPresets[selectedSkyboxName];
                    Material skybox = Resources.Load<Material>(skyboxPath);
                    biome.SetSkybox(skybox);
                });
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

                VisualElement frequencyWeightContainer = GroupSliderAndInt(frequencyWeightSlider, frequencyWeightIntField, "Frequency", "How often this biome will appear in the world.");
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
                string currentHeightmapPath = AssetDatabase.GetAssetPath(currentHeightmap).Split(new[] { "/Resources/" }, StringSplitOptions.None).LastOrDefault()?.Split('.').FirstOrDefault();
                string currentHeightmapName = heightmapPresets.FirstOrDefault(x => x.Value == currentHeightmapPath).Key;
                if (currentHeightmapName == null) { currentHeightmapName = "Import Custom"; }
                int defaultHeightmapIndex = currentHeightmapName != null ? new List<string>(heightmapPresets.Keys).IndexOf(currentHeightmapName) : 0;
                var heightmapDropdown = new PopupField<string>("Type", new List<string>(heightmapPresets.Keys), defaultHeightmapIndex);
                VisualElement heightmapType = CreateField(heightmapDropdown, "The type of heightmap to be used for the biome. This controls the topography of the terrain.");
                
                ObjectField heightmapField = new ObjectField("Custom Heightmap");
                heightmapField.BindProperty(heightmapProperty);
                heightmapField.objectType = typeof(HeightmapBase);
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
                    }
                    var biome = terrain.GetBiome(biomeId);
                    string heightmapPath = heightmapPresets[selectedHeightmapName];
                    HeightmapBase heightmap = Resources.Load<HeightmapBase>(heightmapPath);
                    biome.SetHeightMap(heightmap);
                    UpdateUI(root, terrain);
                });

                heightmapField.RegisterValueChangedCallback(evt =>
                {
                    if (currentHeightmap != evt.newValue) 
                    {
                        UpdateUI(root, terrain);
                    }
                });

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

                            VisualElement sliderContainer = GroupSliderAndFloat(slider, floatField, currentProperty.displayName, currentProperty.displayName);
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

                        case SerializedPropertyType.ArraySize:
                            // ArraySize handling
                            var arraySizeField = new IntegerField(currentProperty.displayName) { value = currentProperty.intValue };
                            arraySizeField.RegisterValueChangedCallback(evt =>
                            {
                                currentProperty.arraySize = evt.newValue;
                                currentProperty.serializedObject.ApplyModifiedProperties();
                                UpdateUI(root,terrain);
                            });
                            heightmapProperties.Add(arraySizeField);
                            break;

                        case SerializedPropertyType.Generic:
                            break;

                        case SerializedPropertyType.ObjectReference:
                            var objectField = new ObjectField(currentProperty.displayName) { value = currentProperty.objectReferenceValue };
                            objectField.BindProperty(currentProperty);
                            objectField.RegisterValueChangedCallback(evt =>
                            {
                                currentProperty.objectReferenceValue = evt.newValue;
                                currentProperty.serializedObject.ApplyModifiedProperties();
                            });
                            heightmapProperties.Add(objectField);
                    
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

            VisualElement buttonsContainer = new VisualElement();
            VisualElement generateBtnsContainer = new VisualElement();
            generateBtnsContainer.Add(generateButton);
            generateBtnsContainer.Add(regenerateFeatures);
            buttonsContainer.Add(generateBtnsContainer);
            buttonsContainer.Add(exportButton);
            buttonsContainer.AddToClassList("buttons-container");
            generateBtnsContainer.AddToClassList("generate-buttons");

            // change class of generate buttons container depending on inspector width
            root.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (evt.newRect.width < 350) { generateBtnsContainer.AddToClassList("column"); }
                else { generateBtnsContainer.RemoveFromClassList("column"); }
            });

            root.Add(buttonsContainer);
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

                VisualElement featureContainer = new VisualElement();
                featureContainer.AddToClassList("feature-container");

                // create main foldout for feature
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

                /*
                    DELETE FEATURE BUTTON
                */
                Button deleteFeatureButton = new Button(() =>
                {
                    terrain.GetBiome(biomeId).DeleteFeature(featureId);
                    UpdateUI(root, terrain);
                });
                deleteFeatureButton.AddToClassList("delete-feature");

                featureContainer.Add(featureFoldout);
                featureContainer.Add(deleteFeatureButton);

                Box featuresProperties = new Box();

                /*
                    FEATURE NAME
                */
                TextField featureNameField = new TextField("Feature Name") { value = featureNameProperty.stringValue };
                featureNameField.RegisterValueChangedCallback(evt =>
                {
                    featureNameProperty.stringValue = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                    featureFoldout.text = string.IsNullOrEmpty(evt.newValue) ? "Unnamed Feature" : evt.newValue;
                });

                VisualElement featureNameContainer = CreateField(featureNameField, "The name of the biome feature.");
                featureNameContainer.AddToClassList("feature-field");

                /*
                    FREQUENCY
                */
                var featureFrequencySlider = new Slider(0, 100) { value = featureFrequencyProperty.intValue };
                var featureFrequencyIntField = new IntegerField { value = featureFrequencyProperty.intValue };

                VisualElement featureFrequencyField = GroupSliderAndInt(featureFrequencySlider, featureFrequencyIntField, "Frequency", "How often this feature will appear in the biome.");
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
                featureScaleField.RegisterValueChangedCallback(evt =>
                {
                    featureScaleProperty.vector3Value = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                });
                VisualElement scaleContainer = CreateField(featureScaleField, "The scale of this feature. Controls how big the feature appears in the biome.");
                scaleContainer.AddToClassList("feature-field");
                /*
                    NORMALITY
                */
                var featureNormalField = new Toggle("Set Normal") { value = featureNormalProperty.boolValue };
                featureNormalField.RegisterValueChangedCallback(evt =>
                {
                    featureNormalProperty.boolValue = evt.newValue;
                    featureElement.serializedObject.ApplyModifiedProperties();
                });

                VisualElement normalContainer = CreateField(featureNormalField, "If true, the object will be placed normal (perpendicular) to the terrain.");
                normalContainer.AddToClassList("feature-field");
                /*
                    PREFAB
                */
                GameObject currentPrefab = terrain.GetBiome(biomeId).GetFeature(featureId).Prefab;
                string currentPrefabPath = AssetDatabase.GetAssetPath(currentPrefab).Split(new[] { "/Resources/" }, StringSplitOptions.None).LastOrDefault()?.Split('.').FirstOrDefault();
                string currentPrefabName = biomeFeaturePresets.FirstOrDefault(x => x.Value == currentPrefabPath).Key;
                if (currentPrefabName == null) { currentPrefabName = "Import Custom"; }
                int defaultPrefabIndex = currentPrefabName != null ? new List<string>(biomeFeaturePresets.Keys).IndexOf(currentPrefabName) : 0;
                var prefabDropdown = new PopupField<string>("Feature", new List<string>(biomeFeaturePresets.Keys), defaultPrefabIndex);  // Set Name to the value in the dictionary or null.
                VisualElement prefabContainer = CreateField(prefabDropdown, "The feature/object model that spawns in the terrain.");
                
                ObjectField prefabField = new ObjectField("Custom Feature");
                prefabField.BindProperty(featurePrefabProperty);
                prefabField.objectType = typeof(GameObject);
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
                    }
                    var feature = terrain.GetBiome(biomeId).GetFeature(featureId);
                    string prefabPath = biomeFeaturePresets[selectedPrefabName];
                    GameObject prefab = Resources.Load<GameObject>(prefabPath);
                    feature.Prefab = prefab;
                });
                prefabContainer.Add(prefabField);
                prefabContainer.AddToClassList("feature-field");

                featuresProperties.Add(featureNameContainer);
                featuresProperties.Add(featureFrequencyField);
                featuresProperties.Add(scaleContainer);
                featuresProperties.Add(normalContainer);
                featuresProperties.Add(prefabContainer);
                featuresProperties.Add(deleteFeatureButton);
                featureFoldout.Add(featuresProperties);
                featureContainer.Add(featureFoldout);
                featureContainer.Add(deleteFeatureButton);
                biomeProperties.Add(featureContainer);
                deleteFeatureButton.BringToFront();
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
                if (selectedFeatureName == "Import Custom")
                {
                    string featurePath = biomeFeaturePresets[selectedFeatureName];
                    GameObject featurePrefab = Resources.Load<GameObject>(featurePath);
                    newFeature.Prefab = featurePrefab;
                    newFeature.Name = "Custom";
                }
                else
                {
                    newFeature.Name = selectedFeatureName;
                    string featurePath = biomeFeaturePresets[selectedFeatureName];
                    GameObject featurePrefab = Resources.Load<GameObject>(featurePath);
                    newFeature.Prefab = featurePrefab;
                }

                terrain.GetBiome(biomeId).AddFeature(newFeature);
                UpdateUI(root, terrain);
            })
            {
                text = "Add Feature"
            };

            biomeProperties.Add(newFeatureDropdown);
            biomeProperties.Add(addFeatureButton);
        }

        private VisualElement GroupSliderAndInt(Slider slider, IntegerField field, string label, string tooltipText)
        {
            // create parent container
            VisualElement container = new VisualElement();
            Label containerLabel = new Label(label);

            // create tooltip element
            VisualElement tooltip = new VisualElement() { tooltip = tooltipText };
            TextElement i = new TextElement() { text = "?" };
            i.AddToClassList("tooltip-i");
            tooltip.Add(i);
            tooltip.AddToClassList("tooltip");

            // group fields
            VisualElement fields = new VisualElement();
            fields.Add(slider);
            fields.Add(field);
            fields.AddToClassList("slider-int-field");

            // add to parent container
            container.Add(containerLabel);
            container.Add(fields);
            container.Add(tooltip);

            container.AddToClassList("slider-int-container");
            container.AddToClassList("property-field");

            return container;
        }

        private VisualElement GroupSliderAndFloat(Slider slider, FloatField field, string label, string tooltipText)
        {
            // create parent container
            VisualElement container = new VisualElement();
            Label containerLabel = new Label(label);
            
            // create tooltip element
            VisualElement tooltip = new VisualElement() { tooltip = tooltipText };
            TextElement i = new TextElement() { text = "?" };
            i.AddToClassList("tooltip-i");
            tooltip.Add(i);
            tooltip.AddToClassList("tooltip");

            // group fields
            VisualElement fields = new VisualElement();
            fields.Add(slider);
            fields.Add(field);
            fields.AddToClassList("slider-float-field");

            // add to parent container
            container.Add(containerLabel);
            container.Add(fields);
            container.Add(tooltip);

            container.AddToClassList("slider-float-container");
            container.AddToClassList("property-field");

            return container;
        }
//
        private VisualElement CreateField<T>(BaseField<T> field, string tooltipText)
        {
            VisualElement fieldContainer = new VisualElement();
            VisualElement tooltip = new VisualElement() { tooltip = tooltipText };
            TextElement i = new TextElement() { text = "?" };
            i.AddToClassList("tooltip-i");
            tooltip.Add(i);
            tooltip.AddToClassList("tooltip");

            fieldContainer.Add(field);
            fieldContainer.Add(tooltip);
            fieldContainer.AddToClassList("property-field");
            return fieldContainer;
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

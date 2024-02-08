#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    [SerializeField]
    private Biome[] _biomes;

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomTerrain))]
    public class CustomTerrainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CustomTerrain terrain = (CustomTerrain)this.target;

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Terrain"))
            {
                terrain.GenerateTerrain();
            }
        }
    }
#endif

    // Function to be called when the button is clicked in Editor mode
    public void GenerateTerrain()
    {
        // Implement your terrain generation logic here
        Debug.Log("Generating Terrain...");
    }

}

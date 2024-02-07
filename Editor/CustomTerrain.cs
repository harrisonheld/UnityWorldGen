#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    // Your existing code...

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomTerrain))]
    public class CustomTerrainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CustomTerrain terrain = (CustomTerrain)target;

            GUILayout.Space(10);

            // Create a button in the Inspector
            if (GUILayout.Button("Generate Terrain"))
            {
                // Call the function you want to execute in Editor mode
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

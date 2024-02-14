using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrain : Editor
{
    public VisualTreeAsset m_InspectorXML;
    // Create a new VisualElement to be the root of our inspector UI
    VisualElement myInspector = new VisualElement();

    // Load from default reference
    m_InspectorXML.CloneTree(myInspector);

    // Return the finished inspector UI
    return myInspector;

}

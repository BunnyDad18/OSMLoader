using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SplineMeshGen))]
[ExecuteInEditMode]
public class SplineMeshGen_Inspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        SplineMeshGen splineMeshGen = (SplineMeshGen)target;
        VisualElement inspector = new();

        Button setupButton = new Button() { text = "Setup" };
        setupButton.clicked += splineMeshGen.Setup;

        inspector.Add(setupButton);

        return inspector;
    }
}

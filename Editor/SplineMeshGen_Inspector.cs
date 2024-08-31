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

        Toggle showInsideGizmo = new Toggle() { text = "Show inside Gizmo" };
        showInsideGizmo.value = splineMeshGen.showInsideGizmos;
        showInsideGizmo.RegisterValueChangedCallback(value => { splineMeshGen.showInsideGizmos = value.newValue; });

        IntegerField splineGizmoCount = new IntegerField("Spline Gizmo Count");
        splineGizmoCount.value = splineMeshGen.splineGizmoCount;
        splineGizmoCount.RegisterValueChangedCallback(value => { splineMeshGen.splineGizmoCount = value.newValue; });

        Button setupButton = new Button() { text = "Setup" };
        setupButton.clicked += splineMeshGen.Setup;

        inspector.Add(showInsideGizmo);
        inspector.Add(splineGizmoCount);
        inspector.Add(setupButton);

        return inspector;
    }
}

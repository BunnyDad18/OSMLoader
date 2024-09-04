using UnityEditor;
using UnityEditor.UIElements;
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

        Slider detailLevel = new Slider($"Detail - {splineMeshGen.detailLevel}", 0.1f, 2f);
        detailLevel.value = splineMeshGen.detailLevel;
        detailLevel.RegisterValueChangedCallback(value => { 
            splineMeshGen.detailLevel = value.newValue;
            detailLevel.label = $"Detail - {value.newValue}";
        });

        Button setupButton = new Button() { text = "Setup" };
        setupButton.clicked += splineMeshGen.Setup;

        PropertyField taxiwayMaterial = new PropertyField();
        taxiwayMaterial.bindingPath = "taxiwayMaterial";
        inspector.Add(taxiwayMaterial);

        inspector.Add(showInsideGizmo);
        inspector.Add(splineGizmoCount);
        inspector.Add(detailLevel);
        inspector.Add(setupButton);

        return inspector;
    }
}

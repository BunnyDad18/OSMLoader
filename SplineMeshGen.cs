using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

[RequireComponent(typeof(SplineContainer))]
public class SplineMeshGen : MonoBehaviour
{
    public bool showInsideGizmos = true;
    public int splineGizmoCount = 5;

    private SplineContainer _splineContainer;

    private List<SplineMesh> _meshList = new();

    public void Setup()
    {
        _meshList.Clear();
        _splineContainer = GetComponent<SplineContainer>();
        for (int i = 0; i < _splineContainer.Splines.Count; i++)
        {
            Spline spline = _splineContainer.Splines[i];
            SplineMesh newMesh = new SplineMesh();
            newMesh.connectedStart = !IsStartKnotIsolated(i);
            newMesh.GenerateEdges(spline, 11);
            _meshList.Add(newMesh);
        }
        foreach(SplineMesh splineMesh in _meshList)
        {
            splineMesh.CheckForIntersections(_meshList);
        }
        Debug.Log("Finsihed Setup");
    }

    private bool IsStartKnotIsolated(int index)
    {
        IReadOnlyList<SplineKnotIndex> knotLinks = _splineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(index, 0));
        List<SplineKnotIndex> endKnots = new List<SplineKnotIndex>();
        foreach (SplineKnotIndex knot in knotLinks)
        {
            if (knot.Knot == 0 || _splineContainer.Splines[knot.Spline].Count == knot.Knot + 1)
            {
                endKnots.Add(knot);
                /*if (endKnots.Count > 1)
                {
                    CreateMarker(_splineContainer.Splines[knot.Spline].Knots.ElementAt(knot.Knot).Position, 2);
                }*/
            }
        }
        Vector3 avarageRotation = Vector3.zero;
        if (endKnots.Count > 1)
        {
            foreach (SplineKnotIndex knot in endKnots)
            {
                Spline testPline = _splineContainer.Splines[knot.Spline];
                testPline.Evaluate(knot.Knot == 0 ? 0 : 1, out float3 position, out float3 tangent, out float3 up);
                //_splineContainer.Splines[knot.Spline].SetTangentMode(knot.Knot, TangentMode.Mirrored);
                avarageRotation += (Vector3)tangent;
            }
            avarageRotation /= endKnots.Count;
            Debug.Log("=============");
            foreach (SplineKnotIndex knot in endKnots)
            {
                Spline testPline = _splineContainer.Splines[knot.Spline];
                testPline.Evaluate(knot.Knot == 0 ? 0 : 1, out float3 position, out float3 tangent, out float3 up);
                BezierKnot newKnot = _splineContainer.Splines[knot.Spline].Knots.ElementAt(knot.Knot);
                float difference = (avarageRotation.normalized - ((Vector3)tangent).normalized).magnitude;
                Debug.Log($"average = {avarageRotation.normalized}. Knot = {((Vector3)tangent).normalized}. difference = {difference}");
                newKnot.Rotation = Quaternion.LookRotation(avarageRotation.normalized * (difference < 1 ? 1 : -1), Vector3.up);
                _splineContainer.Splines[knot.Spline].SetTangentMode(knot.Knot, TangentMode.Continuous);
                _splineContainer.Splines[knot.Spline].SetKnot(knot.Knot, newKnot);
            }
        }
        return knotLinks.Count <= 1;
    }

    private void CreateMarker(Vector3 position, float scale)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.position = position;
        marker.transform.localScale *= scale;
        marker.transform.SetParent(this.transform);
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < _meshList.Count; i++)
        {
            if (i > splineGizmoCount) return;
            SplineMesh spline = _meshList[i];
            DrawSplineEdgeGizmo(spline.left.edges, spline.left.intersects, spline.left.inside, Color.blue);
            //DrawSplineEdgeGizmo(spline.center.edges, spline.center.intersects, spline.center.inside, Color.white);
            DrawSplineEdgeGizmo(spline.right.edges, spline.right.intersects, spline.right.inside, Color.cyan);
            DrawSplineEdgeGizmo(spline.end.edges, spline.end.intersects, spline.end.inside, Color.red);
            DrawSplineEdgeGizmo(spline.start.edges, spline.start.intersects, spline.start.inside, Color.green);
        }
    }

    private void DrawSplineEdgeGizmo(List<Vector3> edges, List<int> intersects, List<int> inside, Color main)
    {
        for (int i = 0; i < edges.Count - 1; i++)
        {
            Gizmos.color = main;
            if (inside.Contains(i) && showInsideGizmos) Gizmos.color = Color.yellow;
            else if (inside.Contains(i)) continue;
            if (intersects.Contains(i)) Gizmos.color = Color.red;
            Gizmos.DrawLine(edges[i], edges[i + 1]);
        }
    }
}

public class SplineEdge
{
    public List<Vector3> edges = new List<Vector3>();
    public List<int> intersects = new List<int>();
    public List<int> inside = new List<int>();
    public List<Vector3> intersectPoints = new List<Vector3>();
}

public class SplineMesh
{
    public SplineEdge left = new();
    public SplineEdge right = new();
    public SplineEdge center = new();
    internal bool connectedStart;

    public SplineEdge end = new();
    public SplineEdge start = new();

    internal void CheckForIntersections(List<SplineMesh> _meshList)
    {
        bool inside = connectedStart;
        for (int i = 0; i < left.edges.Count - 1; i++)
        {
            foreach (SplineMesh mesh in _meshList)
            {
                if (mesh == this) continue;
                bool intersect = CheckEdge(mesh.left, left.edges[i], left.edges[i + 1], out Vector3 position);
                if (!intersect)
                {
                    intersect = CheckEdge(mesh.right, left.edges[i], left.edges[i + 1], out position);
                }
                if (!intersect)
                {
                    intersect = CheckEdge(mesh.end, left.edges[i], left.edges[i + 1], out position);
                }
                if (!intersect)
                {
                    intersect = CheckEdge(mesh.start, left.edges[i], left.edges[i + 1], out position);
                }
                if (intersect)
                {
                    inside = !inside;
                    left.intersects.Add(i);
                    left.intersectPoints.Add(position);
                    left.inside.Add(i);
                }
                else if (inside) left.inside.Add(i);
            }
        }
        inside = connectedStart;
        for (int i = 0; i < right.edges.Count - 1; i++)
        {
            foreach (SplineMesh mesh in _meshList)
            {
                if (mesh == this) continue;
                bool intersect = CheckEdge(mesh.left, right.edges[i], right.edges[i + 1], out Vector3 position);
                if (!intersect)
                {
                    intersect = CheckEdge(mesh.right, right.edges[i], right.edges[i + 1], out position);
                }
                if (!intersect)
                {
                    intersect = CheckEdge(mesh.end, right.edges[i], right.edges[i + 1], out position);
                }
                if (!intersect)
                {
                    intersect = CheckEdge(mesh.start, right.edges[i], right.edges[i + 1], out position);
                }
                if (intersect)
                {
                    inside = !inside;
                    right.intersects.Add(i);
                    right.intersectPoints.Add(position);
                    right.inside.Add(i);
                }
                else if (inside) right.inside.Add(i);
            }
        }
    }

    private bool CheckEdge(SplineEdge edge, Vector3 lineStart, Vector3 lineEnd, out Vector3 position)
    {
        position = Vector3.zero;
        for (int i = 0; i < edge.edges.Count - 1; i++)
        {
            bool intersect = EdgeHelpers.CheckIntersect(lineStart, lineEnd, edge.edges[i], edge.edges[i + 1], out position);
            if (intersect) return true;
        }
        return false;
    }


    internal void GenerateEdges(Spline spline, float width)
    {
        float length = spline.GetLength();
        float detail = length / 5;
        float3 position = float3.zero;
        float3 direction = float3.zero;
        float3 up = float3.zero;

        spline.Evaluate(0, out position, out direction, out up);
        start.edges.Add(position + (math.normalize(math.cross(direction, up)) * width));
        start.edges.Add(position - (math.normalize(math.cross(direction, up)) * width));

        for (float i = 0; i <= 1; i += 1f/detail)
        {
            spline.Evaluate(i, out position, out direction, out up);
            center.edges.Add(position);
            right.edges.Add(position + (math.normalize(math.cross(direction, up)) * width));
            left.edges.Add(position - (math.normalize(math.cross(direction, up)) * width));
        }

        spline.Evaluate(1, out position, out direction, out up);
        center.edges.Add(position);
        right.edges.Add(position + (math.normalize(math.cross(direction, up)) * width));
        left.edges.Add(position - (math.normalize(math.cross(direction, up)) * width));

        end.edges.Add(position + (math.normalize(math.cross(direction, up)) * width));
        end.edges.Add(position - (math.normalize(math.cross(direction, up)) * width));
    }
}
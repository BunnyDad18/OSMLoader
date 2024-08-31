using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

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
        return _splineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(index, 0)).Count <= 1;
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
        float detail = 200f;
        for(float i = 0; i <= 1; i += 1f/detail)
        {
            spline.Evaluate(i, out float3 position, out float3 direction, out float3 up);
            center.edges.Add(position);
            right.edges.Add(position + (math.normalize(math.cross(direction, up)) * width));
            left.edges.Add(position - (math.normalize(math.cross(direction, up)) * width));
        }
    }
}
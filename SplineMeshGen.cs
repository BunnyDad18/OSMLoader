using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineMeshGen : MonoBehaviour
{
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
        foreach(SplineMesh splineMesh in _meshList)
        {
            splineMesh.CheckForInside();
        }
        Debug.Log("Finsihed Setup");
    }

    private bool IsStartKnotIsolated(int index)
    {
        return _splineContainer.KnotLinkCollection.GetKnotLinks(new SplineKnotIndex(index, 0)).Count <= 1;
    }

    private void OnDrawGizmos()
    {
        foreach(SplineMesh spline in _meshList)
        {
            DrawSplineEdgeGizmo(spline.left.edges, spline.left.intersects, spline.left.inside);
            DrawSplineEdgeGizmo(spline.center.edges, spline.center.intersects, spline.center.inside);
            DrawSplineEdgeGizmo(spline.right.edges, spline.right.intersects, spline.right.inside);
        }
    }

    private void DrawSplineEdgeGizmo(List<Vector3> edges, List<int> intersects, List<int> inside)
    {
        for (int i = 0; i < edges.Count - 1; i++)
        {
            Gizmos.color = Color.white;
            if (inside.Contains(i)) continue;// Gizmos.color = Color.yellow;
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
        foreach(SplineMesh spline in _meshList)
        {
            if (spline == this) continue;
            for (int i = 0; i < spline.left.edges.Count - 1; i++)
            {
                CheckEdge(left, spline.left.edges[i], spline.left.edges[i + 1]);
                CheckEdge(right, spline.left.edges[i], spline.left.edges[i + 1]);
            }
            for (int i = 0; i < spline.right.edges.Count - 1; i++)
            {
                CheckEdge(left, spline.right.edges[i], spline.right.edges[i + 1]);
                CheckEdge(right, spline.right.edges[i], spline.right.edges[i + 1]);
            }
        }
    }

    private void CheckEdge(SplineEdge edge, Vector3 lineStart, Vector3 lineEnd)
    {
        for (int i = 0; i < edge.edges.Count - 1; i++)
        {
            bool intersect = EdgeHelpers.CheckIntersect(lineStart, lineEnd, edge.edges[i], edge.edges[i + 1], out Vector3 position);
            if (!intersect) continue;
            //if (edge.intersects.Contains(i)) continue;
            edge.intersects.Add(i);
            edge.intersectPoints.Add(position);
        }
    }
    

    internal void GenerateEdges(Spline spline, float width)
    {
        float length = spline.GetLength();
        float detail = 50f;
        for(float i = 0; i <= 1; i += 1f/detail)
        {
            spline.Evaluate(i, out float3 position, out float3 direction, out float3 up);
            center.edges.Add(position);
            right.edges.Add(position + (math.normalize(math.cross(direction, up)) * width));
            left.edges.Add(position - (math.normalize(math.cross(direction, up)) * width));
        }
    }

    internal void CheckForInside()
    {
        left.intersects.Sort();
        right.intersects.Sort();
        for (int i = 0; i < left.intersects.Count; i++)
        {
            for (int j = 0; j < left.intersects[i]; j++)
            {
                if (i % 2 == 0)
                {
                    left.inside.Add(j);
                }
            }
        }
        for (int i = 0; i < right.intersects.Count; i+= 2)
        {
            int startIndex;
            int endIndex;
            if (i == 0 && connectedStart)
            {
                startIndex = 0;
                endIndex = right.intersects[i];
            }
            else
            {
                startIndex = right.intersects[i];
                if (i + 1 < right.intersects.Count)
                {
                    endIndex = right.intersects[i + 1];
                }
                else
                {
                    endIndex = right.edges.Count;
                }
            }
            for (int j = startIndex; j <= endIndex; j++)
            {
                right.inside.Add(j);
            }
        }
    }
}
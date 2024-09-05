using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineMeshGen : MonoBehaviour
{
    public Material taxiwayMaterial;
    public bool showInsideGizmos = true;
    public int splineGizmoCount = 5;
    public float detailLevel = 1.0f;

    private SplineContainer _splineContainer;

    private List<SplineMeshData> _meshList = new();

    public void Setup()
    {
        DestroyChildern();
        _meshList.Clear();
        _splineContainer = GetComponent<SplineContainer>();
        for (int i = 0; i < _splineContainer.Splines.Count; i++)
        {
            Spline spline = _splineContainer.Splines[i];
            SplineMeshData newMesh = new SplineMeshData();
            newMesh.connectedStart = !IsStartKnotIsolated(i);
            newMesh.GenerateEdges(spline, 11, detailLevel);
            _meshList.Add(newMesh);
        }
        foreach (SplineMeshData splineMesh in _meshList)
        {
            splineMesh.CheckForIntersections(_meshList);
        }
        foreach (SplineMeshData splineMeshData in _meshList)
        {
            CreateMesh(splineMeshData);
        }
        Debug.Log("Finsihed Setup");
    }

    private void CreateMesh(SplineMeshData splineMesh)
    {
        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();

        int triCount = 0;
        int rightIndex = 0;
        for (int i = 0; i < splineMesh.center.edges.Count - 1; i++)
        {
            if(i == 0)
            {
                while (splineMesh.right.inside.Contains(rightIndex))
                {
                    if (rightIndex > splineMesh.right.edges.Count - 3) break;
                    rightIndex++;
                }
                //rightIndex--;
                AddVert(i, true, true);
                AddVert(rightIndex - 1, false, true);
                AddVert(rightIndex - 1, false, false);
            }
            if(!splineMesh.right.inside.Contains(i) && i < splineMesh.right.edges.Count - 2)
            {
                rightIndex = i + 1;
            }
            else if (splineMesh.center.intersects.Contains(i))
            {
                while (splineMesh.right.inside.Contains(rightIndex))
                {
                    if (rightIndex > splineMesh.right.edges.Count - 3) break;
                    rightIndex++;
                }
                AddVert(i, true, true);
                AddVert(rightIndex - 1, false, true);
                AddVert(rightIndex - 1, false, false);
            }


            AddVert(i, true, true);
            AddVert(rightIndex, false, true);
            AddVert(i, true, false);
            if (!splineMesh.right.inside.Contains(i + 1))
            {
                AddVert(i, true, false);
                AddVert(rightIndex, false, true);
                AddVert(rightIndex, false, false);
            }
        }

        void AddVert(int index, bool inside, bool first, int intersectIndex = -1)
        {
            SplineEdge edge = inside ? splineMesh.center : splineMesh.right;
            if(intersectIndex != -1 && intersectIndex < edge.intersectPoints.Count - 1)
            {
                Debug.Log(edge.intersectPoints[intersectIndex]);
                verts.Add(edge.intersectPoints[intersectIndex]);
                CreateMarker(edge.intersectPoints[intersectIndex]);
            }
            else
            {
                verts.Add(edge.edges[first ? index : index + 1]);
            }
            uvs.Add(new Vector2(inside ? .5f : 1, first ? 0 : 1));
            tris.Add(triCount++);
        }

        Mesh mesh = new();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        GetNewMeshFilter().mesh = mesh;
    }

    private void CreateMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.SetParent(transform, false);
        marker.transform.position = position;
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
            }
        }
        Vector3 avarageRotation = Vector3.zero;
        if (endKnots.Count > 1)
        {
            foreach (SplineKnotIndex knot in endKnots)
            {
                Spline testPline = _splineContainer.Splines[knot.Spline];
                testPline.Evaluate(knot.Knot == 0 ? 0 : 1, out float3 position, out float3 tangent, out float3 up);
                avarageRotation += (Vector3)tangent;
            }
            avarageRotation /= endKnots.Count;
            foreach (SplineKnotIndex knot in endKnots)
            {
                Spline testPline = _splineContainer.Splines[knot.Spline];
                testPline.Evaluate(knot.Knot == 0 ? 0 : 1, out float3 position, out float3 tangent, out float3 up);
                BezierKnot newKnot = _splineContainer.Splines[knot.Spline].Knots.ElementAt(knot.Knot);
                float difference = (avarageRotation.normalized - ((Vector3)tangent).normalized).magnitude;
                newKnot.Rotation = Quaternion.LookRotation(avarageRotation.normalized * (difference < 1 ? 1 : -1), Vector3.up);
                _splineContainer.Splines[knot.Spline].SetTangentMode(knot.Knot, TangentMode.Continuous);
                _splineContainer.Splines[knot.Spline].SetKnot(knot.Knot, newKnot);
            }
        }
        return knotLinks.Count <= 1;
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < _meshList.Count; i++)
        {
            if (i > splineGizmoCount) return;
            SplineMeshData spline = _meshList[i];
            DrawSplineEdgeGizmo(spline.left.edges, spline.left.intersects, spline.left.inside, Color.blue);
            DrawSplineEdgeGizmo(spline.center.edges, spline.center.intersects, spline.center.inside, Color.white);
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

    private void DestroyChildern()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private MeshFilter GetNewMeshFilter(string name = "Spline Mesh")
    {
        GameObject newSplineMesh = new(name);
        newSplineMesh.transform.SetParent(transform);
        MeshRenderer renderer = newSplineMesh.AddComponent<MeshRenderer>();
        renderer.SetMaterials(new List<Material>() { taxiwayMaterial });
        return newSplineMesh.AddComponent<MeshFilter>();
    }
}

public class SplineEdge
{
    public List<Vector3> edges = new List<Vector3>();
    public List<int> intersects = new List<int>();
    public List<int> inside = new List<int>();
    public List<Vector3> intersectPoints = new List<Vector3>();
}

public class SplineMeshData
{
    public SplineEdge left = new();
    public SplineEdge right = new();
    public SplineEdge center = new();
    internal bool connectedStart;

    public SplineEdge end = new();
    public SplineEdge start = new();

    internal void CheckForIntersections(List<SplineMeshData> _meshList)
    {
        bool inside = connectedStart;
        CheckEdgeForIntersections(_meshList, left, inside);
        CheckEdgeForIntersections(_meshList, right, inside);
        CheckCenterForIntersections(_meshList, center, inside);
    }

    private void CheckCenterForIntersections(List<SplineMeshData> _meshList, SplineEdge edge, bool inside)
    {
        for (int i = 0; i < edge.edges.Count - 1; i++)
        {
            foreach (SplineMeshData mesh in _meshList)
            {
                if (mesh == this) continue;
                if (CheckEdge(mesh.center, edge.edges[i], edge.edges[i + 1], out Vector3 position) ||
                    CheckEdge(mesh.end, edge.edges[i], edge.edges[i + 1], out position) ||
                    CheckEdge(mesh.start, edge.edges[i], edge.edges[i + 1], out position))
                {
                    inside = !inside;
                    edge.intersects.Add(i);
                    edge.intersectPoints.Add(position);
                    edge.edges[i] = position;
                }
                else if (inside) edge.inside.Add(i);
            }
        }
    }

    private void CheckEdgeForIntersections(List<SplineMeshData> _meshList, SplineEdge edge, bool inside)
    {
        for (int i = 0; i < edge.edges.Count - 1; i++)
        {
            foreach (SplineMeshData mesh in _meshList)
            {
                if (mesh == this) continue;
                if (CheckEdge(mesh.left, edge.edges[i], edge.edges[i + 1], out Vector3 position) ||
                    CheckEdge(mesh.right, edge.edges[i], edge.edges[i + 1], out position) ||
                    CheckEdge(mesh.end, edge.edges[i], edge.edges[i + 1], out position) ||
                    CheckEdge(mesh.start, edge.edges[i], edge.edges[i + 1], out position))
                {
                    inside = !inside;
                    edge.intersects.Add(i);
                    edge.intersectPoints.Add(position);
                    edge.inside.Add(i);
                    edge.edges[i] = position;
                }
                else if (inside) edge.inside.Add(i);
            }
        }
    }

    private bool CheckEdge(SplineEdge edge, Vector3 lineStart, Vector3 lineEnd, out Vector3 position, float offset = .001f)
    {
        position = Vector3.zero;
        for (int i = 0; i < edge.edges.Count - 1; i++)
        {
            bool intersect = EdgeHelpers.CheckIntersect(lineStart, lineEnd, edge.edges[i], edge.edges[i + 1], out position, offset);
            if (intersect) return true;
        }
        return false;
    }


    internal void GenerateEdges(Spline spline, float width, float detailLevel)
    {
        float length = spline.GetLength();
        float detail = length * detailLevel;

        spline.Evaluate(0, out float3 position, out float3 direction, out float3 up);
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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WayNodeMeshBuilder
{
    //public static Mesh Get(List<Vector3> positions, float width)
    //{
    //    var mesh = new Mesh();
    //    List<Vector3> orderedVerts = new List<Vector3>();
    //    List<int> tris = new();
    //    List<Vector2> uvs = new();
    //    List<Vector3>[] verts = GetPostions(positions, width);
    //    for (int i = 0; i < verts.Length; i++)
    //    {
    //        if (i >= verts.Length - 1) break;
    //        for (int j = 0; j < verts[i].Count; j++)
    //        {
    //            if (j >= verts[i].Count - 1) break;
    //            tris.Add(orderedVerts.Count);
    //            orderedVerts.Add(verts[i][j]);
    //            uvs.Add(new Vector2(0, 0));
    //            tris.Add(orderedVerts.Count);
    //            orderedVerts.Add(verts[i][j + 1]);
    //            uvs.Add(new Vector2(1, 0));
    //            tris.Add(orderedVerts.Count);
    //            orderedVerts.Add(verts[i + 1][j]);
    //            uvs.Add(new Vector2(0, 1));

    //            tris.Add(orderedVerts.Count);
    //            orderedVerts.Add(verts[i][j + 1]);
    //            uvs.Add(new Vector2(1, 0));
    //            tris.Add(orderedVerts.Count);
    //            orderedVerts.Add(verts[i + 1][j + 1]);
    //            uvs.Add(new Vector2(1, 1));
    //            tris.Add(orderedVerts.Count);
    //            orderedVerts.Add(verts[i + 1][j]);
    //            uvs.Add(new Vector2(0, 1));
    //        }
    //    }
    //    mesh.SetVertices(orderedVerts.ToArray());
    //    mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
    //    mesh.SetUVs(0, uvs.ToArray());
    //    mesh.RecalculateBounds();
    //    mesh.RecalculateNormals();
    //    mesh.RecalculateUVDistributionMetrics();
    //    return mesh;
    //}

    //enum direction
    //{
    //    none,
    //    left,
    //    right,
    //    both
    //}

    //private static bool CheckPosition(Vector3 position, List<Vector3> childPositions, Vector3 forward, out direction direction, float width)
    //{
    //    direction = direction.none;
    //    foreach(Vector3 child in childPositions)
    //    {
    //        if (Vector3.Distance(position, child) >= width) continue;
    //        if (Vector3.Dot(child, forward) < 0)
    //        {
    //            if (direction == direction.right)
    //            {
    //                direction = direction.both;
    //                return true;
    //            }
    //            direction = direction.left;
    //        }
    //        if (Vector3.Dot(child, forward) > 0)
    //        {
    //            if (direction == direction.left)
    //            {
    //                direction = direction.both;
    //                return true;
    //            }
    //            direction = direction.right;
    //        }
    //    }
    //    return false;
    //}
    //private static List<Vector3>[] GetPostions(List<Vector3> positions, List<Vector3> childPositions, float width)
    //{
    //    List<Vector3>[] verts = new List<Vector3>[positions.Count];
    //    Vector3 direction = Vector3.forward;
    //    for (int i = 0; i < positions.Count; i++)
    //    {
    //        if (i < positions.Count - 1)
    //        {
    //            direction = positions[i + 1] - positions[i];
    //        }
    //        Vector3 right = Vector3.Cross(direction.normalized, Vector3.up).normalized;
    //        CheckPosition(positions[i], childPositions, direction, out direction direction1, width);
    //        if (direction1 == WayNodeMeshBuilder.direction.none)
    //        {
    //            verts[i] = new List<Vector3>
    //            {
    //                positions[i] - (right * (width / 2)),
    //                positions[i] + (right * (width / 2))
    //            };
    //        }
    //        else if (direction1 == WayNodeMeshBuilder.direction.right)
    //        {
    //            verts[i] = new List<Vector3>
    //            {
    //                positions[i] - (right * (width / 2)),
    //                positions[i]
    //            };
    //        }
    //        else if (direction1 == WayNodeMeshBuilder.direction.left)
    //        {
    //            verts[i] = new List<Vector3>
    //            {
    //                positions[i],
    //                positions[i] + (right * (width / 2))
    //            };
    //        }
    //    }
    //    return verts;
    //}
}

public class RunwayMeshBuilder
{
    public static Mesh Get(List<Vector3> positions, float width)
    {
        var mesh = new Mesh();
        List<Vector3> orderedVerts = new List<Vector3>();
        List<int> tris = new();
        List<Vector2> uvs = new();
        List<Vector3>[] verts = GetPostions(positions, width);
        for (int i = 0; i < verts.Length; i++)
        {
            if (i >= verts.Length - 1) break;
            for(int j = 0; j < verts[i].Count; j++)
            {
                if (j >= verts[i].Count - 1) break;
                tris.Add(orderedVerts.Count);
                orderedVerts.Add(verts[i][j]);
                uvs.Add(new Vector2(0, 0));
                tris.Add(orderedVerts.Count);
                orderedVerts.Add(verts[i][j + 1]);
                uvs.Add(new Vector2(1, 0));
                tris.Add(orderedVerts.Count);
                orderedVerts.Add(verts[i + 1][j]);
                uvs.Add(new Vector2(0, 1));

                tris.Add(orderedVerts.Count);
                orderedVerts.Add(verts[i][j + 1]);
                uvs.Add(new Vector2(1, 0));
                tris.Add(orderedVerts.Count);
                orderedVerts.Add(verts[i + 1][j + 1]);
                uvs.Add(new Vector2(1, 1));
                tris.Add(orderedVerts.Count);
                orderedVerts.Add(verts[i + 1][j]);
                uvs.Add(new Vector2(0, 1));
            }
        }
        mesh.SetVertices(orderedVerts.ToArray());
        mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs.ToArray());
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateUVDistributionMetrics();
        return mesh;
    }

    private static List<Vector3>[] GetPostions(List<Vector3> positions, float width)
    {
        List<Vector3>[] verts = new List<Vector3>[positions.Count];
        Vector3 direction = Vector3.forward;
        for (int i = 0; i < positions.Count; i++)
        {
            if (i < positions.Count - 1)
            {
                direction = positions[i + 1] - positions[i];
            }
            Vector3 right = Vector3.Cross(direction.normalized, Vector3.up).normalized;
            verts[i] = new List<Vector3>
            {
                positions[i] - (right * (width / 2)),
                positions[i] + (right * (width / 2))
            };
        }
        return verts;
    }
}

public class OSMMeshBuilder
{
    public static Mesh Get(List<Vector3> positions, float height)
    {
        Mesh wayMesh = new Mesh();
        Vector2[] vector2s = ConvertToVector2(positions);
        List<int> indices = new Triangulator(vector2s).Triangulate().ToList();

        if(height > 0)
        {
            AddHeight(ref positions, ref indices, height);
        }

        wayMesh.SetVertices(positions);
        wayMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        wayMesh.RecalculateBounds();
        wayMesh.RecalculateNormals();

        return wayMesh;
    }

    private static void AddHeight(ref List<Vector3> positions, ref List<int> indices, float height)
    {
        int count = positions.Count;
        for(int i = 0; i < count; i++)
        {
            positions.Add(positions[i]);
            indices.Add(positions.Count - 1);

            positions.Add(positions[i] + (Vector3.up * height));
            indices.Add(positions.Count - 1);

            positions.Add(positions[i + 1]);
            indices.Add(positions.Count - 1);

            positions.Add(positions[i + 1]);
            indices.Add(positions.Count - 1);

            positions.Add(positions[i] + (Vector3.up * height));
            indices.Add(positions.Count - 1);

            positions.Add(positions[i + 1] + (Vector3.up * height));
            indices.Add(positions.Count - 1);

            positions[i] += Vector3.up * height;
        }
    }

    private static Vector2[] ConvertToVector2(List<Vector3> vector3s)
    {
        Vector2[] vector2s = new Vector2[vector3s.Count];
        for(int i = 0; i < vector3s.Count; i++)
        {
            vector2s[i] = new Vector2(vector3s[i].x, vector3s[i].z);
        }
        return vector2s;
    }

    public static float GetAngle(Vector3 pA, Vector3 pB, Vector3 pC)
    {
        Vector3 v1 = pB - pA;
        Vector3 v2 = pC - pB;
        return Vector3.Angle(v1, v2);
    }
}

class Triangulator
{
    private List<Vector2> mPoints = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        mPoints = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = mPoints.Count;
        if (n < 3) return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = mPoints.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = mPoints[p];
            Vector2 qval = mPoints[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = mPoints[V[u]];
        Vector2 B = mPoints[V[v]];
        Vector2 C = mPoints[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = mPoints[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}
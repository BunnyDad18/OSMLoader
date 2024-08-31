using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public enum EndType
{
    None,
    Beginning,
    End
}

[Serializable]
public class Node
{
    public long id;
    public float lat;
    public float lon;
    public Vector3 virtualPosition;
    public List<(Vector3 direction, EndType end)> directions = new List<(Vector3, EndType)>();
    public Vector3 direction = Vector3.zero;
    public List<Way> ways = new List<Way>();

    public BezierKnot knot;

    public Dictionary<string, string> tags = new Dictionary<string, string>();

    internal void AddTag(string key, string value)
    {
        tags.Add(key, value);
    }
}

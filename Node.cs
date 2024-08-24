using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public long id;
    public float lat;
    public float lon;
    public Vector3 virtualPosition;
    public List<Way> ways = new List<Way>();
}

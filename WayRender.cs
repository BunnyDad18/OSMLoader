using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class WayRender : MonoBehaviour
{
    public Material lineMaterial;
    public Material meshMaterial;

    private OSMReader reader;
    private OSMReader Reader { get { if(reader == null) reader = GetComponent<OSMReader>(); return reader; } }

    private Vector3 _offset = Vector3.zero;

    public void SetOffset(float x, float y, float z)
    {
        _offset = new Vector3 (x, y, z);
    }

    public bool RenderWay(Way way, Transform parent)
    {
        Color wayColor = GetColor(way);

        if (way.type is WayType.Water or WayType.Grass or WayType.Wood or WayType.Wetland or WayType.Aerodrome)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = GetPositions(way);
            positions.RemoveAt(0);
            SetupMesh(newWayObject, positions, wayColor, 0);
            if(way.type == WayType.Aerodrome)
            {
                newWayObject.transform.position += Vector3.down * 0.01f;
            }
            return true;
        }
        if (way.type == WayType.Building)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = GetPositions(way);
            //SetupLineRenderer(newWayObject, positions, wayColor);
            positions.RemoveAt(0);

            SetupMesh(newWayObject, positions, wayColor, way.height);
            return true;
        }
        if (way.type is WayType.Other)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = GetPositions(way);
            SetupLineRenderer(newWayObject, positions, wayColor);
            return true;
        }
        if (way.type is WayType.Runway)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = GetPositions(way);
            SetupRunwayMesh(newWayObject, positions);
            return true;
        }
        //if (way.type is WayType.Taxiway)
        //{
        //    GameObject newWayObject = SetupWayGameObject(way, parent);
        //    List<Vector3> positions = GetPositions(way);
        //    SetupTaxiwayMesh(newWayObject, positions);
        //    return true;
        //}
        return false;
    }

    private Color GetColor(Way way)
    {
        if (way.type == WayType.Water) return Color.blue;
        if (way.type is WayType.Grass or WayType.Wood) return Color.green;
        if (way.type == WayType.Building) return Color.gray;
        if (way.type == WayType.Taxiway) return Color.yellow;
        if (way.type == WayType.Brownfield) return new Color(166f / 255f, 123f / 255f, 91f / 255f);
        if (way.type == WayType.Industrial) return new Color(119f / 255f, 176f / 255f, 170f / 255f);
        if (way.type == WayType.Wetland) return new Color(166f / 255f, 123f / 255f, 91f / 255f);
        if (way.type == WayType.Aerodrome) return new Color(.8f, .8f, .8f);
        if (way.type == WayType.Runway) return new Color(.3f, .3f, .3f);
        return Color.white;
    }

    private GameObject SetupWayGameObject(Way way, Transform parent)
    {
        GameObject newWayObject = new GameObject($"Way - {way.type}");
        newWayObject.transform.SetParent(parent);

        newWayObject.AddComponent<WayDataVisualiser>().SetData(way, Reader);
        return newWayObject;
    }

    public List<Vector3> GetPositions(Way way)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (long childElement in way.nodeIndexes)
        {
            Node node = Reader.nodes[childElement];
            Vector3 newPosition = new Vector3(node.lat, 0, node.lon);
            newPosition -= _offset;
            newPosition.z *= -1;
            newPosition *= 100000;
            node.virtualPosition = newPosition;
            if (!node.knotSet)
            {
                node.knot = new BezierKnot(newPosition);
                node.knotSet = true;
            }
            positions.Add(newPosition);
        }
        return positions;
    }

    public List<Node> GetNodes(Way way)
    {
        List<Node> positions = new List<Node>();
        foreach (long childElement in way.nodeIndexes)
        {
            Node node = Reader.nodes[childElement];
            Vector3 newPosition = new Vector3(node.lat, 0, node.lon);
            newPosition -= _offset;
            newPosition.z *= -1;
            newPosition *= 100000;
            node.virtualPosition = newPosition;
            if(!node.knotSet)
            {
                node.knot = new BezierKnot(newPosition);
                node.knotSet = true;
            }
            positions.Add(node);
        }
        return positions;
    }

    private void UpdateNodeDirections(Way way)
    {
        for(int i = 0; i < way.nodeIndexes.Count; i++)
        {
            Node node = Reader.nodes[way.nodeIndexes[i]];

            Vector3 forward = Vector3.forward;

            if (way.nodeIndexes.Count - 1 == i)
            {
                Node behindNode = Reader.nodes[way.nodeIndexes[i - 1]];
                forward = node.virtualPosition - behindNode.virtualPosition;
            }
            else if (i == 0)
            {
                Node infrontNode = Reader.nodes[way.nodeIndexes[i + 1]];
                forward = infrontNode.virtualPosition - node.virtualPosition;
            }
            else
            {
                Node behindNode = Reader.nodes[way.nodeIndexes[i - 1]];
                Node infrontNode = Reader.nodes[way.nodeIndexes[i + 1]];
                forward = node.virtualPosition - behindNode.virtualPosition;
                forward += infrontNode.virtualPosition - node.virtualPosition;
            }
            EndType type = EndType.None;
            if (i == 0) type = EndType.Beginning;
            else if (way.nodeIndexes.Count - 1 == i) type = EndType.End;
            node.directions.Add((forward.normalized, type));
        }
    }

    private void SetupLineRenderer(GameObject gameObject, List<Vector3> positions, Color color)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = MaterialLibrary.Instance.Lines;

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.startWidth = .1f;
        lineRenderer.endWidth = .1f;
    }

    private void SetupMesh(GameObject gameObject, List<Vector3> positions, Color color, float height = 0)
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = OSMMeshBuilder.Get(positions, height);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Instantiate(MaterialLibrary.Instance.Buildings);
        renderer.sharedMaterial.color = color;
    }

    private void SetupRunwayMesh(GameObject gameObject, List<Vector3> positions, float width = 45)
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = RunwayMeshBuilder.Get(positions, width);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Instantiate(MaterialLibrary.Instance.Runway);
    }

    private void SetupTaxiwayMesh(GameObject gameObject, Way way, float width = 20)
    {
        //foreach(KeyValuePair<long, Node> node in Reader.nodes)
        //{
        //    //if(node.Value.ways.Count > 1)
        //    //{
        //    //    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    //    marker.transform.position = node.Value.virtualPosition;
        //    //    marker.transform.localScale *= 2;
        //    //    marker.transform.SetParent(this.transform);
        //    //}
        //    Debug.Log($"Way count in node {node.Value.ways.Count}");
        //}

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = RunwayMeshBuilder.Get(way, _taxiwayNodes, width);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Instantiate(MaterialLibrary.Instance.Taxiway);
    }

    private Dictionary<long, Node> _taxiwayNodes = new Dictionary<long, Node>();

    internal void RenderTxiways(List<Way> taxiways)
    {
        _taxiwayNodes.Clear();
        foreach (Way way in taxiways)
        {
            GetPositions(way);
            UpdateNodeDirections(way);
        }
        foreach (KeyValuePair<long, Node> nodes in Reader.nodes)
        {
            int wayCount = nodes.Value.ways.Count;
            foreach(Way testWay in nodes.Value.ways)
            {
                if(testWay.type != WayType.Taxiway)
                {
                    wayCount--;
                }
            }
            if(wayCount > 0)
            {
                _taxiwayNodes.Add(nodes.Key,nodes.Value);
                nodes.Value.direction = nodes.Value.directions[0].direction;
            }
            if (wayCount > 1)
            {
                CreateMarker(nodes.Value, wayCount);
                bool isMiddle = false;
                Vector3 newDirection = Vector3.zero;
                foreach(var (direction, end) in nodes.Value.directions)
                {
                    if(end == EndType.None) isMiddle = true;

                    if(isMiddle)
                    {
                        nodes.Value.direction = direction;
                        break;
                    }

                    float difference = (newDirection.normalized - direction.normalized).magnitude;

                    if(difference > 1)
                    {
                        newDirection -= direction;
                    }
                    else
                    {
                        newDirection += direction;
                    }
                }
                if (!isMiddle)
                {
                    nodes.Value.direction = (newDirection / nodes.Value.directions.Count).normalized;
                }
            }
        }
        foreach (Way way in taxiways)
        {
            GameObject newWayObject = SetupWayGameObject(way, transform);
            SetupTaxiwayMesh(newWayObject, way);
        }
    }

    private void CreateMarker(Node node, float scale)
    {
        if (splineGen == null)
        {
            splineGen = new SplineGen(this.transform);
        }
        splineGen.Add(node);
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.position = node.virtualPosition;
        marker.transform.localScale *= scale;
        marker.transform.SetParent(this.transform);
        marker.AddComponent<NodeVisualizer>().SetNode(node);
    }
    SplineGen splineGen;
}

public class SplineGen
{
    private Transform _parent;
    private SplineContainer container; 

    public SplineGen(Transform parent)
    {
        _parent = parent;
        GameObject splineHolder = new GameObject("Taxiway Spline");
        splineHolder.transform.parent = parent;
        container = splineHolder.AddComponent<SplineContainer>();
    }

    internal void Add(Node node)
    {
        BezierKnot newKnot = new()
        {
            Position = node.virtualPosition
        };
        //container.
    }
}

public class NodeVisualizer : MonoBehaviour
{
    public List<string> tags = new List<string>();
    [SerializeField] private Node node;

    public void SetNode(Node node)
    {
        this.node = node;
        foreach(var tag in node.tags)
        {
            tags.Add($"{tag.Key} : {tag.Value}");
        }
    }
}
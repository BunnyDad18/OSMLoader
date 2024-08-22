using System;
using System.Collections.Generic;
using UnityEngine;
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
        if (way.type is WayType.Taxiway)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = GetPositions(way);
            SetupTaxiwayMesh(newWayObject, positions);
            return true;
        }
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

        newWayObject.AddComponent<WayDataVisualiser>().SetData(way);
        return newWayObject;
    }

    private List<Vector3> GetPositions(Way way)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (long childElement in way.nodeIndexes)
        {
            Node node = Reader.nodes[childElement];
            Vector3 newPosition = new Vector3(node.lat, 0, node.lon);
            newPosition -= _offset;
            newPosition.z *= -1;
            newPosition *= 100000;
            positions.Add(newPosition);
        }
        return positions;
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

    private void SetupTaxiwayMesh(GameObject gameObject, List<Vector3> positions, float width = 23)
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = RunwayMeshBuilder.Get(positions, width);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Instantiate(MaterialLibrary.Instance.Taxiway);

        foreach (Vector3 position in positions)
        {
            if(taxiwayPosition.Contains(position))
            {
                GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
                node.transform.SetParent(transform, true);
                node.transform.position = position;
            }
            taxiwayPosition.Add(position);
        }
    }

    private void SetupTaxiwayMesh(GameObject gameObject, WayNode wayNode, float width = 23)
    {
        if (wayNode.rendered) return;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = WayNodeMeshBuilder.Get(wayNode, width, GetPositions);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = Instantiate(MaterialLibrary.Instance.Taxiway);
        wayNode.rendered = true;
    }

    private List<Vector3> taxiwayPosition = new List<Vector3>();

    private List<Way> selectedTaxiway = new List<Way>();

    public List<WayNode> _wayTrees = new List<WayNode>();

    internal void RenderTxiways(List<Way> ways)
    {
        selectedTaxiway.Clear();
        taxiwayPosition.Clear();
        _wayTrees.Clear();

        foreach (Way way in ways)
        {
            if (selectedTaxiway.Contains(way)) continue;
            WayNode currentNode = new WayNode();
            currentNode.way = way;
            selectedTaxiway.Add(way);
            PopulateNode(currentNode, ways);
            _wayTrees.Add(currentNode);
        }
        Debug.Log($"Number of way trees = {_wayTrees.Count}");

        for(int i = 0; i < _wayTrees.Count; i++)
        {
            GameObject newWayObject = SetupWayGameObject(_wayTrees[i].way, transform);
            SetupTaxiwayMesh(newWayObject, _wayTrees[i]);
            for(int j = 0; j < _wayTrees[i].connectedWays.Count; j++)
            {
                GameObject newWayChildObject = SetupWayGameObject(_wayTrees[i].connectedWays[j].way, transform);
                SetupTaxiwayMesh(newWayChildObject, _wayTrees[i].connectedWays[j]);
            }
        }
    }

    private void LoopWayNodes(List<WayNode> wayNodes)
    {
        for (int i = 0; i < wayNodes.Count; i++)
        {
            if(wayNodes[i].rendered) continue;
            GameObject newWayObject = SetupWayGameObject(wayNodes[i].way, transform);
            SetupTaxiwayMesh(newWayObject, wayNodes[i]);
            LoopWayNodes(wayNodes[i].connectedWays);
        }
    }

    private void PopulateNode(WayNode currentNode, List<Way> ways)
    {
        List<Vector3> positions = GetPositions(currentNode.way);
        foreach (Way childWay in ways)
        {
            List<Vector3> childPositions = GetPositions(childWay);
            foreach (var childPosition in childPositions)
            {
                if (!positions.Contains(childPosition)) continue;
                if (selectedTaxiway.Contains(childWay)) continue;
                currentNode.connectedWays.Add(new WayNode() { way = childWay, connectedWays = new List<WayNode>() {  } });
                selectedTaxiway.Add(childWay);
                break;
            }
        }
        foreach (WayNode childNode in currentNode.connectedWays)
        {
            PopulateNode(childNode, ways);
        }
    }
}

public class WayNode
{
    public Way way;
    public List<WayNode> connectedWays = new List<WayNode>();
    public bool connected = false;
    public bool rendered = false;
}
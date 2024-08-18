using System.Collections.Generic;
using UnityEngine;

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

        if (way.type is WayType.Water or WayType.Grass or WayType.Wood or WayType.Wetland or WayType.Aerodrome or WayType.Runway)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = new List<Vector3>();
            AddPositions(ref positions, way);
            positions.RemoveAt(0);
            SetupMesh(newWayObject, positions, wayColor, 0);
            if(way.type == WayType.Aerodrome)
            {
                newWayObject.transform.position += Vector3.down * 0.1f;
            }
            return true;
        }
        if (way.type == WayType.Building)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = new List<Vector3>();
            AddPositions(ref positions, way);
            //SetupLineRenderer(newWayObject, positions, wayColor);
            positions.RemoveAt(0);

            SetupMesh(newWayObject, positions, wayColor, way.height);
            return true;
        }
        if (way.type is WayType.Other or WayType.Taxiway)
        {
            GameObject newWayObject = SetupWayGameObject(way, parent);
            List<Vector3> positions = new List<Vector3>();
            AddPositions(ref positions, way);
            SetupLineRenderer(newWayObject, positions, wayColor);
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
        GameObject newWayObject = new GameObject($"Way");
        newWayObject.transform.SetParent(parent);

        WayData wayData = newWayObject.AddComponent<WayData>();
        wayData.data.Add($"ID = {way.id}");
        wayData.type = way.type;
        foreach (KeyValuePair<string, string> tag in way.tags)
        {
            wayData.data.Add($"{tag.Key} - {tag.Value}");
        }
        return newWayObject;
    }

    private void AddPositions(ref List<Vector3> positions, Way way)
    {
        foreach (long childElement in way.nodeIndexes)
        {
            Node node = Reader.nodes[childElement];
            Vector3 newPosition = new Vector3(node.lat, 0, node.lon);
            newPosition -= _offset;
            newPosition.z *= -1;
            newPosition *= 100000;
            positions.Add(newPosition);
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
}
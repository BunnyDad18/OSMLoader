using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class OSMReader : MonoBehaviour
{
    public TextAsset osmData;
    public int lineCount = 2000;
    public Transform mapParent;
    public Transform MapParent { get
        {
            if(mapParent == null)
            {
                mapParent = transform;
            }
            return mapParent;
        }
    }
    public string dataPath;
    public int nodeCount;
    public int wayCount;

    public bool limitIDs = false;

    public List<long> excludeIDs;
    public List<long> exclusiveIDs;

    public List<string> skipKeys = new List<string>();

    public Dictionary<long, Node> nodes = new Dictionary<long, Node>();
    public List<Way> ways = new List<Way>();

    public List<(string, string)> keyValueSkip = new List<(string, string)> ();

    private WayRender render;
    private WayRender Render { get { if (render == null) render = GetComponent<WayRender>(); return render; } }

    private Vector3 _offset = new Vector3(51.508f, 0, 0.070f);

    public void ShowMap(string data)
    {
        XDocument document = XDocument.Parse(data);
        PopulateNodes(document);
        PopulateWays(document);
        GetComponent<WayRender>().SetOffset(_offset.x, _offset.y, _offset.z);
        PopulateWays();
    }

    public void Import()
    {
        XDocument document = XDocument.Parse(osmData.text);
        PopulateNodes(document);
        PopulateWays(document);
        GetComponent<WayRender>().SetOffset(_offset.x, _offset.y, _offset.z);
    }

    private void PopulateNodes(XDocument document)
    {
        nodes = new Dictionary<long, Node>();
        nodeCount = 0;
        foreach (XElement element in document.Descendants("node"))
        {
            Node newNode = new()
            {
                id = long.Parse(element.Attribute("id").Value),
                lat = float.Parse(element.Attribute("lat").Value),
                lon = float.Parse(element.Attribute("lon").Value)
            };
            if (nodes.ContainsKey(newNode.id)) continue;
            PopulateTags(element, newNode);
            nodes.Add(newNode.id, newNode);
            nodeCount++;
            _offset = new Vector3(newNode.lat, 0, newNode.lon);
        }
    }

    private void PopulateWays(XDocument document)
    {
        ways = new List<Way>();
        foreach (XElement element in document.Descendants("way"))
        {
            Way newWay = new()
            {
                id = long.Parse(element.Attribute("id").Value)
            };
            PopulateTags(element, newWay);
            PopulateNodes(element, newWay);
            ways.Add(newWay);
        }
        wayCount = ways.Count;
    }

    private static void PopulateTags(XElement element, Way newWay)
    {
        foreach (XElement tag in element.Descendants("tag"))
        {
            string key = tag.Attribute("k").Value.ToLower();
            string value = tag.Attribute("v").Value.ToLower();
            newWay.AddTag(key, value);
        }
        newWay.height *= 20;
    }

    private static void PopulateTags(XElement element, Node newNode)
    {
        foreach (XElement tag in element.Descendants("tag"))
        {
            string key = tag.Attribute("k").Value.ToLower();
            string value = tag.Attribute("v").Value.ToLower();
            newNode.AddTag(key, value);
        }
    }

    private void PopulateNodes(XElement element, Way newWay)
    {
        foreach (XElement nodes in element.Descendants("nd"))
        {
            long nodeIndex = long.Parse(nodes.Attribute("ref").Value);
            if (!this.nodes[nodeIndex].ways.Contains(newWay))
            {
                newWay.nodeIndexes.Add(nodeIndex);
                this.nodes[nodeIndex].ways.Add(newWay);
            }
        }
    }

    public void DestroyChildern()
    {
        for(int i = MapParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(MapParent.GetChild(i).gameObject);
        }
    }

    private bool CheckSkipTags(Dictionary<string, string> tags)
    {
        foreach(KeyValuePair<string, string> tag in tags)
        {
            if(skipKeys.Contains(tag.Key)) return true;
        }
        return false;
    }

    private bool SkipWay(Way way)
    {
        if (way.tags.Count == 0) return true;
        if (CheckSkipTags(way.tags)) return true;
        if (limitIDs && !exclusiveIDs.Contains(way.id)) return true;
        if (limitIDs && excludeIDs.Contains(way.id)) return true;
        return false;
    }

    private List<Way> taxiways = new List<Way>();

    private bool AddTaxiway(Way way)
    {
        if(way.type == WayType.Taxiway)
        {
            taxiways.Add(way);
            AddToSpline(way);
            return true;
        }
        return false;
    }

    public void PopulateWays()
    {
        DestroyChildern();
        int offset = 0;
        if (ways.Count == 0) Import();

        CreateSplineObject();

        for(int i = 0; i < lineCount; i++)
        {
            int index = i + offset;
            if (index >= ways.Count) break;
            Way way = ways[index];
            if (SkipWay(way) || AddTaxiway(way) || true|| !Render.RenderWay(way, MapParent))
            {
                offset++;
                i--;
                continue;
            }
        }
        //Render.RenderTxiways(taxiways);
        //MapParent.name = $"Main - {MapParent.childCount}";
    }

    private SplineContainer _splineContainer;

    private void CreateSplineObject()
    {
        _splineContainer = new GameObject("Taxiway Spline").AddComponent<SplineContainer>();
        _splineContainer.transform.parent = transform;
        _splineContainer.gameObject.AddComponent<SplineMeshGen>();
    }

    private void AddToSpline(Way way)
    {
        if (_splineContainer == null) return;
        List<Node> wayNodes = GetComponent<WayRender>().GetNodes(way);
        Spline newSpline = _splineContainer.AddSpline();
        foreach (Node node in wayNodes)
        {
            newSpline.Add(node.knot, TangentMode.AutoSmooth, 1);
        }
    }
}

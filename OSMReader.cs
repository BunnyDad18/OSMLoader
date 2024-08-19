using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

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

    private void PopulateNodes(XElement element, Way newWay)
    {
        foreach (XElement nodes in element.Descendants("nd"))
        {
            long nodeIndex = long.Parse(nodes.Attribute("ref").Value);
            newWay.nodeIndexes.Add(nodeIndex);
            this.nodes[nodeIndex].ways.Add(newWay);
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

    public void PopulateWays()
    {
        DestroyChildern();
        int offset = 0;
        if (ways.Count == 0) Import();

        for(int i = 0; i < lineCount; i++)
        {
            int index = i + offset;
            if (index >= ways.Count) break;
            Way way = ways[index];
            if (SkipWay(way) || !Render.RenderWay(way, MapParent))
            {
                offset++;
                i--;
                continue;
            }
        }
        //MapParent.name = $"Main - {MapParent.childCount}";
    }
}

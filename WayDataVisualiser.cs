using System.Collections.Generic;
using UnityEngine;

public class WayDataVisualiser : MonoBehaviour
{
    public Way Data { get; private set; }

    public string id;
    public WayType type;
    public List<string> data = new List<string>();

    private OSMReader reader;

    public void SetData(Way way, OSMReader reader)
    {
        Data = way;
        type = way.type;
        id = way.id.ToString();
        foreach (KeyValuePair<string, string> tag in way.tags)
        {
            data.Add($"{tag.Key} - {tag.Value}");
        }
        this.reader = reader;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (Data == null) return;
        foreach(var node in Data.nodeIndexes)
        {
            Gizmos.DrawLine(reader.nodes[node].virtualPosition, reader.nodes[node].virtualPosition + reader.nodes[node].direction);
        }
    }
}

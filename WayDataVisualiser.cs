using System.Collections.Generic;
using UnityEngine;

public class WayDataVisualiser : MonoBehaviour
{
    public Way Data { get; private set; }

    public string id;
    public WayType type;
    public List<string> data = new List<string>();

    public void SetData(Way way)
    {
        Data = way;
        type = way.type;
        id = way.id.ToString();
        foreach (KeyValuePair<string, string> tag in way.tags)
        {
            data.Add($"{tag.Key} - {tag.Value}");
        }
    }
}

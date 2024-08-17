using UnityEngine;
using System.Collections.Generic;

public class DebugTags : MonoBehaviour
{
    private static DebugTags instance;
    public static DebugTags Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DebugTags>();
            }
            if (instance == null)
            {
                instance = new GameObject("Debug_Way_Tags").AddComponent<DebugTags>();
            }
            return instance;
        }
    }

    public List<string> keys = new List<string>();

    public void AddKey(string key)
    {
        if (keys.Contains(key)) return;
        keys.Add(key);
    }
}

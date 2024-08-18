using UnityEngine;

public class MaterialLibrary : MonoBehaviour
{
    public static MaterialLibrary _instance;
    public static MaterialLibrary Instance
    {
        get
        {
            if(_instance == null)
                _instance = Transform.FindAnyObjectByType<MaterialLibrary>();
            return _instance;
        }
    }

    [SerializeField] private Material _buildings;
    [SerializeField] private Material _lines;

    public Material Buildings => _buildings;
    public Material Lines => _lines;
}

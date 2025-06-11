using UnityEngine;
using System.Collections.Generic;

public class TilePathManager : MonoBehaviour
{
    public static TilePathManager Instance;

    public List<Transform> tilePath = new List<Transform>();

    void Awake()
    {
        Instance = this;

        // Automatically fill tilePath from children
        foreach (Transform child in transform)
        {
            tilePath.Add(child);
        }
    }

    public List<Transform> GetPath()
    {
        return tilePath;
    }
}

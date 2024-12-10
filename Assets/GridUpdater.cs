using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GridUpdater : MonoBehaviour
{
    GraphUpdateScene[] comps;
    // Start is called before the first frame update
    void Start()
    {
        comps = GetComponents<GraphUpdateScene>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var comp in comps)
            comp.Apply();
    }
}

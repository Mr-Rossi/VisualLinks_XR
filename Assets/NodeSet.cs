using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;
using Valve.VR;
using Pathfinding;
using System.Threading;
using System.Threading.Tasks;

public class NodeSet : MonoBehaviour
{
    public GameObject start;
    public GameObject end;

    public bool ShouldSmoothe = false;
    public float Smoothness = 1.0f;

    private Vector3 direction;

    private PathfindingTestScript manager;

    private void UpdateNodes()
    {
        direction = end.transform.position - start.transform.position;

        if (manager.mode != PathfindingTestScript.LinkMode.Straight)
        {
            if (AstarPath.active == null) return;

            var p = ABPath.Construct(start.transform.position + Vector3.up * 0.26f, end.transform.position, OnPathFound);

            // Start the path by calling the AstarPath component directly
            // AstarPath.active is the active AstarPath instance in the scene
            AstarPath.StartPath(p);
        }
        else
        {

            //debugSpheres.Clear();
            for (int i = 0; i < spline.nodes.Count; i++)
            {
                spline.nodes[i].Position = start.transform.position + ((float)i / (spline.nodes.Count - 1)) * direction;
                spline.nodes[i].Direction = direction * (i + 1);
                spline.nodes[i].Up = Vector3.up;
                spline.nodes[i].Roll = 0;
                spline.nodes[i].Scale = new Vector3(0.05f + i * 0.05f, 0.05f + i * 0.05f, 0.05f + i * 0.05f);
            }
        }
    }
    
    private void OnPathFound(Path p)
    {
        for (int i = 1; i < spline.nodes.Count - 1;)
        {
            spline.RemoveNode(spline.nodes[i]);
        }
        if (!ShouldSmoothe)
        {
            for (int i = 0; i < p.path.Count; i++)
            {
                spline.InsertNode(spline.nodes.Count - 1, new SplineNode((Vector3)(p.path[i].position), direction));
            }
        }
        else
        {
            List<Vector3> splineToSmoothe = new List<Vector3>();
            for (int i = 0; i < p.path.Count; i++)
            {
                splineToSmoothe.Add((Vector3)(p.path[i].position));
            }

            splineToSmoothe = Curver.MakeSmoothCurve(splineToSmoothe, Smoothness);
            for (int i = 0; i < splineToSmoothe.Count; i++)
            {
                spline.InsertNode(spline.nodes.Count - 1, new SplineNode(splineToSmoothe[i], direction));
            }
        }

        /*foreach (var node in spline.nodes)
        {
            node.Scale = Vector3.one * 0.1f;
        }*/

        for (int i = 0; i < spline.nodes.Count; i++)
        {
            spline.nodes[i].Scale = Vector3.one * (0.2f / spline.nodes.Count * i + 0.03f);
        }
        var smoother = GetComponent<SplineSmoother>();
        smoother.curvature = 0.1f;
        smoother.enabled = false;
        smoother.enabled = true;

        Debug.Log("done at " + Time.realtimeSinceStartup);
        if (MenuHelper.testRun) {
            MenuHelper.StopStopwatch();
            MenuHelper.RecordLoadTime(gameObject.name, p.path.Count);
        }
    }

    private Spline spline;


    // Start is called before the first frame update
    void Start()
    {
        if(!start)
        {
            ObjectSelector os = FindObjectOfType<ObjectSelector>();
            start = os.FindObjectInWim(os.wimParent, end.transform).gameObject;
        }
        spline = GetComponent<Spline>();
        var splinemeshtiling = GetComponent<SplineMeshTiling>();
        if (splinemeshtiling.material)
        {
            splinemeshtiling.material = Instantiate(splinemeshtiling.material);
            splinemeshtiling.material.color = UnityEngine.Random.ColorHSV();
        }
        manager = FindObjectOfType<PathfindingTestScript>();
        manager.OnUpdated += UpdateNodes;
    }

    private void OnDestroy()
    {
        if(manager)
            manager.OnUpdated -= UpdateNodes;
    }


    private float updateTimer;

    public float UpdateThreshold = 0.1f;

    // Update is called once per frame
    void Update()
    {
        //#if !UNITY_ANDROID
        updateTimer += Time.deltaTime;
        if (updateTimer >= UpdateThreshold)
        {
            updateTimer-= UpdateThreshold;
            spline.nodes[0].Position = start.transform.position;
            spline.nodes[spline.nodes.Count - 1].Position = end.transform.position;
        }
//#endif
    }


}

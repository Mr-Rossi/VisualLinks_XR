using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ObjectSelector : MonoBehaviour
{
    public SteamVR_Action_Boolean selectAction;
    public GameObject rightHand;
    public GameObject cylinder;
    public GameObject tentaclePrefab;
    public Transform linkParent;
    public Transform wimParent;

    private bool linksCurveSpace;
    private float smoothness = 2.0f;
    private float updateThreshold = 0.1f;

    public bool LinksCurveSpace { get => linksCurveSpace; set => linksCurveSpace = value; }
    public float Smoothness { get => smoothness; internal set => smoothness = value; }
    public float UpdateThreshold { get => updateThreshold; set => updateThreshold = value; }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        selectAction.AddOnChangeListener(OnSelectActionChange, SteamVR_Input_Sources.Any);
    }

    private void OnDisable()
    {
        selectAction.RemoveOnChangeListener(OnSelectActionChange, SteamVR_Input_Sources.Any);
    }

    private void OnSelectActionChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if (newState == true)
        {
            EnterDownState();
        }
        else
        {
            EnterUpState();
        }
    }

    public void EnterDownState()
    {
        cylinder.SetActive(true);
    }

    public void EnterUpState()
    {
        Debug.Log("Selecting Object");
        RaycastHit hitinfo;
        Physics.Raycast(rightHand.transform.position, rightHand.transform.forward, out hitinfo);
        if (hitinfo.transform)
        {
            Debug.Log("hit " + hitinfo.transform.name);
            var button = hitinfo.transform.GetComponent<VisualLinksButton>();
            if (button)
            {
                button.OnClick();
            }
            else
            {
                var target = FindObjectInWim(wimParent, hitinfo.transform);
                if(target)
                {
                    SpawnLink(hitinfo.transform, target);
                }
            }
        }
        else
        {
            Debug.Log("no hit");
        }
        cylinder.SetActive(false);
    }

    private void SpawnLink(Transform start, Transform end)
    {
        Debug.Log("found matching object, spawning link");
        GameObject go = Instantiate(tentaclePrefab, linkParent);
        NodeSet spline = go.GetComponent<NodeSet>();
        spline.start = start.gameObject;
        spline.end = end.gameObject;
        spline.Smoothness = smoothness;
        if (smoothness > 0)
            spline.ShouldSmoothe = true;
        else
            spline.ShouldSmoothe = false;
        spline.UpdateThreshold = updateThreshold;
        go.GetComponent<SplineMesh.SplineMeshTiling>().curveSpace = linksCurveSpace;
    }

    public Transform FindObjectInWim(Transform parent, Transform target, bool recursive = true)
    {
        if (target.parent.name == parent.name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == target.name && child.transform.localPosition == target.localPosition)
                {
                    return child;
                }
            }
        }
        else if(recursive)
        {
            foreach (Transform subParent in parent)
            {
                var retvalue = FindObjectInWim(subParent, target);
                if (retvalue)
                    return retvalue;
            }
        }
        return null;
    }
}

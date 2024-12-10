using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestInputManager : MonoBehaviour
{


    public int count;
    public int samples = 100;
    public float totalTime;

    private PathfindingTestScript manager;

    // Start is called before the first frame update
    void Start()
    {
        count = samples;
        totalTime = 0f;
        manager = FindObjectOfType<PathfindingTestScript>();
    }


    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            manager.NextMode();
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
            GetComponent<ObjectSelector>().EnterDownState();
        if (OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger))
            GetComponent<ObjectSelector>().EnterUpState();

        count -= 1;
        totalTime += Time.deltaTime;

        if (count <= 0)
        {
            float fps = samples / totalTime;
            Debug.Log("average fps: " + fps); // your way of displaying number. Log it, put it to text object…
            totalTime = 0f;
            count = samples;
        }
    }
}

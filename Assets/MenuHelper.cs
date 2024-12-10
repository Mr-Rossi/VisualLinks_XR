using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.SpatialTracking;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Globalization;

public class MenuHelper : MonoBehaviour
{
    public Transform LinksParent;
    public ObjectSelector ObjectSelector;
    private PathfindingTestScript manager;
    private List<VisualLinksButton> presetbuttons;

    public Vector3[] CameraTrackPoints;
    public Vector3[] CameraRotations;
    public Transform[] LinkEndPoints;
    public NodeSet LinkPrefab;
    //public Transform PresetLinksParent;
    private static MenuHelper Instance;
    private UnityEngine.UI.Text ProgressText;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        ProgressText = GameObject.Find("RunProgressText").GetComponent<UnityEngine.UI.Text>();
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        stopwatch = new Stopwatch();
        manager = FindObjectOfType<PathfindingTestScript>();
        presetbuttons = FindObjectsOfType<VisualLinksButton>().Where(x => x.name.StartsWith("LinksPreset")).OrderBy(x => x.name).ToList();
        presetbuttons.RemoveAt(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (testRun)
        {
            testRunWriter.WriteLine(Time.deltaTime); 
        }
        /*if(timeUntilNextTestRun>0)
        {
            timeUntilNextTestRun -= Time.deltaTime;
            if (timeUntilNextTestRun <= 0)
                DoCompleteTestRuns();
        }*/
    }

    public void ToggleLinksCurveSpace()
    {
        ObjectSelector.LinksCurveSpace = !ObjectSelector.LinksCurveSpace;
        var children = LinksParent.GetComponentsInChildren<SplineMesh.SplineMeshTiling>();

        foreach (var child in children)
        {
            child.curveSpace = ObjectSelector.LinksCurveSpace;
        }
    }

    public void SetBezierSmoothness(float smoothness)
    {
        ObjectSelector.Smoothness = smoothness;
        var children = LinksParent.GetComponentsInChildren<NodeSet>();

        foreach (var child in children)
        {
            child.Smoothness = smoothness;
            if (smoothness > 0)
                child.ShouldSmoothe = true;
            else
                child.ShouldSmoothe = false;
        }
    }

    public void SetUpdateThreshold(float threshold)
    {
        ObjectSelector.UpdateThreshold = threshold;
        var children = LinksParent.GetComponentsInChildren<NodeSet>();

        foreach (var child in children)
        {
            child.UpdateThreshold = threshold;
        }
    }

    public void SetLinkPreset(Transform preset)
    {
        foreach (Transform link in LinksParent)
        {
            Destroy(link.gameObject);
        }
        foreach(Transform link in preset)
        {
            Instantiate(link.gameObject, LinksParent).SetActive(true);
        }
    }

    public void SetLinkAmount(int amount)
    {
        foreach (Transform link in LinksParent)
        {
            Destroy(link.gameObject);
        }
        for (int i = 0; i < amount; i++)
        {
            LinkPrefab.gameObject.name = "Tentacle" + i;
            LinkPrefab.end = LinkEndPoints[LinkEndPoints.Length / amount * i].gameObject;
            Instantiate(LinkPrefab.gameObject, LinksParent).SetActive(true);
        }
    }

    public void SwitchScene(string scene)
    {
        var player = FindObjectOfType<Valve.VR.InteractionSystem.Player>();
        Destroy(player.gameObject);
        SceneManager.LoadScene(scene);
    }

    public void FollowCameraTrack()
    {
#if UNITY_ANDROID
        Debug.Log("not implemented yet");
        return;
#else

        var player = FindObjectOfType<Valve.VR.InteractionSystem.Player>();
        player.hmdTransform.GetComponent<TrackedPoseDriver>().enabled = false;
        player.hmdTransform.localPosition = Vector3.up*2f;
        Sequence cameraTrack = DOTween.Sequence();
        for (int i = 0; i < CameraTrackPoints.Length; i++)
        {
            cameraTrack.Append(player.transform.DOMove(CameraTrackPoints[i], 5f));
            cameraTrack.Join(player.hmdTransform.DORotate(CameraRotations[i], 5f));
        }
        cameraTrack.OnComplete(()=>
        {
            player.hmdTransform.GetComponent<TrackedPoseDriver>().enabled = true;
            FinishTestRun();
        });
        cameraTrack.Play();
#endif
    }


    private static bool completeRun = false;

    bool curve = false;
    int completeRunCount = 0;
    float timeUntilNextTestRun;

    bool runwithextralinks;

    public void DoCompleteTestRuns(bool extralinks = false)
    {
        if (completeRunCount >= 400)
        {
            completeRun = false;
            completeRunCount = 0;
            return;
        }
        runwithextralinks = extralinks;

        ProgressText.text = "Progress: " + completeRunCount + "/400";

        completeRun = true;
        if (curve)
        {
            ToggleLinksCurveSpace();
            curve = false;
        }
        else
        {
            ToggleLinksCurveSpace();
            curve = true;
        }

        SetBezierSmoothness(completeRunCount / 100);
        if(!extralinks)
            presetbuttons[completeRunCount % 10].OnClick();
        else
        {
            SetLinkAmount(13 + (completeRunCount % 10) * 5);
        }
        DOVirtual.DelayedCall(3, DoCompleteTestRunsSecondPart);
        //PathfindingTestScript.LinkMode tempmode = (PathfindingTestScript.LinkMode)(completeRunCount % 100 / 10);
        //if(manager.mode != tempmode)
        //    manager.mode = tempmode;

        //UnityEngine.Debug.Log("count " + completeRunCount + "curve " + curve);
        ////var tween = DOVirtual.DelayedCall(5, DoTestRun);
        //DoTestRun();
        //if (!curve)
        //{
        //    completeRunCount++;
        //    if(completeRunCount % 10 >= 3)
        //    {
        //        completeRunCount += 7;
        //    }
        //}
        ///*if (completeRunCount < 400)
        //    DoCompleteTestRuns();
        //else
        //    completeRunCount = 0;*/
    }

    private void DoCompleteTestRunsSecondPart()
    {

        PathfindingTestScript.LinkMode tempmode = (PathfindingTestScript.LinkMode)(((completeRunCount % 100) / 10)%5);
        if (manager.mode != tempmode)
            manager.mode = tempmode;

        UnityEngine.Debug.Log("count " + completeRunCount + "curve " + curve);
        //var tween = DOVirtual.DelayedCall(5, DoTestRun);
        DoTestRun();
        if (!curve)
        {
            completeRunCount++;
            if (completeRunCount % 10 >= 3)
            {
                completeRunCount += 7;
            }
        }
        /*if (completeRunCount < 400)
            DoCompleteTestRuns();
        else
            completeRunCount = 0;*/
    }


    //private int count;
    //private int samples = 100;
    //private float totalTime;
    public static bool testRun = false;
    private static StreamWriter testRunWriter;
    private static Stopwatch stopwatch;
    private static Dictionary<string, List<double>> recordedValues;
    private static DirectoryInfo currentDirectory;

    public void DoTestRun()
    {
        recordedValues = new Dictionary<string, List<double>>();
        currentDirectory = Directory.CreateDirectory(Application.persistentDataPath + "/" + System.DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"));

        testRunWriter = new StreamWriter(currentDirectory.FullName + "/testinfo.csv");
        testRunWriter.WriteLine("Device;Scene;Mode;MeshPerCurve;Smoothness;UpdateThreshold;AmountOfLinks");
        //testRunWriter.WriteLine("Device: Vive");
        //testRunWriter.WriteLine("Scene: " + SceneManager.GetActiveScene().name);
        //testRunWriter.WriteLine("Mode: " + manager.mode.ToString());
        //testRunWriter.WriteLine("MeshPerCurve: " + ObjectSelector.LinksCurveSpace);
        //testRunWriter.WriteLine("Smoothness: " + ObjectSelector.Smoothness);
        //testRunWriter.WriteLine("Update Threshold: " + ObjectSelector.UpdateThreshold);
        //testRunWriter.WriteLine("Amount of Links: " + LinksParent.childCount);
        testRunWriter.WriteLine("{0};{1};{2};{3};{4};{5};{6}", "vive", SceneManager.GetActiveScene().name, manager.mode.ToString(),
            ObjectSelector.LinksCurveSpace, ObjectSelector.Smoothness, ObjectSelector.UpdateThreshold, LinksParent.childCount);
        testRunWriter.Close();
        testRunWriter = new StreamWriter(currentDirectory.FullName + "/frametimes.csv");
        testRunWriter.WriteLine("deltatime");
        testRun = true;
        FollowCameraTrack();
    }

    public void FinishTestRun()
    {
        if(testRun)
        {
            testRunWriter.Close();
            testRunWriter = new StreamWriter(currentDirectory.FullName + "/loadtimes.csv");
            foreach (var key in recordedValues.Keys)
            {
                testRunWriter.Write(key);
                if (key != recordedValues.Keys.Last())
                    testRunWriter.Write(";");
                else
                    testRunWriter.WriteLine();
            }
            //foreach(var values in recordedValues.Values)

            int maxSize = recordedValues.Values.Max(a => a != null ? a.Count : 0);

            //Console.WriteLine(String.Join(", ", names));

            for (int i = 0; i < maxSize; i++)
            {
                foreach (string name in recordedValues.Keys)
                {
                    var value = recordedValues[name];

                    if ((value != null) && (i < value.Count))
                    {
                        testRunWriter.Write(value[i]);
                    }

                    if (name != recordedValues.Keys.Last())
                    {
                        testRunWriter.Write(";");
                    }
                }
                testRunWriter.WriteLine();
            }
            testRunWriter.Close();
            testRun = false;

            if (completeRun)
            {
                //timeUntilNextTestRun = 5;
                DoCompleteTestRuns(runwithextralinks);
                //var tween = DOVirtual.DelayedCall(5, DoCompleteTestRuns);
            }
        }
    }
    
    public static void WriteToTestLog(string s)
    {
        testRunWriter.WriteLine(s);
    }

    private static int loadtimeCounter = 0;

    public static void StartStopwatch()
    {
        loadtimeCounter = 0;
        stopwatch.Reset();
        stopwatch.Start();
    }

    public static void StopStopwatch()
    {
        loadtimeCounter++;
        if (loadtimeCounter >= Instance.LinksParent.childCount)
        {
            stopwatch.Stop();
            //WriteToTestLog("node refresh " + stopwatch.Elapsed.TotalMilliseconds);
            RecordLoadTime("NodeRefresh", stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    public static void RecordLoadTime(string id, double value)
    {
        if (recordedValues.ContainsKey(id))
        {
            recordedValues[id].Add(value);
        }
        else
        {
            var list = new List<double>();
            list.Add(value);
            recordedValues.Add(id, list);
        }
    }
}

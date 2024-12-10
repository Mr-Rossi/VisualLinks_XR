using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.IO;
using System.Threading.Tasks;
using System;
using Valve.VR;
using UnityEngine.SceneManagement;

public class PathfindingTestScript : MonoBehaviour
{
    public float nodesDistance = 0.5f;
    public GameObject safetyBoundsParent;

    public SteamVR_Action_Boolean plantAction;

    public event Action OnUpdated;

    public float minX, maxX, minZ, maxZ;
    public float wallDistance = 0.51f;

    private Dictionary<string, List<uint>> allPenalties;
    private string currentPenalties;

    private Transform mainCameraTransform;

    public enum LinkMode
    {
        Straight,
        Ceiling,
        Floor,
        Wall,
        //CeilingCurved,
        //FloorCurved,
        StraightAvoid
    }

    [SerializeField]
    private LinkMode _mode;

    public LinkMode mode
    {
        get { return _mode; }
        set
        {
            _mode = value;
            LoadPointFromCache(mode, true);
            OnUpdated?.Invoke();
        }
    }


    private void OnEnable()
    {
        plantAction.AddOnChangeListener(OnPlantActionChange, SteamVR_Input_Sources.Any);
    }
    private void OnDisable()
    {
        plantAction.RemoveOnChangeListener(OnPlantActionChange, SteamVR_Input_Sources.Any);
    }

    private void OnPlantActionChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if (newState == false)
        {
            NextMode();
        }
    }

    public void NextMode()
    {
        var numberOfOptions = System.Enum.GetValues(typeof(LinkMode)).Length;
        if ((int)(mode + 1) == numberOfOptions) mode = 0;
        else mode++;
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCameraTransform = Camera.main.transform;
        LoadAllPoints();
    }

    public void WriteAllPointsToFile()
    {
        int nodesMade = 0;
        safetyBoundsParent.SetActive(true);
        Collider[] safetyBoundsList = safetyBoundsParent.GetComponentsInChildren<Collider>();
        List<Bounds> boundsList = new List<Bounds>();
        foreach (var child in safetyBoundsList)
        {
            boundsList.Add(child.bounds);
        }
        safetyBoundsParent.SetActive(false);
        foreach (var child in boundsList)
        {
            for (float x = child.min.x; x < child.max.x; x += nodesDistance)
            {
                for (float y = child.min.y; y < child.max.y; y += nodesDistance)
                {
                    for (float z = child.min.z; z < child.max.z; z += nodesDistance)
                    {
                        WritePointToFile(new Vector3(x, y, z));
                        nodesMade++;
                    }
                }
            }
        }
        Debug.Log("Created " + nodesMade + " files");
    }

    public void WritePointToFile(Vector3 point)
    {
        point = GetNearestHalfVector(point);
        string pointpath = point.ToString("F1");
        point = StringToVector3(pointpath);
        string path = "NodeData/" + SceneManager.GetActiveScene().name + "/" + pointpath + ".txt";

        Debug.Log("Writing " + path);

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);

        int layerMask = 1 << 8;

        for (int i = 0; i < AstarPath.active.data.pointGraph.nodeCount; i++)
        {
            var node = AstarPath.active.data.pointGraph.nodes[i];
            //AstarPath.active.AddWorkItem(new AstarWorkItem(() =>
            //{
            Vector3 dir = ((Vector3)node.position) - point;
            if (!Physics.Raycast(point, dir, dir.magnitude))//&& dot > 0.25f)
            {
                if (Physics.Raycast(point, dir, Mathf.Infinity, layerMask))
                {
                    writer.Write(1);
                    //node.Penalty = 10000U;
                    continue;
                }
                else
                    writer.Write(0);
                //node.Penalty = 0U;
            }
            else
                writer.Write(1);
            //node.Penalty = 10000U;

            //}));
        }
        writer.Close();
    }

    public void WriteCurrentPointToFile()
    {
        if (mainCameraTransform)
            WritePointToFile(mainCameraTransform.position);
        else
            WritePointToFile(Vector3.zero);
    }

    public void LoadPointFromFile()
    {

        string pointpath = GetNearestHalfVector(mainCameraTransform.position).ToString("F1");
        //string path = "Resources/NodeData/" + pointpath + ".txt";

        var textfile = Resources.Load<TextAsset>("/NodeData/" + SceneManager.GetActiveScene().name + "/"+ pointpath + ".txt");

        //Debug.Log("Reading " + path);

        //string path = "Assets/Resources/test.txt";

        //Write some text to the test.txt file
        //StreamReader reader = new StreamReader(path);
        List<uint> penalties = new List<uint>();

        foreach(char c in textfile.text)
        {
            penalties.Add((uint)(c - '0'));
        }
        /*while (reader.EndOfStream == false)
        {
            penalties.Add((uint)((char)reader.Read() - '0'));
        }
        reader.Close();*/

        for (int i = 0; i < AstarPath.active.data.pointGraph.nodeCount; i++)
        {
            var node = AstarPath.active.data.pointGraph.nodes[i];
            uint penalty = penalties[i];
            AstarPath.active.AddWorkItem(new AstarWorkItem(() =>
            {
                node.Penalty = penalty * 10000U;

            }));
        }

    }



    // returns false if correct penalties were already loaded
    public bool LoadPointFromCache(LinkMode mode, bool forceUpdate = false)
    {
        if (!mainCameraTransform)
            mainCameraTransform = Camera.main.transform;
        string pointpath = GetNearestHalfVector(mainCameraTransform.position).ToString("F1");
        if (!forceUpdate && pointpath == currentPenalties)
            return false;
        string path = pointpath;

        Debug.Log("Reading " + path);
        if (!allPenalties.ContainsKey(path))
            return false;
        MenuHelper.StartStopwatch();

        List<uint> penalties = allPenalties[path];
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();


        switch (mode)
        {
            case LinkMode.StraightAvoid:
                Parallel.For(0, AstarPath.active.data.pointGraph.nodeCount, index =>
                {
                    AstarPath.active.data.pointGraph.nodes[index].Penalty = penalties[index];
                });
                break;
            case LinkMode.Ceiling:
                Parallel.For(0, AstarPath.active.data.pointGraph.nodeCount, index =>
                {
                    AstarPath.active.data.pointGraph.nodes[index].Penalty = penalties[index] + (3000U - (uint)AstarPath.active.data.pointGraph.nodes[index].position.y);
                });
                break;
            case LinkMode.Floor:
                Parallel.For(0, AstarPath.active.data.pointGraph.nodeCount, index =>
                {
                    AstarPath.active.data.pointGraph.nodes[index].Penalty = penalties[index] + 2*(uint)AstarPath.active.data.pointGraph.nodes[index].position.y;
                });
                break;
            case LinkMode.Wall:
                Parallel.For(0, AstarPath.active.data.pointGraph.nodeCount, index =>
                {
                    var xPos = AstarPath.active.data.pointGraph.nodes[index].position.x / 1000f;
                    var zPos = AstarPath.active.data.pointGraph.nodes[index].position.z / 1000f;
                    if (minX + wallDistance > xPos || maxX - wallDistance < xPos || minZ + wallDistance > zPos || maxZ - wallDistance < zPos)
                        AstarPath.active.data.pointGraph.nodes[index].Penalty = penalties[index];
                    else
                        AstarPath.active.data.pointGraph.nodes[index].Penalty = 10000U;
                });
                break;
        }



        stopwatch.Stop();
        if(MenuHelper.testRun)
            MenuHelper.RecordLoadTime("load", stopwatch.Elapsed.TotalMilliseconds);
        currentPenalties = pointpath;
        return true;
    }

    public void LoadAllPoints()
    {
        allPenalties = new Dictionary<string, List<uint>>();

        string path = "Nodedata/" + SceneManager.GetActiveScene().name;

        Debug.Log("Reading all files in" + path);


        TextAsset[] files = Resources.LoadAll<TextAsset>(path);
        foreach (TextAsset file in files)
        {
            List<uint> penalties = new List<uint>();
            foreach (char c in file.text)
            {
                penalties.Add((uint)(c - '0') * 10000U);
            }

            allPenalties.Add(file.name, penalties);
        }

        //string path = "Assets/Resources/test.txt";

        /*DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] info = dir.GetFiles("*.*");

        foreach (FileInfo f in info)
        {
            //Write some text to the test.txt file
            StreamReader reader = new StreamReader(f.FullName);
            List<uint> penalties = new List<uint>();
            while (reader.EndOfStream == false)
            {
                penalties.Add((uint)((char)reader.Read() - '0') * 10000U);
            }
            reader.Close();

            allPenalties.Add(f.Name, penalties);
        }*/

    }

    private float timeSinceLastUpdate = 0.0f;

    // Update is called once per frame
    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= 0.1f && LoadPointFromCache(mode))
        {
            OnUpdated?.Invoke();
            timeSinceLastUpdate = 0f;
        }
    }

    public Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public Vector3 GetNearestHalfVector(Vector3 vector)
    {
        vector *= 2;
        string temp = vector.ToString("F0");
        return StringToVector3(temp) / 2f;
    }
}

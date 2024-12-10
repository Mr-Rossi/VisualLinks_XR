using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PathfindingTestScript))]
public class PathfinderTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PathfindingTestScript myScript = (PathfindingTestScript)target;
        if (GUILayout.Button("Write Penalties"))
        {
            myScript.WriteCurrentPointToFile();
        }
        if (GUILayout.Button("Load Penalties"))
        {
            myScript.LoadPointFromFile();
        }
        if (GUILayout.Button("WRITE ALL PENALTIES"))
        {
            myScript.WriteAllPointsToFile();
        }
    }
}
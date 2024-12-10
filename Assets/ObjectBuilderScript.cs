using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ObjectBuilderScript : MonoBehaviour
{
    public GameObject obj;
    public Vector3 spawnPoint;
    public Transform nodeParent;
    public float nodesDistance = 1.0f;
    public GameObject safetyBoundsParent;
    //public List<Collider> safetyBoundsList;
    //Bounds safetyBounds;
    List<Vector3> nodeList;

    public void CleanChildren()
    {

        var tempList = nodeParent.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            DestroyImmediate(child.gameObject);
        }
         
    }

    public void BuildObject()
    {
        BoundsOctree<Collider> boundsTree = new BoundsOctree<Collider>(20, transform.position, 0.1f, 1);
        Collider[] allObjects = UnityEngine.Object.FindObjectsOfType<Collider>();
        foreach (var go in allObjects)
        {
            boundsTree.Add(go, go.bounds);
        }

        CleanChildren();

        /*for (int x = -5; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int z = -10; z < 10; z++)
                {
                    Instantiate(obj, new Vector3(x,y,z), Quaternion.identity, nodeParent);
                }
            }
        }*/

        //safetyBounds = new Bounds(new Vector3(0f, 2.5f, 0f), new Vector3(10f, 5f, 20f)); //old bounds
        //safetyBounds = new Bounds(new Vector3(-4f, 1.5f, 1.25f), new Vector3(35f, 4f, 22f)); //new bounds

        nodeList = new List<Vector3>();
        //InstantiateNodes(boundsTree.rootNode);
        InstantiateNodesEquallyDistributed();
    }

    private bool SafetyBoundsContain(Vector3 pos)
    {
        Collider[] safetyBoundsList = safetyBoundsParent.GetComponentsInChildren<Collider>();
        foreach (var child in safetyBoundsList)
        {
            if (child.bounds.Contains(pos))
                return true;
        }
        return false;
    }

    private void InstantiateNodesEquallyDistributed()
    {
        int nodesMade = 0;
        safetyBoundsParent.SetActive(true);
        Collider[] safetyBoundsList = safetyBoundsParent.GetComponentsInChildren<Collider>();
        foreach(var child in safetyBoundsList)
        {
            for (float x = child.bounds.min.x; x < child.bounds.max.x; x+=nodesDistance)
            {
                for (float y = child.bounds.min.y; y < child.bounds.max.y; y += nodesDistance)
                {
                    for (float z = child.bounds.min.z; z < child.bounds.max.z; z += nodesDistance)
                    {
                        Instantiate(obj, new Vector3(x, y, z), Quaternion.identity, nodeParent);
                        nodesMade++;
                    }
                }
            }
        }
        safetyBoundsParent.SetActive(false);
        Debug.Log("Created " + nodesMade + " nodes");
    }

    private void InstantiateNodes(BoundsOctreeNode<Collider> node)
    {
        //Gizmos.DrawWireCube(thisBounds.center, thisBounds.size);

        Bounds thisBounds = new Bounds(node.Center, new Vector3(node.adjLength, node.adjLength, node.adjLength));
        for (float x = thisBounds.min.x; x < thisBounds.max.x; x+= Mathf.Clamp(0.33f*thisBounds.size.x, 0.1f, 1.0f))
        {
            for (float y = thisBounds.min.y; y < thisBounds.max.y; y+= Mathf.Clamp(0.33f * thisBounds.size.y, 0.1f, 1.0f))
            {
                for (float z = thisBounds.min.z; z < thisBounds.max.z; z += Mathf.Clamp(0.33f * thisBounds.size.z, 0.1f, 1.0f))
                {
                    Vector3 xyz = new Vector3(x, y, z);
                    if (!SafetyBoundsContain(xyz))
                        continue;
                    bool tooClose = false;
                    foreach(var vecnode in nodeList)
                    {
                        if ((xyz - vecnode).magnitude < 0.1f)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (!tooClose)
                    {
                        Instantiate(obj, xyz, Quaternion.identity, nodeParent);
                        nodeList.Add(xyz);
                    }
                }
            }
        }

        if (node.children != null)
        {
            for (int i = 0; i < 8; i++)
            {
                InstantiateNodes(node.children[i]);
            }
        }
    }
}
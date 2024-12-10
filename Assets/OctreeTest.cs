using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeTest : MonoBehaviour
{
    BoundsOctree<Collider> boundsTree;
    // Start is called before the first frame update
    void Start()
    {
        boundsTree = new BoundsOctree<Collider>(20, transform.position, 0.1f, 1);
        Collider[] allObjects = UnityEngine.Object.FindObjectsOfType<Collider>();
        foreach (var go in allObjects)
        {
            boundsTree.Add(go, go.bounds);
        }
    }

    void OnDrawGizmos()
    {
        //if(boundsTree != null)
        //    boundsTree.DrawAllBounds(); // Draw node boundaries
        //boundsTree.DrawAllObjects(); // Draw object boundaries
        //boundsTree.DrawCollisionChecks(); // Draw the last *numCollisionsToSave* collision check boundaries
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}

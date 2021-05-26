using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    private float lerpTime = 1f;
    private float currentLerpTime;


    private Hierarchical_Pathfinding hpa;

    private void Awake ( )
    {
        hpa = gameObject.GetComponent<Hierarchical_Pathfinding>();
    }
    private void Start ( )
    {
        hpa.OnTreeBuilt += TreeRecieved;
    }
    private void TreeRecieved (object sender, TreeData<List<Cluster>> data)
    {
        TreeData<List<Cluster>> showDataBro = data;

        ScanTree( data );
    }
    private void StartLerp ( )
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > lerpTime)
        {
            currentLerpTime = lerpTime;
        }

        float perc = currentLerpTime / lerpTime;
        // gameObject.transform.position = Vector3.Lerp();
    }
    private void ScanTree (TreeData<List<Cluster>> tree)
    {

        foreach (var children in tree.Children)
        {
            TreeData<List<Cluster>> subTree = children.Value;
            Debug.Log( $"{children.Key[0, 0]}-{children.Key[1, 0]} / {children.Value.Data.Count}" );
            //gotta make a pathfinding on the cluster grid from the initial point to the next
            //this part is going to be a headache
            ScanTree( subTree );
        }

    }
}

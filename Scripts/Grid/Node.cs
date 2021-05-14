using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int level;
    public Vector3 worldPosition;
    public Cell cell;
    public Node pair;
    public Entrance entrance;
    public Cluster cluster;

    public Node ( )
    {

    }
    public Node (Cluster cluster)
    {
        this.cluster = cluster;
    }

    public void SetEntrance (Entrance entrance)
    {
        this.entrance = entrance;
    }
    public void SetPositions (Vector3 worldPosition)
    {
        this.worldPosition = worldPosition;
    }
    public void SetCell (Cell cell)
    {
        this.cell = cell;
    }
}

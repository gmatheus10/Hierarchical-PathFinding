using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Cluster cluster;
    public Entrance entrance;
    public Cell cell;
    public Node pair;
    public Vector3Int gridPosition;
    public Vector3 worldPosition;
    public int level;

    public List<List<Cell>> paths = new List<List<Cell>>();
    public Node (Cluster cluster, Entrance entrance)
    {
        this.cluster = cluster;
        this.entrance = entrance;
    }
    public void SetPositions (Vector3 worldPosition, Vector3Int gridPosition)
    {
        this.worldPosition = worldPosition;
        this.gridPosition = gridPosition;
    }
    public void SetCell (Cell cell)
    {
        this.cell = cell;
    }
    public void AddPath (List<Cell> path)
    {
        paths.Add( path );
    }


    //need to check if the cluster of End position is the same as the previous 

}

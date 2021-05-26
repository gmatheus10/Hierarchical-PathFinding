using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    public Vector2Int size;
    public Vector3 originPosition;
    public Vector3Int gridPosition;

    public float gCost;
    public float hCost;
    public float FCost;
    public Cluster cameFrom;

    public Dictionary<string, List<Cell>> borders = new Dictionary<string, List<Cell>>();
    public List<Entrance> entrances = new List<Entrance>();
    public List<Node> clusterNodes = new List<Node>();
    public int level;

    public List<Cluster> lesserLevelClusters = new List<Cluster>();
    public Grid<Cell> clusterGrid;
    public Cluster (Vector2Int size, Vector3 originPosition, int level = 1)
    {
        this.originPosition = originPosition;
        this.level = level;
        this.size = size;
    }
    public void SetGridPosition (Vector3Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }
    public void AddEntrance (Entrance entrance)
    {
        entrances.Add( entrance );
    }
    public void AddNodeToCluster (Node node)
    {
        if (this.IsPositionInside( node.worldPosition ))
        {
            if (!clusterNodes.Contains( node ))
            {

                clusterNodes.Add( node );
            }
        }

    }
    public bool IsPositionInside (Vector3 position)
    {
        Vector3 thisStart = this.originPosition;
        Vector3 thisEnd = thisStart + new Vector3( this.size.x, this.size.y, 0 );


        bool xLargerStart = position.x >= thisStart.x;
        bool yLargerStart = position.y >= thisStart.y;

        bool xSmallerEnd = position.x <= thisEnd.x;
        bool ySmallerEnd = position.y <= thisEnd.y;

        return xLargerStart && xSmallerEnd && yLargerStart && ySmallerEnd;
    }
    private bool IsClusterInside (Cluster cluster)
    {
        bool isStartinside = this.IsPositionInside( cluster.originPosition );
        bool isEndinside = this.IsPositionInside( cluster.originPosition + new Vector3( cluster.size.x, cluster.size.y, 0 ) );

        return isStartinside && isEndinside;
    }
    public bool IsEntranceInside (List<Cell> entrance)
    {
        int count = 0;
        foreach (Cell cell in entrance)
        {
            if (IsPositionInside( cell.worldPosition ))
            {
                count++;
            }
        }
        if (count == entrance.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void AddLesserClusters (Cluster[,] lesserClustersArray)
    {
        //need a list / array of the lesserClusters 
        //detect clusters inside
        //IsPositionInside for cluster origin position and for cluster end position
        for (int i = 0; i < lesserClustersArray.GetLength( 0 ); i++)
        {
            for (int j = 0; j < lesserClustersArray.GetLength( 1 ); j++)
            {
                Cluster cluster = lesserClustersArray[i, j];
                if (IsClusterInside( cluster ))
                {
                    lesserLevelClusters.Add( cluster );

                }
            }
        }

        //sort them on the array
    }
    public void AddGrid (Grid<Cell> clusterGrid)
    {
        this.clusterGrid = clusterGrid;
    }
    public bool IsNodeInside (Node node)
    {
        return clusterGrid.IsInsideGrid( node.worldPosition );
    }
    public void SetFCost ( )
    {
        this.FCost = this.gCost + this.hCost;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    public Vector2Int size;
    public Vector3 OriginPosition { get; private set; }
    public Vector3Int GridPosition { get; private set; }

    public float gCost;
    public float hCost;
    public float FCost { get; private set; }
    public Cluster cameFrom;

    public Dictionary<string, List<Cell>> borders = new Dictionary<string, List<Cell>>();
    public Dictionary<string, Entrance> entrances = new Dictionary<string, Entrance>();
    public List<Node> clusterNodes = new List<Node>();
    public int level;

    public List<Cluster> lesserLevelClusters = new List<Cluster>();
    public Grid<Cell> clusterGrid { get; private set; }
    public Cluster (Vector2Int size, Vector3 originPosition, int level = 1)
    {
        this.OriginPosition = originPosition;
        this.level = level;
        this.size = size;
    }
    public void SetGridPosition (Vector3Int gridPosition)
    {
        this.GridPosition = gridPosition;
    }
    public void AddEntrance (Entrance entrance)
    {
        string key1 = $"{entrance.Cluster1.OriginPosition}->{entrance.Cluster2.OriginPosition}";
        entrances.Add( key1, entrance );
    }
    public void AddNodeToCluster (Node node)
    {
        if (this.IsPositionInside( node.WorldPosition ))
        {
            if (!clusterNodes.Contains( node ))
            {

                clusterNodes.Add( node );
            }
        }

    }
    public bool IsPositionInside (Vector3 position)
    {
        Vector3 thisStart = this.OriginPosition;
        Vector3 thisEnd = thisStart + new Vector3( this.size.x, this.size.y, 0 );


        bool xLargerStart = position.x >= thisStart.x;
        bool yLargerStart = position.y >= thisStart.y;

        bool xSmallerEnd = position.x <= thisEnd.x;
        bool ySmallerEnd = position.y <= thisEnd.y;

        return xLargerStart && xSmallerEnd && yLargerStart && ySmallerEnd;
    }
    private bool IsClusterInside (Cluster cluster)
    {
        bool isStartinside = this.IsPositionInside( cluster.OriginPosition );
        bool isEndinside = this.IsPositionInside( cluster.OriginPosition + new Vector3( cluster.size.x, cluster.size.y, 0 ) );

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
    public void SetGrid (Grid<Cell> clusterGrid)
    {
        this.clusterGrid = clusterGrid;
    }

    public void SetFCost ( )
    {
        this.FCost = this.gCost + this.hCost;
    }


}

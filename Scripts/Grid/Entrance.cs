using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entrance
{
    public Cluster cluster1;
    public Cluster cluster2;

    public Vector3 originPosition;
    public Vector3 endPosition;

    public List<Cell> entranceTiles = new List<Cell>();
    public List<Cell> symmEntranceTiles = new List<Cell>();

    public List<Node> entranceNodes = new List<Node>();

    public bool isBlocked = false;
    public enum Orientation
    {
        North, East, South, West
    }
    public Orientation C1Orientation, C2Orientation;
    public Entrance ( )
    {

    }
    public void SetClusters (Cluster cluster1, Cluster cluster2)
    {
        this.cluster1 = cluster1;
        this.cluster2 = cluster2;
    }
    public void FillEntrance (List<Cell> entranceTiles)
    {
        this.entranceTiles = entranceTiles;
        this.originPosition = entranceTiles[0].worldPosition;
    }
    public void FillSymmEntrance (List<Cell> symmTiles)
    {
        symmEntranceTiles = symmTiles;
        this.endPosition = symmTiles[symmTiles.Count - 1].worldPosition;
    }
    public Cluster[] GetClusters (int level = 1)
    {
        Cluster[] clusters = new Cluster[2];
        clusters[0] = cluster1;
        clusters[1] = cluster2;
        return clusters;
    }
    public List<Cell> GetEntrance ( )
    {
        return entranceTiles;
    }
    public List<Cell> GetSymm ( )
    {
        return symmEntranceTiles;
    }
    public void AddNode (Node node)
    {
        this.entranceNodes.Add( node );
    }
    public void SetOrientation (Orientation C1Orientation, Orientation C2Orientation)
    {
        this.C1Orientation = C1Orientation;
        this.C2Orientation = C2Orientation;
    }
    public Cell GetSymmetricalCell (Cell Reference)
    {
        //    get the index i from the reference
        //return the symmList[i]
        int index = entranceTiles.IndexOf( Reference );

        try
        {
            Cell symmCell = symmEntranceTiles[index];
            return symmCell;
        }
        catch (System.Exception)
        {

            Debug.Log( "Reference: " + Reference.worldPosition );
            return null;
        }
    }
    public void LevelUpEntrance (Cluster cluster1, Cluster cluster2, int level)
    {

        bool entranceTilesInside = cluster1.IsEntranceInside( this.entranceTiles );
        bool symmTilesInside = cluster2.IsEntranceInside( this.symmEntranceTiles );
        if (entranceTilesInside && symmTilesInside)
        {

            this.cluster1 = cluster1;
            cluster1.AddEntrance( this );
            this.cluster2 = cluster2;
            cluster2.AddEntrance( this );
        }

    }
    public bool IsNodeInside (Node node)
    {
        return entranceNodes.Contains( node );
    }
}
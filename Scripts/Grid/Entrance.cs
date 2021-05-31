using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Entrance
{
    public Cluster Cluster1 { get; private set; }
    public Cluster Cluster2;

    public Vector3 originPosition;
    public Vector3 endPosition;

    public List<Cell> entranceTiles = new List<Cell>();
    public List<Cell> symmEntranceTiles = new List<Cell>();

    public List<Node> entranceNodes = new List<Node>();

    public bool isBlocked = false;
    public Entrance ( )
    {

    }

    public void SetClusters (Cluster Cluster1, Cluster Cluster2)
    {
        this.Cluster1 = Cluster1;
        this.Cluster2 = Cluster2;
    }
    public void SetClusters (Cluster[] pair)
    {
        if (pair.Length > 2)
        {
            throw new System.Exception( "Cannot have more than 2 elements in argument array" );
        }
        else
        {
            this.Cluster1 = pair[0];
            this.Cluster2 = pair[1];
        }
    }
    public void FillEntrance (List<Cell> entranceTiles)
    {
        this.entranceTiles = entranceTiles;
        this.originPosition = entranceTiles[0].worldPosition;
        this.endPosition = entranceTiles[entranceTiles.Count - 1].worldPosition;
    }
    public void FillSymmEntrance (List<Cell> symmTiles)
    {
        symmEntranceTiles = symmTiles;

    }
    public Cluster[] GetClusters ( )
    {
        Cluster[] clusters = new Cluster[2];
        clusters[0] = Cluster1;
        clusters[1] = Cluster2;
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
    public Cell GetSymmetricalCell (Cell Reference)
    {
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
    public bool HaveEntrance (Cluster cluster1, Cluster cluster2)
    {

        bool entranceTilesInside = cluster1.IsEntranceInside( this.entranceTiles );
        bool symmTilesInside = cluster2.IsEntranceInside( this.symmEntranceTiles );
        if (entranceTilesInside && symmTilesInside)
        {
            return true;
        }
        return false;
    }
    public bool IsNodeInside (Node node)
    {
        return entranceNodes.Contains( node );
    }


    public Entrance MergeEntrances (params Entrance[] entrances)
    {
        Entrance merged = new Entrance();

        merged.SetClusters( entrances[0].GetClusters() );
        merged.originPosition = entrances[0].originPosition;
        merged.endPosition = entrances[entrances.Length - 1].endPosition;

        List<Cell> mergedTiles = new List<Cell>();
        List<Cell> mergedSymmTiles = new List<Cell>();

        List<Node> mergedNodes = new List<Node>();

        foreach (Entrance e in entrances)
        {
            mergedTiles = mergedTiles.Concat( e.entranceTiles ).ToList();
            mergedSymmTiles = mergedSymmTiles.Concat( e.symmEntranceTiles ).ToList();
            mergedNodes = mergedNodes.Concat( e.entranceNodes ).ToList();
        }
        merged.entranceTiles = mergedTiles;
        merged.symmEntranceTiles = mergedSymmTiles;
        merged.entranceNodes = mergedNodes;
        if (mergedNodes.Count == 0)
        {
            merged.isBlocked = true;
        }
        return merged;
    }
}
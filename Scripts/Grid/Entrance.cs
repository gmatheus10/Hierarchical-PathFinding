using System.Collections;
using System.Linq;
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
  public Entrance()
  {

  }

  public void SetClusters(Cluster cluster1, Cluster cluster2)
  {
    this.cluster1 = cluster1;
    this.cluster2 = cluster2;
  }
  public void SetClusters(Cluster[] pair)
  {
    if (pair.Length > 2)
    {
      throw new System.Exception("Cannot have more than 2 elements in argument array");
    }
    else
    {
      this.cluster1 = pair[0];
      this.cluster2 = pair[1];
    }
  }
  public void FillEntrance(List<Cell> entranceTiles)
  {
    this.entranceTiles = entranceTiles;
    this.originPosition = entranceTiles[0].worldPosition;
  }
  public void FillSymmEntrance(List<Cell> symmTiles)
  {
    symmEntranceTiles = symmTiles;
    this.endPosition = symmTiles[symmTiles.Count - 1].worldPosition;
  }
  public Cluster[] GetClusters(int level = 1)
  {
    Cluster[] clusters = new Cluster[2];
    clusters[0] = cluster1;
    clusters[1] = cluster2;
    return clusters;
  }
  public List<Cell> GetEntrance()
  {
    return entranceTiles;
  }
  public List<Cell> GetSymm()
  {
    return symmEntranceTiles;
  }
  public void AddNode(Node node)
  {
    this.entranceNodes.Add(node);
  }
  public Cell GetSymmetricalCell(Cell Reference)
  {
    //    get the index i from the reference
    //return the symmList[i]
    int index = entranceTiles.IndexOf(Reference);

    try
    {
      Cell symmCell = symmEntranceTiles[index];
      return symmCell;
    }
    catch (System.Exception)
    {

      Debug.Log("Reference: " + Reference.worldPosition);
      return null;
    }
  }
  public bool HaveEntrance(Cluster cluster1, Cluster cluster2)
  {

    bool entranceTilesInside = cluster1.IsEntranceInside(this.entranceTiles);
    bool symmTilesInside = cluster2.IsEntranceInside(this.symmEntranceTiles);
    if (entranceTilesInside && symmTilesInside)
    {
      return true;
    }
    return false;
  }
  public bool IsNodeInside(Node node)
  {
    return entranceNodes.Contains(node);
  }


  public Entrance MergeEntrances(params Entrance[] entrances)
  {
    Entrance merged = new Entrance();

    merged.SetClusters(entrances[0].GetClusters());
    merged.originPosition = entrances[0].originPosition;
    merged.endPosition = entrances[entrances.Length - 1].endPosition;

    List<Cell> mergedTiles = new List<Cell>();
    List<Cell> mergedSymmTiles = new List<Cell>();

    List<Node> mergedNodes = new List<Node>();

    foreach (Entrance e in entrances)
    {
      mergedTiles = mergedTiles.Concat(e.entranceTiles).ToList();
      mergedSymmTiles = mergedSymmTiles.Concat(e.symmEntranceTiles).ToList();
      mergedNodes = mergedNodes.Concat(e.entranceNodes).ToList();
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
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Hierarchical_Pathfinding : MonoBehaviour
{
  //Recieves the Node from PlayerController and converts into a path list
  // Give the path list to the Movement.cs
  private int MaxLevel;
  private AbstractGraph abstractGraph;
  //Events publishers scripts
  private PlayerController playerController;
  private void Awake()
  {
    abstractGraph = GameObject.FindGameObjectWithTag("Grid").GetComponent<AbstractGraph>();
    playerController = gameObject.GetComponent<PlayerController>();
  }
  private void Start()
  {
    playerController.OnPlayerDestinationSet += PlayerController_DestinationRecieved;
    MaxLevel = abstractGraph.Level;
  }
  public delegate void SendPathTree(object sender, TreeData<List<Cluster>> tree);
  public event SendPathTree OnTreeBuilt;
  private void PlayerController_DestinationRecieved(object sender, PlayerController.PlayerPositions pos)
  {
    List<Cluster> MaxLevelPath = HierarchicalSearch(pos.startNode, pos.endNode, abstractGraph.Level);
    HPA_Utils.ShowPathClusters(MaxLevelPath, Color.yellow);
    HPA_Utils.ShowPathLines(MaxLevelPath, Color.red);
  }

  private List<Cluster> HierarchicalSearch(Node start, Node end, int level)
  {
    InsertNode(start, level);
    InsertNode(end, level);
    Cluster startCluster = GetCluster(start.worldPosition, level);
    Cluster endCluster = GetCluster(end.worldPosition, level);
    List<Cluster> abstractPath = SearchForPath(startCluster, endCluster);

    return abstractPath;
  }
  private void InsertNode(Node node, int maxLevel)
  {
    for (int i = 1; i <= maxLevel; i++)
    {
      Cluster c = GetCluster(node.worldPosition, maxLevel);
      ConnectToBorder(node, c);
    }
    node.level = maxLevel;


    void ConnectToBorder(Node node, Cluster cluster)
    {
      int level = cluster.level;
      foreach (Node n in cluster.clusterNodes)
      {
        if (node.level < level)
        {
          continue;
        }
        //this distance will be used later!!
        float distance = (node.worldPosition - n.worldPosition).magnitude;
      }

    }
  }



  //Helpers
  private List<Cluster> SearchForPath(Cluster startCluster, Cluster endCluster)
  {
    int Level = startCluster.level;
    SortedList<float, Cluster> openList = new SortedList<float, Cluster>();
    List<Cluster> closedList = new List<Cluster>();
    ScanGridAndSetDefault(Level);


    startCluster.gCost = 0;
    startCluster.hCost = Utils.ManhatamDistance(startCluster, endCluster);

    startCluster.SetFCost();

    openList.Add(startCluster.FCost, startCluster);
    while (openList.Count > 0)
    {
      Cluster currentCluster = openList.Values[0];
      if (currentCluster == endCluster)
      {
        return CalculatePath(currentCluster);
      }
      openList.RemoveAt(0);
      closedList.Add(currentCluster);
      List<Cluster> neighbourList = GetNeighboursList(currentCluster, Level);

      foreach (Cluster neighbour in neighbourList)
      {
        float newG = currentCluster.gCost + Utils.ManhatamDistance(neighbour, currentCluster);
        if (neighbour == endCluster)
        {
          neighbour.cameFrom = currentCluster;
          return CalculatePath(neighbour);
        }

        if (newG < neighbour.gCost)
        {
          neighbour.gCost = newG;
          neighbour.hCost = Utils.ManhatamDistance(neighbour, endCluster);
          neighbour.SetFCost();

          if (!openList.Values.Contains(neighbour))
          {
            neighbour.cameFrom = currentCluster;
            string key = $"{neighbour.originPosition}->{currentCluster.originPosition}";

            if (isEntranceBlocked(neighbour, key))
            {
              continue;
            }

            try
            {
              openList.Add(neighbour.FCost, neighbour);
            }
            catch (System.Exception)
            {
              if (!closedList.Contains(neighbour))
              {
                if (openList.ContainsKey(neighbour.FCost))
                {
                  Cluster insideOpenList = openList[neighbour.FCost];

                  if (isInsiderIsCloser(insideOpenList, neighbour))
                  {
                    continue;
                  }
                  else
                  {
                    ReplaceInsiderWith(neighbour);
                  }
                }
              }
            }
          }
        }
      }
    }
    Debug.Log($"Didn't find it - start cluster: {startCluster.originPosition}");
    HPA_Utils.DrawClusterBorders(startCluster);
    HPA_Utils.DrawClusterBorders(endCluster);
    return null;

    bool isEntranceBlocked(Cluster neighbour, string key)
    {
      if (neighbour.entrances.ContainsKey(key))
      {
        if (neighbour.entrances[key].isBlocked)
        {
          return true;
        }
      }
      return false;
    }
    bool isInsiderIsCloser(Cluster insider, Cluster neighbour)
    {
      if (insider.gCost <= neighbour.gCost)
      {
        return true;
      }
      return false;
    }
    void ReplaceInsiderWith(Cluster neighbour)
    {
      openList.Remove(neighbour.FCost);
      openList.Add(neighbour.FCost, neighbour);
    }
  }
  private List<Cluster> CalculatePath(Cluster endCluster)
  {
    List<Cluster> path = new List<Cluster>() { endCluster };

    Cluster queue = endCluster;
    while (queue.cameFrom != null)
    {
      path.Add(queue.cameFrom);
      queue = queue.cameFrom;
    }
    path.Reverse();
    return path;
  }
  private Cluster GetCluster(Vector3 position, int Level)
  {
    List<Cluster[,]> allClusters = abstractGraph.allClustersAllLevels;
    foreach (Cluster[,] setOfClusters in allClusters)
    {
      for (int i = 0; i < setOfClusters.GetLength(0); i++)
      {
        for (int j = 0; j < setOfClusters.GetLength(1); j++)
        {
          Cluster current = setOfClusters[i, j];
          if (current.IsPositionInside(position))
          {
            if (current.level == Level)
            {
              return current;
            }
          }
        }
      }
    }
    return null;
  }
  private List<Cluster> GetNeighboursList(Cluster current, int Level, Cluster parent = null)
  {
    Vector3 center = current.originPosition + new Vector3(0.5f * current.size.x, 0.5f * current.size.y, 0);
    List<Cluster> neighbours = new List<Cluster>();

    for (int i = -1; i <= 1; i++)
    {
      for (int j = -1; j <= 1; j++)
      {
        if ((i == 0 && j == 0) || (i == 1 && j == 1) || (i == -1 && j == -1) || (i == -1 && j == 1) || (i == 1 && j == -1))
        {
          continue;
        }
        Vector3 neighbourPosition = center + new Vector3(i * current.size.x, j * current.size.y);
        if (parent != null)
        {
          if (!parent.IsPositionInside(neighbourPosition))
          {
            continue;
          }
        }

        Cluster neighbour = GetCluster(neighbourPosition, Level);
        if (neighbour != null)
        {
          neighbours.Add(neighbour);
        }

      }
    }
    return neighbours;

  }
  private void ScanGridAndSetDefault(int Level)
  {
    List<Cluster[,]> allClusters = abstractGraph.allClustersAllLevels;

    foreach (Cluster[,] setOfClusters in allClusters)
    {
      for (int i = 0; i < setOfClusters.GetLength(0); i++)
      {
        for (int j = 0; j < setOfClusters.GetLength(1); j++)
        {
          Cluster current = setOfClusters[i, j];
          if (current.level == Level)
          {
            current.gCost = Mathf.Infinity;
            current.SetFCost();
            current.cameFrom = null;
          }
        }
      }
    }
  }

}

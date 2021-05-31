using UnityEngine;
using System;
using System.Linq;
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
    private void Awake ( )
    {
        abstractGraph = GameObject.FindGameObjectWithTag( "Grid" ).GetComponent<AbstractGraph>();
        playerController = gameObject.GetComponent<PlayerController>();
    }
    private void Start ( )
    {
        playerController.OnPlayerDestinationSet += PlayerController_DestinationRecieved;
        MaxLevel = abstractGraph.Level;
    }
    private void PlayerController_DestinationRecieved (object sender, PlayerController.PlayerPositions pos)
    {
        List<Node> MaxLevelPath = HierarchicalSearch( pos.startNode, pos.endNode, abstractGraph.Level );
        HPA_Utils.ShowPathLines( MaxLevelPath, Color.red );
    }


    private List<Node> HierarchicalSearch (Node start, Node end, int level)
    {
        InsertNode( start, level );
        InsertNode( end, level );
        List<Node> abstractPath = SearchForPath( start, end );
        //  RefinePath( abstractPath, level );
        return abstractPath;

        void InsertNode (Node node, int maxLevel)
        {
            node.level = maxLevel;
            for (int i = 1; i <= maxLevel; i++)
            {
                Cluster c = GetCluster( node.WorldPosition, maxLevel );
                node.cluster = c;
                ConnectToBorder( node, c );
            }



            void ConnectToBorder (Node node, Cluster cluster)
            {
                int level = cluster.level;
                foreach (Node n in cluster.clusterNodes)
                {
                    if (n.level < level)
                    {
                        continue;
                    }
                    int distance = Utils.ManhatamDistance( node.GridPosition, n.GridPosition );
                    node.AddNeighbour( n, distance );
                    n.AddNeighbour( node, distance );

                    n.neighbours = n.neighbours.OrderBy( e => e.Key ).ToList();

                }
                node.neighbours = node.neighbours.OrderBy( e => e.Key ).ToList();
                cluster.AddNodeToCluster( node );
            }
        }
    }
    private void RefinePath (List<Cluster> AbstractPath, int level)
    {
        //need to do this.
        for (int i = level - 1; i >= 1; i--)
        {

        }
        //void Tree (List<Cluster> path, int level)
        //{
        //    if (level != 1)
        //    {
        //        foreach (Cluster c in path)
        //        {

        //        }
        //    }
        //}
    }
    //Helpers
    private List<Node> SearchForPath (Node startNode, Node endNode)
    {
        int Level = startNode.level;
        SortedList<float, Node> openList = new SortedList<float, Node>();
        List<Node> closedList = new List<Node>();
        ScanGridAndSetDefault( Level );


        startNode.gCost = 0;
        startNode.hCost = Utils.NodeDistance( startNode, endNode );

        startNode.SetFCost();

        openList.Add( startNode.FCost, startNode );
        while (openList.Count > 0)
        {
            Node currentNode = openList.Values[0];
            if (currentNode == endNode)
            {
                return CalculatePath( currentNode );
            }
            openList.RemoveAt( 0 );
            closedList.Add( currentNode );

            HPA_Utils.DrawCrossInPosition( currentNode.WorldPosition, Color.red );

            foreach (var pair in currentNode.neighbours)
            {
                Node neighbour = pair.Value;
                float newG = currentNode.gCost + Utils.NodeDistance( neighbour, currentNode );
                if (neighbour == endNode)
                {
                    neighbour.SetParent( currentNode );
                    return CalculatePath( endNode );
                }

                if (newG < neighbour.gCost)
                {
                    neighbour.gCost = newG;
                    neighbour.hCost = Utils.NodeDistance( neighbour, endNode );
                    neighbour.SetFCost();

                    if (!openList.Values.Contains( neighbour ))
                    {
                        neighbour.SetParent( currentNode );
                        try
                        {
                            openList.Add( neighbour.FCost, neighbour );
                            HPA_Utils.DrawCrossInPosition( neighbour.WorldPosition, Color.blue );
                        }
                        catch (System.Exception)
                        {
                            if (!closedList.Contains( neighbour ))
                            {
                                if (openList.ContainsKey( neighbour.FCost ))
                                {
                                    Node insideOpenList = openList[neighbour.FCost];

                                    if (isInsiderCloser( insideOpenList, neighbour ))
                                    {
                                        HPA_Utils.DrawCrossInPosition( neighbour.WorldPosition, Color.black );
                                        continue;
                                    }
                                    else
                                    {
                                        ReplaceInsiderWith( neighbour );
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        Debug.Log( $"Didn't find it - start cluster: {startNode.WorldPosition}" );

        return null;

        bool isInsiderCloser (Node insider, Node neighbour)
        {
            if (insider.gCost <= neighbour.gCost)
            {
                return true;
            }
            return false;
        }
        void ReplaceInsiderWith (Node neighbour)
        {
            openList.Remove( neighbour.FCost );
            openList.Add( neighbour.FCost, neighbour );
        }
    }
    private List<Node> CalculatePath (Node endNode)
    {
        List<Node> path = new List<Node>() { endNode };

        Node queue = endNode;
        while (queue.Parent != null)
        {
            path.Add( queue.Parent );
            queue = queue.Parent;
        }
        path.Reverse();
        return path;
    }
    private Cluster GetCluster (Vector3 position, int Level)
    {
        List<Cluster[,]> allClusters = abstractGraph.allClustersAllLevels;
        foreach (Cluster[,] setOfClusters in allClusters)
        {
            for (int i = 0; i < setOfClusters.GetLength( 0 ); i++)
            {
                for (int j = 0; j < setOfClusters.GetLength( 1 ); j++)
                {
                    Cluster current = setOfClusters[i, j];
                    if (current.IsPositionInside( position ))
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
    private void ScanGridAndSetDefault (int Level)
    {
        List<Cluster[,]> allClusters = abstractGraph.allClustersAllLevels;

        foreach (Cluster[,] setOfClusters in allClusters)
        {
            for (int i = 0; i < setOfClusters.GetLength( 0 ); i++)
            {
                for (int j = 0; j < setOfClusters.GetLength( 1 ); j++)
                {
                    Cluster current = setOfClusters[i, j];
                    if (current.level == Level)
                    {
                        for (int m = 0; m < current.clusterNodes.Count - 1; m++)
                        {
                            Node node = current.clusterNodes[m];
                            node.gCost = Mathf.Infinity;
                            node.SetFCost();
                            node.SetParent( null );
                        }
                    }
                }
            }
        }
    }

}

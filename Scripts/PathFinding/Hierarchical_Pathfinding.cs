using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


public class Hierarchical_Pathfinding : MonoBehaviour
{
    //Recieves the Node from PlayerController and converts into a path list
    // Give the path list to the Movement.cs
    private AbstractGraph abstractGraph;
    int MaxLevel;
    //Events publishers scripts
    private PlayerController playerController;
    Node endNode = null;
    Node startNode = null;
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
        startNode = pos.startNode;
        endNode = pos.endNode;

        List<Node> abstractPath = HierarchicalSearch( startNode, endNode, MaxLevel );
        List<Cluster> clusterPath = RefinePath( abstractPath, MaxLevel );

    }


    private List<Node> HierarchicalSearch (Node start, Node end, int level)
    {
        InsertNode( start, level );
        InsertNode( end, level );
        if (start.neighbours.Contains( end ))
        {
            start.neighbours.Remove( end );
        }
        List<Node> abstractPath = SearchForPath( start, end );

        return abstractPath;

        void InsertNode (Node node, int maxLevel)
        {

            for (int i = 1; i <= maxLevel; i++)
            {
                Cluster c = GetCluster( node.WorldPosition, i );
                node.cluster = c;
                ConnectToBorder( node, c );
            }
            node.level = maxLevel;
            node.neighbours.Sort( new Node() );

            void ConnectToBorder (Node node, Cluster cluster)
            {
                int level = cluster.level;
                if (cluster.clusterNodes.Contains( node ))
                {
                    return;
                }
                foreach (Node n in cluster.clusterNodes)
                {
                    if (n.level < level)
                    {
                        continue;
                    }
                    node.AddNeighbour( n );
                    n.AddNeighbour( node );

                }
                cluster.AddNodeToCluster( node );

                //disconnect node from the cluster later
            }
        }

    }

    List<Cluster> RefinePath (List<Node> path, int level)
    {
        DebugPath( path );

        Hierarchical( path, level );

        void Hierarchical (List<Node> path, int level)
        {
            level--;
            if (level < 1)
            {
                return;
            }
            var nodesGrouped = path.GroupBy( node => ( node.cluster ) );
            List<Node> lesserPath = new List<Node>();
            foreach (var node in nodesGrouped)
            {
                List<Node> less = HierarchicalSearch( node.First(), node.Last(), level );
                lesserPath.AddRange( less );
            }
            DebugPath( lesserPath );
            Hierarchical( lesserPath, level );
        }

        return null;

        void DebugPath (List<Node> path)
        {
            if (path[0].level == 1)
            {
                // HPA_Utils.ShowPathLines( path, Color.blue );
                foreach (Node n in path)
                {
                    HPA_Utils.DrawCrossInPosition( n.WorldPosition, Color.blue );
                }
            }
            if (path[0].level == 2)
            {
                HPA_Utils.ShowPathLines( path, Color.yellow );
                foreach (Node n in path)
                {
                    HPA_Utils.DrawCrossInPosition( n.WorldPosition, Color.yellow );
                }
            }
            if (path[0].level == 3)
            {
                HPA_Utils.ShowPathLines( path, Color.green );
                foreach (Node n in path)
                {
                    HPA_Utils.DrawCrossInPosition( n.WorldPosition, Color.green );
                }
            }

        }
    }
    //Helpers
    private List<Node> SearchForPath (Node startNode, Node endNode)
    {
        SortedList<float, Node> openList = new SortedList<float, Node>();
        List<Node> closedList = new List<Node>();
        if (startNode.GridPosition == endNode.GridPosition)
        {
            return new List<Node>() { startNode };
        }
        ScanGridAndSetDefault();


        startNode.gCost = 0;
        startNode.hCost = Utils.ManhatamDistance( startNode.GridPosition, endNode.GridPosition );

        startNode.SetFCost();

        openList.Add( startNode.FCost, startNode );
        while (openList.Count > 0)
        {
            Node currentNode = openList.Values[0];

            openList.RemoveAt( 0 );
            closedList.Add( currentNode );
            foreach (var pair in currentNode.neighbours)
            {
                Node neighbour = pair;
                if (neighbour.GridPosition == endNode.GridPosition)
                {
                    neighbour.SetParent( currentNode );
                    return CalculatePath( neighbour );
                }

                float newG = currentNode.gCost + Utils.ManhatamDistance( neighbour.GridPosition, currentNode.GridPosition );

                if (newG < neighbour.gCost)
                {
                    ConfigureNeighbour( endNode, currentNode, neighbour, newG );

                    if (!openList.Values.Contains( neighbour ))
                    {
                        if (!openList.ContainsKey( neighbour.FCost ))
                        {
                            openList.Add( neighbour.FCost, neighbour );
                        }
                        else
                        {
                            Node insider = openList[neighbour.FCost];
                            if (insider.gCost < neighbour.gCost)
                            {
                                openList.Remove( neighbour.FCost );
                                openList.Add( neighbour.FCost, neighbour );
                            }
                        }

                    }
                }
            }
        }
        Debug.Log( $"Didn't find it - start Node: {startNode.WorldPosition}" );

        return null;


        void ScanGridAndSetDefault ( )
        {
            List<Cluster[,]> allClusters = abstractGraph.allClustersAllLevels;

            foreach (Cluster[,] setOfClusters in allClusters)
            {
                for (int i = 0; i < setOfClusters.GetLength( 0 ); i++)
                {
                    for (int j = 0; j < setOfClusters.GetLength( 1 ); j++)
                    {
                        Cluster current = setOfClusters[i, j];

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
        static void ConfigureNeighbour (Node endNode, Node currentNode, Node neighbour, float newG)
        {
            neighbour.SetParent( currentNode );
            neighbour.gCost = newG;
            neighbour.hCost = Utils.ManhatamDistance( neighbour.GridPosition, endNode.GridPosition );
            neighbour.SetFCost();
        }
        List<Node> CalculatePath (Node endNode)
        {
            List<Node> path = new List<Node>();

            path.Add( endNode );
            Node queue = endNode;
            while (queue.Parent != null)
            {
                path.Add( queue.Parent );
                queue = queue.Parent;
            }
            path.Reverse();


            path[0].cluster.clusterNodes.Remove( path[0] );
            path[path.Count - 1].cluster.clusterNodes.Remove( path[path.Count - 1] );

            return path;
        }
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
}

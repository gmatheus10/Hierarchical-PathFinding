using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hierarchical_Pathfinding : MonoBehaviour
{
    //Recieves the Node from PlayerController and converts into a path list
    // Give the path list to the PF_Module script
    private AbstractGraph abstractGraph;
    private PathFinding pathFinding;
    private WaypointPathFinding waypointPathFinding;
    private Vector3 startPos;
    private Vector3 goalPos;
    //Events publishers scripts
    private PlayerController playerController;
    private void Awake ( )
    {
        abstractGraph = gameObject.GetComponent<AbstractGraph>();
        playerController = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerController>();
    }
    private void Start ( )
    {
        playerController.OnPlayerDestinationSet += PlayerController_DestinationRecieved;
    }
    private void PlayerController_DestinationRecieved (object sender, PlayerController.PlayerPositions pos)
    {
        startPos = pos.currentPos;
        goalPos = pos.destinationPos;

        List<Cluster> path = HierarchicalSearch( startPos, goalPos, abstractGraph.Level );
        HPA_Utils.ShowPathLines( path );
    }
    List<Cluster> HierarchicalSearch (Vector3 start, Vector3 end, int level)
    {
        waypointPathFinding = new WaypointPathFinding();

        InsertNode( start, level );
        InsertNode( end, level );
        // LinkClusters( startNode, endNode );
        List<Cluster> abstractPath = SearchForPath( startPos, goalPos, level );
        //refine the path by reducing it in small level 1 lists to pass to the PF_Module(renaming it later) so that it can move the object in the grid
        return abstractPath;
    }
    void InsertNode (Vector3 Pos, int maxLevel)
    {
        List<Cluster[,]> allClusters = abstractGraph.allClustersAllLevels;
        Node node = null;

        foreach (Cluster[,] setOfClusters in allClusters)
        {
            for (int i = 0; i < setOfClusters.GetLength( 0 ); i++)
            {
                for (int j = 0; j < setOfClusters.GetLength( 1 ); j++)
                {
                    Cluster current = setOfClusters[i, j];
                    if (current.IsPositionInside( Pos ))
                    {
                        // List<Waypoint> setOfWaypoints = new List<Waypoint>();
                        node = InstantiateNode( Pos, current );
                        ConnectToBorder( node, current );
                        #region waypointListTentative
                        //ConvertNodeAndStore( setOfWaypoints, current, node );
                        //SetWaypointNeighbours( setOfWaypoints );
                        //if goalCluster is not equal no startCluster, then i need a link node between the clusters to make them neighbour to each other
                        #endregion

                    }
                }
            }
        }

        AbstractGraph.LevelUpNode( maxLevel, node );

        static Node InstantiateNode (Vector3 Pos, Cluster current)
        {
            Node node = new Node();
            node.SetPositions( Pos );
            node.cluster = current;
            return node;
        }
    }
    private void ConnectToBorder (Node node, Cluster cluster)
    {
        int level = cluster.level;
        Edge edge = null;
        List<Cell> path = null;
        pathFinding = new PathFinding( cluster.clusterGrid );
        foreach (Node n in cluster.clusterNodes)
        {
            if (n.level < level)
            {
                continue;
            }
            path = pathFinding.FindPath( node.worldPosition, n.worldPosition );
            if (path != null)
            {
                edge = new Edge( node, n, level, Edge.EdgeType.INTRA );
                HPA_Utils.DrawCrossInPosition( node.worldPosition, Color.green );
                //HPA_Utils.DrawEdge( edge, Color.blue, 10000f );
            }
        }
    }

    private List<Cluster> SearchForPath (Vector3 start, Vector3 end, int Level)
    {
        SortedList<float, Cluster> openList = new SortedList<float, Cluster>();
        List<Cluster> closedList = new List<Cluster>();
        ScanGridAndSetDefault( Level );

        Cluster startCluster = GetCluster( start, Level );
        Cluster endCluster = GetCluster( end, Level );

        startCluster.gCost = 0;
        startCluster.hCost = CalculateDistance( startCluster, endCluster );
        startCluster.SetFCost();

        openList.Add( startCluster.FCost, startCluster );

        while (openList.Count > 0)
        {
            Cluster currentCluster = openList.Values[0];
            if (currentCluster == endCluster)
            {
                Debug.Log( "found" );
                return CalculatePath( currentCluster );
            }
            openList.RemoveAt( 0 );
            closedList.Add( currentCluster );
            List<Cluster> neighbourList = GetNeighboursList( currentCluster, Level );

            foreach (Cluster neighbour in neighbourList)
            {
                float newG = currentCluster.gCost + CalculateDistance( neighbour, currentCluster );
                neighbour.cameFrom = currentCluster;
                if (newG < neighbour.gCost)
                {
                    neighbour.gCost = newG;
                    neighbour.hCost = CalculateDistance( neighbour, endCluster );
                    neighbour.SetFCost();

                    if (!openList.Values.Contains( neighbour ))
                    {
                        try
                        {
                            openList.Add( neighbour.FCost, neighbour );
                        }
                        catch (System.Exception)
                        {
                            int index = openList.Keys.IndexOf( neighbour.FCost );
                            if (openList.Values[index].gCost <= neighbour.gCost)
                            {
                                continue;
                            }
                            else
                            {
                                openList.RemoveAt( index );
                                openList.Add( neighbour.FCost, neighbour );
                            }
                        }
                    }
                }
            }
        }

        Debug.Log( "Didn't find it" );
        return null;

        float CalculateDistance (Cluster a, Cluster b)
        {
            return ( a.originPosition - b.originPosition ).magnitude;
        }
    }
    private List<Cluster> CalculatePath (Cluster endCluster)
    {
        List<Cluster> path = new List<Cluster>();
        path.Add( endCluster );
        Cluster queue = endCluster;
        while (queue.cameFrom != null)
        {
            path.Add( queue.cameFrom );
            queue = queue.cameFrom;
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
    private List<Cluster> GetNeighboursList (Cluster current, int Level)
    {
        Vector3 center = current.originPosition + new Vector3( 0.5f * current.size.x, 0.5f * current.size.y, 0 );
        List<Cluster> neighbours = new List<Cluster>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                Vector3 neighbourPosition = center + new Vector3( i * current.size.x, j * current.size.y );
                Cluster neighbour = GetCluster( neighbourPosition, Level );
                if (neighbour != null)
                {
                    neighbours.Add( neighbour );
                }

            }
        }
        return neighbours;

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
                        current.gCost = Mathf.Infinity;
                        current.SetFCost();
                        current.cameFrom = null;
                    }
                }
            }
        }
    }
    #region waypointHelpers
    //private void SetWaypointNeighbours (List<Waypoint> setOfWaypoints)
    //{
    //    foreach (Waypoint waypoint in setOfWaypoints)
    //    {
    //        List<Waypoint> filtered = new List<Waypoint>();
    //        for (int i = 0; i < setOfWaypoints.Count; i++)
    //        {
    //            if (setOfWaypoints[i].position != waypoint.position)
    //            {
    //                if (!filtered.Contains( setOfWaypoints[i] ))
    //                {
    //                    filtered.Add( setOfWaypoints[i] );
    //                }
    //            }
    //        }
    //        waypoint.SetNeighbours( filtered );
    //    }
    //}

    //private void ConvertNodeAndStore (List<Waypoint> setOfWaypoints, Cluster current, Node node)
    //{
    //    Waypoint waypoint = null;
    //    waypoint = waypointPathFinding.NodeToWaypoint( node );
    //    waypoint.cluster = current;
    //    CheckAndAddWaypoint( setOfWaypoints, waypoint );
    //    foreach (Node n in current.clusterNodes)
    //    {
    //        waypoint = waypointPathFinding.NodeToWaypoint( n );
    //        waypoint.cluster = current;
    //        CheckAndAddWaypoint( setOfWaypoints, waypoint );
    //        //if (n.pair != null)
    //        //{
    //        //    Waypoint pair = waypointPathFinding.NodeToWaypoint( n.pair );
    //        //    CheckAndAddWaypoint( setOfWaypoints, pair );
    //        //}
    //    }

    //    static void CheckAndAddWaypoint (List<Waypoint> setOfWaypoints, Waypoint waypoint)
    //    {
    //        if (!setOfWaypoints.Contains( waypoint ) && waypoint != null)
    //        {
    //            setOfWaypoints.Add( waypoint );
    //        }
    //    }
    //}

    #endregion
}

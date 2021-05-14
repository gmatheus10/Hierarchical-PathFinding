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
    private void Update ( )
    {
        // HPA_Utils.ShowPathLines( HierarchicalSearch( startPos, goalPos, abstractGraph.Level ) );
    }
    private void PlayerController_DestinationRecieved (object sender, PlayerController.PlayerPositions pos)
    {
        startPos = pos.currentPos;
        goalPos = pos.destinationPos;
        List<Vector3> path = HierarchicalSearch( startPos, goalPos, abstractGraph.Level );
        HPA_Utils.ShowPathLines( path );
    }
    List<Vector3> HierarchicalSearch (Vector3 start, Vector3 end, int level)
    {
        waypointPathFinding = new WaypointPathFinding();

        InsertNode( start, level );
        InsertNode( end, level );
        // LinkClusters( startNode, endNode );
        List<Vector3> abstractPath = waypointPathFinding.NavigateToWaypoint( start, end );
        //refine the path by reducing it in small level 1 lists to pass to the PF_Module(renaming it later) so that it can move the object in the grid
        return abstractPath;
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
                            List<Waypoint> setOfWaypoints = new List<Waypoint>();
                            node = InstantiateNode( Pos, current );
                            if (node != null)
                            {
                                ConnectToBorder( node, current );
                                ConvertNodeAndStore( setOfWaypoints, current, node );
                                SetWaypointNeighbours( setOfWaypoints );
                                //if goalCluster is not equal no startCluster, then i need a link node between the clusters to make them neighbour to each other
                            }
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
        #region linkClusters
        //void LinkClusters (Node startNode, Node endNode)
        //{
        //    if (startNode.cluster != endNode.cluster) // <-- might be a headache if the cluster level is in consideration
        //    {
        //        Cluster start = startNode.cluster;
        //        Cluster end = endNode.cluster;
        //        float minimum = Mathf.Infinity;
        //        float distance = 0f;
        //        Node savedS = null;
        //        Node savedE = null;
        //        for (int i = 0; i < start.clusterNodes.Count - 1; i++)
        //        {
        //            for (int j = 0; j < end.clusterNodes.Count - 1; j++)
        //            {
        //                Node n1 = start.clusterNodes[i];
        //                Node n2 = end.clusterNodes[j];
        //                distance = ( n1.worldPosition - n2.worldPosition ).magnitude;
        //                if (distance < minimum)
        //                {
        //                    minimum = distance;
        //                    savedS = n1;
        //                    savedE = n2;
        //                }
        //            }
        //        }
        //        Waypoint wayS = waypointPathFinding.waypoints.Find( e => e.position == savedS.worldPosition );
        //        Waypoint wayE = waypointPathFinding.waypoints.Find( e => e.position == savedE.worldPosition );
        //        wayS.neighbours.Add( wayE );
        //        wayE.neighbours.Add( wayS );
        //    }
        //}
        #endregion
    }

    private void ConnectToBorder (Node node, Cluster cluster)
    {
        int level = cluster.level;
        Edge edge = null;
        List<Cell> path = null;
        Vector3 nodePos = node.worldPosition;
        pathFinding = new PathFinding( cluster.clusterGrid );
        foreach (Node n in cluster.clusterNodes)
        {
            path = pathFinding.FindPath( nodePos, n.worldPosition );
            if (path != null)
            {
                edge = new Edge( node, n, level, Edge.EdgeType.INTRA );
                HPA_Utils.DrawCrossInPosition( nodePos, Color.green );
                //HPA_Utils.DrawEdge( edge, Color.blue, 10000f );
            }
        }
    }


    private void SetWaypointNeighbours (List<Waypoint> setOfWaypoints)
    {
        foreach (Waypoint waypoint in setOfWaypoints)
        {
            List<Waypoint> filtered = new List<Waypoint>();
            for (int i = 0; i < setOfWaypoints.Count; i++)
            {
                if (setOfWaypoints[i].position != waypoint.position)
                {
                    if (!filtered.Contains( setOfWaypoints[i] ))
                    {
                        filtered.Add( setOfWaypoints[i] );
                    }
                }
            }
            waypoint.SetNeighbours( filtered );
        }
    }

    private void ConvertNodeAndStore (List<Waypoint> setOfWaypoints, Cluster current, Node node)
    {
        Waypoint waypoint = null;
        waypoint = waypointPathFinding.NodeToWaypoint( node );
        waypoint.cluster = current;
        CheckAndAddWaypoint( setOfWaypoints, waypoint );
        foreach (Node n in current.clusterNodes)
        {
            waypoint = waypointPathFinding.NodeToWaypoint( n );
            waypoint.cluster = current;
            CheckAndAddWaypoint( setOfWaypoints, waypoint );
            //if (n.pair != null)
            //{
            //    Waypoint pair = waypointPathFinding.NodeToWaypoint( n.pair );
            //    CheckAndAddWaypoint( setOfWaypoints, pair );
            //}
        }

        static void CheckAndAddWaypoint (List<Waypoint> setOfWaypoints, Waypoint waypoint)
        {
            if (!setOfWaypoints.Contains( waypoint ) && waypoint != null)
            {
                setOfWaypoints.Add( waypoint );
            }
        }
    }


}

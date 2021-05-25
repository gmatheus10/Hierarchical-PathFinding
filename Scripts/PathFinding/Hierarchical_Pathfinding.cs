using System.Collections.Generic;
using UnityEngine;

public class Hierarchical_Pathfinding : MonoBehaviour
{
    //Recieves the Node from PlayerController and converts into a path list
    // Give the path list to the PF_Module script
    private int MaxLevel;
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
        MaxLevel = abstractGraph.Level;
    }
    private void PlayerController_DestinationRecieved (object sender, PlayerController.PlayerPositions pos)
    {
        startPos = pos.currentPos;
        goalPos = pos.destinationPos;

        List<Cluster> MaxLevelPath = HierarchicalSearch( startPos, goalPos, abstractGraph.Level );
        HPA_Utils.ShowPathClusters( MaxLevelPath, Color.blue );
        PathTree( MaxLevelPath );

    }

    private List<Cluster> HierarchicalSearch (Vector3 start, Vector3 end, int level)
    {
        waypointPathFinding = new WaypointPathFinding();

        InsertNode( start, level );
        InsertNode( end, level );
        List<Cluster> abstractPath = SearchForPath( startPos, goalPos, level );
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
                            node = InstantiateNode( Pos, current );
                            ConnectToBorder( node, current );
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
            void ConnectToBorder (Node node, Cluster cluster)
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

                    }
                }
            }
        }
    }

    private void PathTree (List<Cluster> MaxLevelPath)
    {
        TreeData<List<Cluster>> pathTree = new TreeData<List<Cluster>>( $"Level{MaxLevel}", MaxLevelPath );
        Vector3 startPosition = startPos;
        NavigatingOnTree( MaxLevelPath );

        void NavigatingOnTree (List<Cluster> path)
        {
            Vector3 navStart = Vector3.zero;
            for (int i = 1; i <= path.Count; i++)
            {
                Cluster currentCluster = path[i - 1];
                int lesserLevel = currentCluster.level - 1;

                if (lesserLevel < 1)
                {
                    break;
                }
                else
                {
                    try
                    {
                        Cluster nextCluster = path[i];

                        List<Cluster> lesserPath = NavigateInsideCluster( currentCluster, nextCluster, ref navStart, lesserLevel );
                        AddTreeAndContinue( pathTree, i, lesserLevel, lesserPath );
                    }
                    catch (System.Exception)
                    {

                        List<Cluster> lesserPath = NavigateInsideCluster( currentCluster, null, ref navStart, lesserLevel );
                        AddTreeAndContinue( pathTree, i, lesserLevel, lesserPath );
                    }

                }
            }

            List<Cluster> NavigateInsideCluster (Cluster currentCluster, Cluster nextCluster, ref Vector3 navStart, int lesserLevel)
            {
                Vector3 endPosition = currentCluster.originPosition + new Vector3( currentCluster.size.x - 0.5f, currentCluster.size.y - 0.5f );
                Vector3 newStart = navStart;
                if (isPositionInSubcluster( currentCluster, goalPos ))
                {
                    endPosition = goalPos;
                }
                else
                {
                    GetClosestExit( currentCluster, nextCluster, ref navStart, ref endPosition );
                }

                if (!isPositionInSubcluster( currentCluster, startPos ))
                {
                    startPosition = newStart;
                }

                List<Cluster> lesserPath = SearchForPath( startPosition, endPosition, lesserLevel, currentCluster );

                return lesserPath;
                bool isPositionInSubcluster (Cluster currentCluster, Vector3 position)
                {
                    foreach (var c in currentCluster.lesserLevelClusters)
                    {
                        if (c.IsPositionInside( position ))
                        {
                            return true;
                        }

                    }

                    return false;
                }
                void GetClosestExit (Cluster currentCluster, Cluster nextCluster, ref Vector3 navStart, ref Vector3 endPosition)
                {
                    float min = Mathf.Infinity;
                    foreach (Node n in currentCluster.clusterNodes)
                    {
                        float distance = ( n.worldPosition - startPos ).magnitude;
                        if (distance < min)
                        {
                            if (nextCluster.IsPositionInside( n.pair.worldPosition ))
                            {
                                foreach (Cluster sub in currentCluster.lesserLevelClusters)
                                {
                                    if (sub.IsPositionInside( n.worldPosition ) && !sub.IsPositionInside( startPos ))
                                    {
                                        Cluster startCluster = GetCluster( startPos, sub.level );
                                        if (!isBlocked( startCluster, sub ))
                                        {
                                            endPosition = n.worldPosition;
                                            navStart = n.pair.worldPosition;
                                            min = distance;
                                        }
                                    }
                                    else if (sub.IsPositionInside( n.worldPosition ) && sub.IsPositionInside( startPos ))
                                    {
                                        endPosition = n.worldPosition;
                                        navStart = n.pair.worldPosition;
                                        min = distance;
                                    }
                                }
                            }
                        }
                    }
                    bool isBlocked (Cluster currentCluster, Cluster neighbour)
                    {
                        string key = $"{currentCluster.originPosition}->{neighbour.originPosition}";
                        if (abstractGraph.setOfEntrances.ContainsKey( key ))
                        {
                            Entrance entrance = abstractGraph.setOfEntrances[key];
                            if (entrance.isBlocked)
                            {
                                return true;
                            }
                            return false;
                        }
                        return false;
                    }
                }
            }

            void AddTreeAndContinue (TreeData<List<Cluster>> pathTree, int i, int lesserLevel, List<Cluster> lesserPath)
            {
                if (lesserPath != null)
                {
                    HPA_Utils.ShowPathClusters( lesserPath, Color.yellow );
                    HPA_Utils.ShowPathLines( lesserPath, Color.red );
                    TreeData<List<Cluster>> subTree = new TreeData<List<Cluster>>( $"Level{lesserLevel}.{i - 1}", lesserPath );
                    pathTree.Add( subTree );
                    NavigatingOnTree( lesserPath );
                }
            }
        }
    }


    //Helpers
    private List<Cluster> SearchForPath (Vector3 start, Vector3 end, int Level, Cluster parent = null)
    {
        SortedList<float, Cluster> openList = new SortedList<float, Cluster>();
        List<Cluster> closedList = new List<Cluster>();
        ScanGridAndSetDefault( Level );

        Cluster startCluster = GetCluster( start, Level );
        Cluster endCluster = GetCluster( end, Level );

        startCluster.gCost = 0;
        startCluster.hCost = Utils.ManhatamDistance( startCluster, endCluster );

        startCluster.SetFCost();

        openList.Add( startCluster.FCost, startCluster );
        while (openList.Count > 0)
        {
            Cluster currentCluster = openList.Values[0];
            if (currentCluster == endCluster)
            {
                return CalculatePath( currentCluster );
            }
            openList.RemoveAt( 0 );
            closedList.Add( currentCluster );
            List<Cluster> neighbourList = GetNeighboursList( currentCluster, Level, parent );

            foreach (Cluster neighbour in neighbourList)
            {
                float newG = currentCluster.gCost + Utils.ManhatamDistance( neighbour, currentCluster );
                if (neighbour == endCluster)
                {
                    neighbour.cameFrom = currentCluster;
                    return CalculatePath( neighbour );
                }

                if (newG < neighbour.gCost)
                {
                    neighbour.gCost = newG;
                    neighbour.hCost = Utils.ManhatamDistance( neighbour, endCluster );
                    neighbour.SetFCost();

                    if (!openList.Values.Contains( neighbour ))
                    {
                        try
                        {
                            neighbour.cameFrom = currentCluster;
                            //if is on level 1 check if it cluster.cameFrom.entrance has an entrance to current cluster. If not, then continue
                            openList.Add( neighbour.FCost, neighbour );
                        }
                        catch (System.Exception)
                        {
                            if (!closedList.Contains( neighbour ))
                            {
                                var pair = openList[neighbour.FCost];
                                if (pair.gCost <= neighbour.gCost)
                                {
                                    continue;
                                }
                                else
                                {
                                    openList.Remove( neighbour.FCost );
                                    openList.Add( neighbour.FCost, neighbour );
                                }
                            }
                        }
                    }
                }
            }
        }
        Debug.Log( "Didn't find it" );
        return null;
    }
    private List<Cluster> CalculatePath (Cluster endCluster)
    {
        List<Cluster> path = new List<Cluster>() { endCluster };

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
    private List<Cluster> GetNeighboursList (Cluster current, int Level, Cluster parent = null)
    {
        Vector3 center = current.originPosition + new Vector3( 0.5f * current.size.x, 0.5f * current.size.y, 0 );
        List<Cluster> neighbours = new List<Cluster>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (( i == 0 && j == 0 ) || ( i == 1 && j == 1 ) || ( i == -1 && j == -1 ) || ( i == -1 && j == 1 ) || ( i == 1 && j == -1 ))
                {
                    continue;
                }
                Vector3 neighbourPosition = center + new Vector3( i * current.size.x, j * current.size.y );
                if (parent != null)
                {
                    if (!parent.IsPositionInside( neighbourPosition ))
                    {
                        continue;
                    }
                }

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

}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractGraph : MonoBehaviour
{
    private DebugOnClick DebugOnClick;

    private CreateGrid createGrid;
    public Grid<Cell> grid;

    public int Level;
    public Vector2Int LevelOneClusterSize;

    private Cluster[,] clustersLevel1;
    private Cluster[,] multiLevelCluster;
    public List<Cluster[,]> allClustersAllLevels = new List<Cluster[,]>();

    public Dictionary<string, Entrance> setOfEntrances = new Dictionary<string, Entrance>();

    private void Awake ( )
    {
        createGrid = gameObject.GetComponent<CreateGrid>();
        DebugOnClick = gameObject.GetComponent<DebugOnClick>();
    }
    private void Start ( )
    {
        grid = createGrid.grid;
        PreProcessing( Level );
        //DebugOnClick.PassMousePosition += DebugCluster;
    }
    void DebugCluster (Vector3 position)
    {
        int LEVEL = 2;

        foreach (Cluster[,] level in allClustersAllLevels)
        {
            for (int i = 0; i < level.GetLength( 0 ); i++)
            {
                for (int j = 0; j < level.GetLength( 1 ); j++)
                {
                    Cluster cluster = level[i, j];
                    if (cluster.level == LEVEL)
                    {
                        if (cluster.IsPositionInside( position ))
                        {
                            HPA_Utils.DrawNodesInCluster( cluster );

                        }
                    }
                }
            }
        }
    }
    void PreProcessing (int maxLevel)
    {
        AbstractMaze();
        BuildGraph();
        for (int l = 2; l <= maxLevel; l++)
        {
            AddLevelToGraph( l );
        }
    }
    void AbstractMaze ( )
    {
        clustersLevel1 = BuildClusters();
        for (int i = 0; i < clustersLevel1.GetLength( 0 ); i++)
        {
            for (int j = 0; j < clustersLevel1.GetLength( 1 ); j++)
            {
                Cluster c1 = clustersLevel1[i, j];

                for (int l = -1; l <= 1; l++)
                {
                    for (int m = -1; m <= 1; m++)
                    {
                        //checks only up, down, left and right
                        if (( l == 0 && m == 0 ) || ( l == 1 && m == 1 ) || ( l == -1 && m == -1 ) || ( l == -1 && m == 1 ) || ( l == 1 && m == -1 ))
                        {
                            continue;
                        }
                        if (( i + l < 0 ) || ( i + l ) >= clustersLevel1.GetLength( 0 ))
                        {
                            continue;
                        }
                        if (( j + m < 0 ) || ( j + m ) >= clustersLevel1.GetLength( 1 ))
                        {
                            continue;
                        }
                        Cluster nextCluster = clustersLevel1[i + l, j + m];
                        HandlePairOfClusters( c1, nextCluster );
                    }
                }

            }
        }

        void HandlePairOfClusters (Cluster c1, Cluster nextCluster)
        {
            KeyValuePair<string, List<Cell>>[] border = GetAdjacentBorder( c1, nextCluster );

            BuildEntrances( c1, nextCluster, border );

        }

    }
    public Cluster[,] BuildClusters (int level = 1)
    {
        int clustersOnX, clustersOnY;
        GetClusterCount( level, out clustersOnX, out clustersOnY );

        Cluster[,] setOfClusters = new Cluster[clustersOnX, clustersOnY];
        Cluster[,] lesserClustersArray = clustersLevel1;
        for (int x = 0; x < clustersOnX; x++)
        {
            for (int y = 0; y < clustersOnY; y++)
            {
                Cluster cluster = InstantiateCluster( level, x, y, lesserClustersArray );
                setOfClusters[x, y] = cluster;
                FillCluster( cluster );
            }
        }
        clustersLevel1 = setOfClusters;
        return setOfClusters;


        Cluster InstantiateCluster (int level, int x, int y, Cluster[,] lesserClustersArray)
        {
            int levelAdjustmentX = (int)( LevelOneClusterSize.x * Mathf.Pow( 2, level - 1 ) );
            int levelAdjustmentY = (int)( LevelOneClusterSize.y * Mathf.Pow( 2, level - 1 ) );

            Vector2Int size = new Vector2Int( levelAdjustmentX, levelAdjustmentY );
            Vector3 clusterPosition = grid.GetWorldPosition( x * size.x, y * size.y );

            Cluster cluster = new Cluster( size, clusterPosition, level );

            cluster.SetGridPosition( grid.GetGridPosition( clusterPosition ) );
            Color color = new Color( 0.3f * level, -1f + 0.7f * level, 0.05f * level, 1 );
            HPA_Utils.DrawClusters( cluster.OriginPosition, cluster.size, color );

            if (level > 1)
            {
                cluster.AddLesserClusters( lesserClustersArray );

            }
            return cluster;
        }
        void GetClusterCount (int level, out int clustersOnX, out int clustersOnY)
        {
            clustersOnX = grid.Width / LevelOneClusterSize.x;
            clustersOnY = grid.Height / LevelOneClusterSize.y;
            if (level > 1)
            {
                clustersOnX /= level;
                clustersOnY /= level;
                //call cluster.AddLesserCluster using clustersLevel1 for the level 2
                //then update clustersLevel1 for the current set
            }
        }
        void FillCluster (Cluster c)
        {
            string bottom = "bottom";
            string top = "top";
            string right = "right";
            string left = "left";

            List<Cell> bottomBorder = new List<Cell>();
            List<Cell> topBorder = new List<Cell>();
            List<Cell> rightBorder = new List<Cell>();
            List<Cell> leftBorder = new List<Cell>();

            for (int i = 0; i < c.size.x; i++)
            {
                Vector3 bottomCellPosition = new Vector3( i, 0, 0 ) + c.OriginPosition;
                Cell bottomCell = grid.GetGridObject( bottomCellPosition );

                bottomBorder.Add( bottomCell );
                for (int j = 0; j < c.size.y; j++)
                {
                    Vector3 cellPosition = new Vector3( i, j, 0 ) + c.OriginPosition;
                    Cell cell = grid.GetGridObject( cellPosition );
                    if (j == c.size.y - 1)
                    {
                        topBorder.Add( cell );
                    }
                    if (i == c.size.x - 1)
                    {
                        rightBorder.Add( cell );
                    }
                    if (i == 0)
                    {
                        leftBorder.Add( cell );
                    }
                }
            }
            c.borders.Add( bottom, bottomBorder );
            c.borders.Add( left, leftBorder );
            c.borders.Add( right, rightBorder );
            c.borders.Add( top, topBorder );

        }

    }



    private void BuildEntrances (Cluster c1, Cluster c2, KeyValuePair<string, List<Cell>>[] AdjacentBorder)
    {
        Entrance entrance = new Entrance();

        List<Cell> entranceCells = new List<Cell>();
        List<Cell> symmEntranceCells = new List<Cell>();

        KeyValuePair<string, List<Cell>> C1border = AdjacentBorder[0];
        KeyValuePair<string, List<Cell>> C2border = AdjacentBorder[1];

        List<Cell> c1BorderCells = C1border.Value;
        List<Cell> c2BorderCells = C2border.Value;


        for (int i = 0; i < c1BorderCells?.Count; i++)
        {
            Cell cell1 = c1BorderCells[i];
            Cell cell2 = c2BorderCells?[i];
            AddCellsToLists( cell1, cell2 );
        }

        if (entranceCells.Count > 0 && symmEntranceCells.Count > 0)
        {
            entrance.FillEntrance( entranceCells );
            entrance.FillSymmEntrance( symmEntranceCells );
            entrance.SetClusters( c1, c2 );
        }
        else
        {
            return;
        }

        c1.AddEntrance( entrance );


        string key1 = $"{c1.OriginPosition}->{c2.OriginPosition}";
        setOfEntrances.Add( key1, entrance );

        void AddCellsToLists (Cell cell1, Cell cell2)
        {
            entranceCells.Add( cell1 );
            symmEntranceCells.Add( cell2 );
        }
    }
    private KeyValuePair<string, List<Cell>>[] GetAdjacentBorder (Cluster c1, Cluster c2)
    {
        Vector3Int c1BottomLeft = c1.GridPosition; //BottomLeft
        Vector3Int c1TopRight = GetClusterEndGridPosition( c1 ); //UpperRight

        Vector3Int c2BottomLeft = c2.GridPosition; //BottomLeft
        Vector3Int c2TopRight = GetClusterEndGridPosition( c2 ); //UpperRight

        Vector3Int c2BotRight = new Vector3Int( c2TopRight.x, c2BottomLeft.y, 0 );
        Vector3Int c2TopLeft = new Vector3Int( c2BottomLeft.x, c2TopRight.y, 0 );

        Vector3Int c1BotRight = new Vector3Int( c1TopRight.x, c1BottomLeft.y, 0 );
        Vector3Int c1TopLeft = new Vector3Int( c1BottomLeft.x, c1TopRight.y, 0 );

        if (c1BottomLeft == c2TopLeft && c1BotRight == c2TopRight)
        {
            KeyValuePair<string, List<Cell>> bottom = new KeyValuePair<string, List<Cell>>( "bottom", c1.borders["bottom"] );
            KeyValuePair<string, List<Cell>> top = new KeyValuePair<string, List<Cell>>( "top", c2.borders["top"] );

            return new KeyValuePair<string, List<Cell>>[] { bottom, top };
        }
        else if (c1BotRight == c2BottomLeft && c1TopRight == c2TopLeft)
        {
            KeyValuePair<string, List<Cell>> right = new KeyValuePair<string, List<Cell>>( "right", c1.borders["right"] );
            KeyValuePair<string, List<Cell>> left = new KeyValuePair<string, List<Cell>>( "left", c2.borders["left"] );

            return new KeyValuePair<string, List<Cell>>[] { right, left };
        }
        else if (c1TopLeft == c2BottomLeft && c1TopRight == c2BotRight)
        {
            KeyValuePair<string, List<Cell>> top = new KeyValuePair<string, List<Cell>>( "top", c1.borders["top"] );
            KeyValuePair<string, List<Cell>> bottom = new KeyValuePair<string, List<Cell>>( "bottom", c2.borders["bottom"] );

            return new KeyValuePair<string, List<Cell>>[] { top, bottom };
        }
        else if (c1BottomLeft == c2BotRight && c1TopLeft == c2TopRight)
        {
            KeyValuePair<string, List<Cell>> left = new KeyValuePair<string, List<Cell>>( "left", c1.borders["left"] );
            KeyValuePair<string, List<Cell>> right = new KeyValuePair<string, List<Cell>>( "right", c2.borders["right"] );

            return new KeyValuePair<string, List<Cell>>[] { left, right };
        }
        return default;

        Vector3Int GetClusterEndGridPosition (Cluster cluster)
        {
            return grid.GetGridPosition( cluster.OriginPosition + new Vector3( cluster.size.x, cluster.size.y, 0 ) );
        }
    }
    void BuildGraph ( )
    {
        foreach (KeyValuePair<string, Entrance> entrance in setOfEntrances)
        {
            List<Cell> cells = entrance.Value.GetEntrance();

            List<Cell> vacant = new List<Cell>();
            List<Cell> full = new List<Cell>();

            int middle = 0;

            for (int i = 0; i < cells.Count; i++)
            {
                Cell c = cells[i];

                if (!c.isWall)
                {
                    vacant.Add( c );
                }
                else
                {
                    full.Add( c );
                    if (vacant.Count >= 1)
                    {
                        middle = Mathf.FloorToInt( vacant.Count * 0.5f );
                        InstantiateNode( entrance.Value, vacant[middle] );
                        vacant.Clear();
                    }
                    if (full.Count == cells.Count)
                    {
                        entrance.Value.isBlocked = true;
                    }
                }
            }
            if (vacant.Count > 0)
            {
                middle = Mathf.FloorToInt( vacant.Count * 0.5f );
                int half1 = Mathf.FloorToInt( middle * 0.5f );
                int half2 = Mathf.FloorToInt( ( middle + vacant.Count ) * 0.5f );
                InstantiateNode( entrance.Value, vacant[half1] );
                InstantiateNode( entrance.Value, vacant[half2] );
            }
        }
        ClusterAddGrid();
        allClustersAllLevels.Add( clustersLevel1 );

        for (int i = 0; i < clustersLevel1.GetLength( 0 ); i++)
        {
            for (int j = 0; j < clustersLevel1.GetLength( 1 ); j++)
            {
                Cluster cluster = clustersLevel1[i, j];
                List<Node> nodes = cluster.clusterNodes;
                CalculateEdge( nodes );
            }
        }


        void InstantiateNode (Entrance entrance, Cell middleCell)
        {
            if (!middleCell.isWall)
            {
                Cell symmMiddle = entrance.GetSymmetricalCell( middleCell );

                if (!symmMiddle.isWall)
                {
                    Cluster c1 = entrance.Cluster1;
                    Cluster c2 = entrance.Cluster2;

                    Node c1Node = NewNode( c1, middleCell );
                    Node c2Node = NewNode( c2, symmMiddle );

                    c1Node.SetPair( c2Node );
                    c2Node.SetPair( c1Node );

                    AddNode( entrance, c1Node, 1 );

                    c1.AddNodeToCluster( c1Node );
                    c2.AddNodeToCluster( c2Node );
                }
                else
                {
                    entrance.isBlocked = true;
                }
            }
            else
            {
                entrance.isBlocked = true;
            }
            Node NewNode (Cluster cluster, Cell CellNode)
            {
                Node node = new Node( cluster );
                node.SetPosition( CellNode.worldPosition );
                node.SetGridPosition( CellNode.gridPosition );
                return node;
            }
            void AddNode (Entrance entrance, Node node, int Level)
            {
                node.level = Level;
                entrance.AddNode( node );
            }
        }
        void ClusterAddGrid ( )
        {
            for (int i = 0; i < clustersLevel1.GetLength( 0 ); i++)
            {
                for (int j = 0; j < clustersLevel1.GetLength( 1 ); j++)
                {
                    Cluster cluster = clustersLevel1[i, j];
                    Grid<Cell> clusterGrid = grid.GetFractionOfGrid( cluster.OriginPosition, cluster.size, false );
                    cluster.SetGrid( clusterGrid );
                }
            }
        }
    }

    void AddLevelToGraph (int level)
    {
        multiLevelCluster = BuildClusters( level );
        for (int i = 0; i < multiLevelCluster.GetLength( 0 ); i++)
        {
            for (int j = 0; j < multiLevelCluster.GetLength( 1 ); j++)
            {
                Cluster currentCluster = multiLevelCluster[i, j];

                UpdateNeighbours( currentCluster, i, j );

                Grid<Cell> clusterGrid = grid.GetFractionOfGrid( currentCluster.OriginPosition, currentCluster.size, false );
                currentCluster.SetGrid( clusterGrid );
            }

        }
        allClustersAllLevels.Add( multiLevelCluster );


        void UpdateNeighbours (Cluster currentCluster, int i, int j)
        {

            for (int l = -1; l <= 1; l++)
            {
                for (int m = -1; m <= 1; m++)
                {
                    //checks only up, down, left and right
                    if (( l == 0 && m == 0 ) || ( l == 1 && m == 1 ) || ( l == -1 && m == -1 ) || ( l == -1 && m == 1 ) || ( l == 1 && m == -1 ))
                    {
                        continue;
                    }
                    if (( i + l < 0 ) || ( i + l ) >= multiLevelCluster.GetLength( 0 ))
                    {
                        continue;
                    }
                    if (( j + m < 0 ) || ( j + m ) >= multiLevelCluster.GetLength( 1 ))
                    {
                        continue;
                    }
                    Cluster nextCluster = multiLevelCluster[i + l, j + m];
                    HandleMultiLevelClusters( currentCluster, nextCluster );
                }
            }
            void HandleMultiLevelClusters (Cluster currentCluster, Cluster nextCluster)
            {
                if (!GetAdjacentBorder( currentCluster, nextCluster ).Equals( default( KeyValuePair<string, List<string>> ) ))
                {
                    UpdateEntrances( currentCluster, nextCluster );
                }
            }
            void UpdateEntrances (Cluster cluster, Cluster nextCluster)
            {

                Entrance newEntrance = new Entrance();
                List<Entrance> merge = new List<Entrance>();

                foreach (KeyValuePair<string, Entrance> pair in setOfEntrances)
                {
                    Entrance entrance = pair.Value;
                    if (entrance.HaveEntrance( cluster, nextCluster ))
                    {
                        merge.Add( entrance );
                    }
                }

                newEntrance = newEntrance.MergeEntrances( merge.ToArray() );
                cluster.AddEntrance( newEntrance );

                UpdateNodes( cluster, nextCluster );

                void UpdateNodes (Cluster cluster, Cluster nextCluster)
                {
                    foreach (var entrance in cluster.entrances)
                    {
                        foreach (Node node in entrance.Value.entranceNodes)
                        {
                            AddLeveledNode( cluster, node );
                            AddLeveledNode( nextCluster, node.Pair );
                        }
                    }
                    List<Node> nodes = cluster.clusterNodes;
                    CalculateEdge( nodes );

                    void AddLeveledNode (Cluster cluster, Node node)
                    {
                        Node newNode = new Node( cluster );
                        newNode.SetPosition( node.WorldPosition );
                        newNode.SetGridPosition( node.GridPosition );
                        newNode.SetPair( node.Pair );
                        newNode.level = cluster.level;
                        cluster.AddNodeToCluster( newNode );
                    }
                }
            }
        }
    }

    private static void CalculateEdge (List<Node> nodes)
    {
        for (int m = 0; m < nodes.Count - 1; m++)
        {
            Node n1 = nodes[m];
            for (int n = 0; n < nodes.Count - 1; n++)
            {
                if (m == n) { continue; }
                Node n2 = nodes[n];
                int distance = Utils.ManhatamDistance( n1.GridPosition, n2.GridPosition );
                n1.AddNeighbour( n2, distance );
            }
        }

    }
}


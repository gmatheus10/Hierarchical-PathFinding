using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AbstractGraph : MonoBehaviour
{
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
    }
    private void Start ( )
    {
        grid = createGrid.grid;
        PreProcessing( Level );
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
                Cluster nextCluster;
                try
                {
                    nextCluster = clustersLevel1[i + 1, j];
                    HandlePairOfClusters( c1, nextCluster );
                    nextCluster = clustersLevel1[i, j + 1];
                    HandlePairOfClusters( c1, nextCluster );
                }
                catch
                {
                    try
                    {
                        nextCluster = clustersLevel1[i + 1, j];
                        HandlePairOfClusters( c1, nextCluster );
                    }
                    catch
                    {
                        try
                        {
                            nextCluster = clustersLevel1[i, j + 1];
                            HandlePairOfClusters( c1, nextCluster );
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
        }

        void HandlePairOfClusters (Cluster c1, Cluster nextCluster)
        {
            if (IsAdjacent( c1, nextCluster ))
            {
                BuildEntrances( c1, nextCluster );
            }
        }

    }
    public Cluster[,] BuildClusters (int level = 1)
    {
        int clustersOnX = grid.Width / LevelOneClusterSize.x;
        int clustersOnY = grid.Height / LevelOneClusterSize.y;
        if (level > 1)
        {
            clustersOnX /= level;
            clustersOnY /= level;
            //call cluster.AddLesserCluster using clustersLevel1 for the level 2
            //then update clustersLevel1 for the current set
        }

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
    }
    private Cluster InstantiateCluster (int level, int x, int y, Cluster[,] lesserClustersArray)
    {
        int levelAdjustmentX = (int)( LevelOneClusterSize.x * Mathf.Pow( 2, level - 1 ) );
        int levelAdjustmentY = (int)( LevelOneClusterSize.y * Mathf.Pow( 2, level - 1 ) );
        Vector2Int size = new Vector2Int( levelAdjustmentX, levelAdjustmentY );
        Vector3 clusterPosition = grid.GetWorldPosition( x * size.x, y * size.y );

        Cluster cluster = new Cluster( size, clusterPosition, level );

        cluster.SetGridPosition( grid.GetGridPosition( clusterPosition ) );
        Color color = new Color( 0.3f * level, -1f + 0.7f * level, 0.05f * level, 1 );
        HPA_Utils.DrawClusters( cluster.originPosition, cluster.size, color );

        if (level > 1)
        {
            cluster.AddLesserClusters( lesserClustersArray );

        }
        return cluster;
    }
    private void FillCluster (Cluster c)
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
            Vector3 bottomCellPosition = new Vector3( i, 0, 0 ) + c.originPosition;
            Cell bottomCell = grid.GetGridObject( bottomCellPosition );

            bottomBorder.Add( bottomCell );
            for (int j = 0; j < c.size.y; j++)
            {
                Vector3 cellPosition = new Vector3( i, j, 0 ) + c.originPosition;
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
    // ////////////////////////////////////////////////////////////////////
    private void BuildEntrances (Cluster c1, Cluster c2)
    {
        Entrance entrance = new Entrance();
        Entrance symmEntrance = new Entrance();

        List<Cell> entranceCells = new List<Cell>();
        List<Cell> symmEntranceCells = new List<Cell>();

        List<Cell> C1rightBorder = c1.borders["right"];
        List<Cell> C2leftBorder = c2.borders["left"];

        for (int i = 0; i < C1rightBorder.Count; i++)
        {
            Cell cell1 = C1rightBorder[i];
            Cell cell2 = C2leftBorder[i];
            if (cell1.gridPosition.x + 1 == cell2.gridPosition.x)
            {
                AddCellsToLists( cell1, cell2 );
                AssignOrientation( Entrance.Orientation.East, Entrance.Orientation.West );
            }
        }

        List<Cell> C1topBorder = c1.borders["top"];
        List<Cell> C2bottomBorder = c2.borders["bottom"];

        for (int i = 0; i < C1topBorder.Count; i++)
        {
            Cell cell1 = C1topBorder[i];
            Cell cell2 = C2bottomBorder[i];
            if (cell1.gridPosition.y + 1 == cell2.gridPosition.y)
            {
                AddCellsToLists( cell1, cell2 );
                AssignOrientation( Entrance.Orientation.North, Entrance.Orientation.South );
            }
        }

        entrance.FillEntrance( entranceCells );
        entrance.FillSymmEntrance( symmEntranceCells );
        entrance.SetClusters( c1, c2 );

        symmEntrance.FillEntrance( symmEntranceCells );
        symmEntrance.FillSymmEntrance( entranceCells );
        symmEntrance.SetClusters( c2, c1 );

        c1.AddEntrance( entrance );
        c2.AddEntrance( symmEntrance );

        string key1 = $"{c1.originPosition}->{c2.originPosition}";
        if (!setOfEntrances.ContainsKey( key1 ))
        {
            setOfEntrances.Add( key1, entrance );
        }
        string key2 = $"{c2.originPosition}->{c1.originPosition}";
        if (!setOfEntrances.ContainsKey( key2 ))
        {
            setOfEntrances.Add( key2, symmEntrance );
        }
        void AddCellsToLists (Cell cell1, Cell cell2)
        {
            entranceCells.Add( cell1 );
            symmEntranceCells.Add( cell2 );
        }
        void AssignOrientation (Entrance.Orientation c1Orientation, Entrance.Orientation c2Orientation)
        {
            if (CheckOrientation( c1Orientation, c2Orientation ))
            {
                entrance.SetOrientation( c1Orientation, c2Orientation );
                symmEntrance.SetOrientation( c2Orientation, c1Orientation );
            }
        }
        bool CheckOrientation (Entrance.Orientation c1Orientation, Entrance.Orientation c2Orientation)
        {
            return entrance.C1Orientation != c1Orientation || entrance.C2Orientation != c2Orientation;
        }
    }
    private bool IsAdjacent (Cluster c1, Cluster c2)
    {
        Vector3Int c1Origin = GetClusterOriginGridPosition( c1 );
        Vector3Int c1End = GetClusterEndGridPosition( c1 );

        Vector3Int c2Origin = GetClusterOriginGridPosition( c2 );
        Vector3Int c2End = GetClusterEndGridPosition( c2 );

        Vector3Int c2BotRight = new Vector3Int( c2End.x, c2Origin.y, 0 );
        Vector3Int c2TopLeft = new Vector3Int( c2Origin.x, c2End.y, 0 );

        Vector3Int c1BotRight = new Vector3Int( c1End.x, c1Origin.y, 0 );
        Vector3Int c1TopLeft = new Vector3Int( c1Origin.x, c1End.y, 0 );

        if (c1Origin == c2TopLeft && c1BotRight == c2End)
        {
            return true;
        }
        else if (c1BotRight == c2Origin && c1End == c2TopLeft)
        {
            return true;
        }
        else if (c1TopLeft == c2Origin && c1End == c2BotRight)
        {
            return true;
        }
        else if (c1Origin == c2BotRight && c1TopLeft == c2End)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    // ///////////////////////////////////////////////////////////////////
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
                InstantiateNode( entrance.Value, vacant[middle] );
            }
        }
        #region buildSubGrid
        for (int i = 0; i < clustersLevel1.GetLength( 0 ); i++)
        {
            for (int j = 0; j < clustersLevel1.GetLength( 1 ); j++)
            {
                Cluster cluster = clustersLevel1[i, j];
                Grid<Cell> clusterGrid = grid.GetFractionOfGrid( cluster.originPosition, cluster.size, false );
                cluster.AddGrid( clusterGrid );
                PathFinding pathFinding = new PathFinding( clusterGrid );

            }
        }
        #endregion
        allClustersAllLevels.Add( clustersLevel1 );
    }

    private void InstantiateNode (Entrance entrance, Cell middleCell)
    {
        if (!middleCell.isWall)
        {
            Cell symmMiddle = entrance.GetSymmetricalCell( middleCell );

            if (!symmMiddle.isWall)
            {
                Cluster c1 = entrance.cluster1;
                Cluster c2 = entrance.cluster2;

                Node c1Node = NewNode( c1, middleCell );
                Node c2Node = NewNode( c2, symmMiddle );

                c1Node.pair = c2Node;
                c2Node.pair = c1Node;

                AddNode( entrance, c1Node, 1 );
                AddNode( entrance, c2Node, 1 );

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
            node.SetEntrance( entrance );
            node.SetPositions( CellNode.worldPosition );
            node.SetCell( CellNode );
            return node;
        }

    }
    private void AddNode (Entrance entrance, Node node, int Level)
    {
        node.level = Level;
        entrance.AddNode( node );
    }
    // ///////////////////////////////////////////////////////////////////
    void AddLevelToGraph (int level)
    {
        multiLevelCluster = BuildClusters( level );
        for (int i = 0; i < multiLevelCluster.GetLength( 0 ); i++)
        {
            for (int j = 0; j < multiLevelCluster.GetLength( 1 ); j++)
            {
                Cluster currentCluster = multiLevelCluster[i, j];
                Cluster nextCluster;
                #region levelUpNodes
                try
                {
                    nextCluster = multiLevelCluster[i + 1, j];
                    HandleMultiLevelClusters( level, currentCluster, nextCluster );
                    nextCluster = multiLevelCluster[i, j + 1];
                    HandleMultiLevelClusters( level, currentCluster, nextCluster );
                    if (i == j && i == 0)
                    {
                        Cluster subCluster = currentCluster.lesserLevelClusters[0];
                        LevelUpCornerSubClusters( level, currentCluster, subCluster );
                    }
                }
                catch
                {
                    try
                    {
                        nextCluster = multiLevelCluster[i + 1, j];
                        HandleMultiLevelClusters( level, currentCluster, nextCluster );
                    }
                    catch
                    {
                        try
                        {
                            if (j == 0)
                            {
                                Cluster subCluster = currentCluster.lesserLevelClusters[2];
                                LevelUpCornerSubClusters( level, currentCluster, subCluster );

                            }
                            nextCluster = multiLevelCluster[i, j + 1];
                            HandleMultiLevelClusters( level, currentCluster, nextCluster );
                        }
                        catch
                        {
                            Cluster subCluster = currentCluster.lesserLevelClusters[3];
                            LevelUpCornerSubClusters( level, currentCluster, subCluster );

                        }
                    }
                    try
                    {
                        nextCluster = multiLevelCluster[0, j + 1];
                    }
                    catch
                    {
                        if (i == 0)
                        {
                            Cluster subCluster = currentCluster.lesserLevelClusters[1];
                            LevelUpCornerSubClusters( level, currentCluster, subCluster );

                        }
                    }
                }
                #endregion
                Grid<Cell> clusterGrid = grid.GetFractionOfGrid( currentCluster.originPosition, currentCluster.size, false );
                currentCluster.AddGrid( clusterGrid );
            }

        }
        allClustersAllLevels.Add( multiLevelCluster );
        void HandleMultiLevelClusters (int level, Cluster currentCluster, Cluster nextCluster)
        {
            if (IsAdjacent( currentCluster, nextCluster ))
            {
                UpdateEntrances( currentCluster, nextCluster, level );
            }
        }
    }

    private static void LevelUpCornerSubClusters (int level, Cluster currentCluster, Cluster subCluster)
    {
        foreach (Node node in subCluster.clusterNodes)
        {
            LevelUpNode( level, node );
            currentCluster.AddNodeToCluster( node );
        }
    }

    private void UpdateEntrances (Cluster cluster, Cluster nextCluster, int level)
    {
        foreach (KeyValuePair<string, Entrance> entrance in setOfEntrances)
        {
            entrance.Value.LevelUpEntrance( cluster, nextCluster, level );
        }
        UpdateNodes( cluster, nextCluster, level );

    }


    private void UpdateNodes (Cluster cluster, Cluster nextCluster, int level)
    {
        //  Entrance entrance = cluster.entrances[0];
        foreach (Entrance entrance in cluster.entrances)
        {
            Node node = entrance.entranceNodes[0];

            if (!cluster.clusterNodes.Contains( node ))
            {
                LevelUpNode( level, node );
                cluster.AddNodeToCluster( node );
            }
            if (!nextCluster.clusterNodes.Contains( node.pair ))
            {
                LevelUpNode( level, node );
                nextCluster.AddNodeToCluster( node.pair );
            }

        }

    }

    public static void LevelUpNode (int level, Node node)
    {
        if (node != null)
        {
            if (node.level != level)
            {
                node.level = level;
                if (node.pair != null && node.pair.level != level)
                {
                    node.pair.level = level;
                }
            }
            if (node.pair != null)
            {
                //Debug.DrawLine( node.worldPosition, node.pair.worldPosition, Color.red, 10000f );
            }
        }
    }






    //Helpers
    private Vector3Int GetClusterOriginGridPosition (Cluster cluster)
    {
        return grid.GetGridPosition( cluster.originPosition );
    }
    private Vector3Int GetClusterEndGridPosition (Cluster cluster)
    {
        return grid.GetGridPosition( cluster.originPosition + new Vector3( cluster.size.x, cluster.size.y, 0 ) );
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPA : MonoBehaviour
{
    private Grid<Cell> grid;
    public int Level;
    public Vector2Int LevelOneClusterSize;
    private CreateGrid createGrid;
    private Cluster[,] clustersLevel1;
    private Cluster[,] multiLevelCluster;
    List<Entrance> setOfEntrances = new List<Entrance>();
    private void Awake ( )
    {
        createGrid = gameObject.GetComponent<CreateGrid>();
        grid = createGrid.grid;
    }
    private void Start ( )
    {
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
                Entrance entrance = BuildEntrances( c1, nextCluster );
                setOfEntrances.Add( entrance );
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
        Color color = new Color( 0.3f * level, -1f + 0.6f * level, 0.05f * level, 1 );
        DrawClusters( cluster.originPosition, cluster.size, color );

        if (level > 1)
        {
            cluster.AddLesserClusters( lesserClustersArray );

        }
        return cluster;
    }
    private void FillCluster (Cluster c)
    {
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
        c.BuildBorders( bottomBorder, leftBorder, topBorder, rightBorder );

    }
    // ////////////////////////////////////////////////////////////////////
    private Entrance BuildEntrances (Cluster c1, Cluster c2)
    {
        Entrance entrance = new Entrance();
        List<Cell> entranceCells = new List<Cell>();
        List<Cell> symmEntranceCells = new List<Cell>();

        for (int i = 0; i < c1.rightBorder.Count; i++)
        {
            Cell cell1 = c1.rightBorder[i];
            Cell cell2 = c2.leftBorder[i];
            if (cell1.gridPosition.x + 1 == cell2.gridPosition.x)
            {
                AddCellsToLists( cell1, cell2 );
                AssignOrientation( Entrance.Orientation.East, Entrance.Orientation.West );
            }
        }

        for (int i = 0; i < c1.topBorder.Count; i++)
        {
            Cell cell1 = c1.topBorder[i];
            Cell cell2 = c2.bottomBorder[i];
            if (cell1.gridPosition.y + 1 == cell2.gridPosition.y)
            {
                AddCellsToLists( cell1, cell2 );
                AssignOrientation( Entrance.Orientation.North, Entrance.Orientation.South );
            }
        }

        entrance.FillEntrance( entranceCells );
        entrance.FillSymmEntrance( symmEntranceCells );

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
            }
        }
        bool CheckOrientation (Entrance.Orientation c1Orientation, Entrance.Orientation c2Orientation)
        {
            return entrance.C1Orientation != c1Orientation || entrance.C2Orientation != c2Orientation;
        }
        entrance.SetClusters( c1, c2 );

        c1.AddEntrance( entrance );
        c2.AddEntrance( entrance );
        return entrance;
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
        foreach (Entrance entrance in setOfEntrances)
        {
            bool isNorthOrSouth = entrance.C1Orientation == Entrance.Orientation.North || entrance.C1Orientation == Entrance.Orientation.South;
            List<Cell> cells = entrance.GetEntrance();
            int x = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                Cell c = cells[i];
                if (!c.isWall && c != cells[cells.Count - 1])
                {
                    x++;
                }
                else
                {
                    int middle = ( x ) / 2;
                    if (c == cells[cells.Count - 1] && x == 0)
                    {
                        middle = cells.Count - 1;
                    }
                    if (isNorthOrSouth)
                    {
                        float entranceStart = cells[0].worldPosition.x;
                        Cell middleCell = grid.GetGridObject( new Vector3( entranceStart + middle, c.worldPosition.y, 0 ) );
                        InstantiateNode( entrance, middleCell );
                    }
                    else
                    {
                        float entranceStart = cells[0].worldPosition.y;
                        Cell middleCell = grid.GetGridObject( new Vector3( c.worldPosition.x, entranceStart + middle, 0 ) );
                        InstantiateNode( entrance, middleCell );
                    }
                }
            }
        }
        for (int i = 0; i < clustersLevel1.GetLength( 0 ); i++)
        {
            for (int j = 0; j < clustersLevel1.GetLength( 1 ); j++)
            {
                Cluster cluster = clustersLevel1[i, j];
                Grid<Cell> clusterGrid = grid.GetFractionOfGrid( cluster.originPosition, cluster.size, false );
                cluster.AddGrid( clusterGrid );
                PathFinding pathFinding = new PathFinding( clusterGrid );
                CalculateEdge( cluster, cluster.clusterNodes, pathFinding, 1 );

            }
        }
    }

    private void InstantiateNode (Entrance entrance, Cell middleCell)
    {
        if (!middleCell.isWall)
        {
            Cell symmMiddle = entrance.GetSymmetricalCell( middleCell );

            if (!symmMiddle.isWall)
            {
                Node c1Node = NewNode( entrance, middleCell );
                AddNode( entrance, c1Node, 1 );
                Node c2Node = NewNode( entrance, symmMiddle );
                AddNode( entrance, c2Node, 1 );
                c1Node.pair = c2Node;
                c2Node.pair = c1Node;
                InstantiateINTEREdge( entrance, c1Node, c2Node );
                entrance.cluster1.AddNodeToCluster( c1Node );
                entrance.cluster2.AddNodeToCluster( c2Node );
                Debug.DrawLine( c1Node.worldPosition, c2Node.worldPosition, Color.red, 10000f );
            }
        }
        Node NewNode (Entrance entrance, Cell CellNode)
        {
            Node c1Node = new Node( entrance.cluster1, entrance );
            c1Node.SetPositions( CellNode.worldPosition, CellNode.gridPosition );
            c1Node.SetCell( CellNode );
            return c1Node;
        }

        void InstantiateINTEREdge (Entrance entrance, Node c1Node, Node c2Node)
        {
            Edge edge = new Edge( c1Node, c2Node, 1, Edge.EdgeType.INTER, 1 );
            entrance.SetEdges( edge );
        }
    }
    private void AddNode (Entrance entrance, Node node, int Level)
    {
        //DrawCrossInCell( node.cell, Color.yellow );
        node.level = Level;
        entrance.SetNodes( node );
    }
    private void CalculateEdge (Cluster cluster, List<Node> nodes, PathFinding pathFinding, int level)
    {
        List<Cell> path = null;
        Edge edge = null;
        for (int x = 0; x <= nodes.Count - 1; x++)
        {
            Node node1 = nodes[x];

            for (int y = 0; y <= nodes.Count - 1; y++)
            {
                Node node2 = nodes[y];
                if (node1 == node2)
                {
                    continue;
                }
                if (cluster.IsNodeInside( node1 ))
                {
                    if (cluster.IsNodeInside( node2 ))
                    {
                        try
                        {
                            if (level == 1)
                            {
                                path = pathFinding.FindPath( node1.worldPosition, node2.worldPosition );
                                edge = new Edge( node1, node2, level, Edge.EdgeType.INTRA, path.Count );
                            }
                            if (level > 1)
                            {
                                List<Cluster> lesserClusters = cluster.lesserLevelClusters;
                                foreach (Cluster subCluster in lesserClusters)
                                {
                                    foreach (Cluster nextSubCluster in lesserClusters)
                                    {
                                        if (subCluster == nextSubCluster)
                                        {
                                            continue;
                                        }
                                        if (subCluster.IsNodeInside( node1 ))
                                        {
                                            if (nextSubCluster.IsNodeInside( node2 ))
                                            {
                                                path = pathFinding.FindPath( node1.worldPosition, node2.worldPosition );
                                                edge = new Edge( node1, node2, level, Edge.EdgeType.INTRA );

                                                subCluster.AddEdge( edge );
                                            }
                                        }

                                    }
                                }

                                if (level == Level)
                                {

                                    ShowPathLines( path );
                                }
                                //Debug.DrawLine( node1.worldPosition, node2.worldPosition, Color.red, 10000f );
                            }
                        }
                        catch (System.Exception)
                        {
                            throw;
                        }
                    }
                }
            }
        }

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

                try
                {
                    nextCluster = multiLevelCluster[i + 1, j];
                    HandleMultiLevelClusters( level, currentCluster, nextCluster );
                    nextCluster = multiLevelCluster[i, j + 1];
                    HandleMultiLevelClusters( level, currentCluster, nextCluster );
                    if (i == j && i == 0)
                    {
                        Cluster subCluster = currentCluster.lesserLevelClusters[0];
                        Node node = subCluster.clusterNodes[0];
                        LevelUpNode( level, node );
                        currentCluster.AddNodeToCluster( node );

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
                                Node node = subCluster.clusterNodes[0];
                                LevelUpNode( level, node );
                                currentCluster.AddNodeToCluster( node );

                            }
                            nextCluster = multiLevelCluster[i, j + 1];
                            HandleMultiLevelClusters( level, currentCluster, nextCluster );
                        }
                        catch
                        {
                            Cluster subCluster = currentCluster.lesserLevelClusters[3];
                            Node node = subCluster.clusterNodes[0];
                            LevelUpNode( level, node );
                            currentCluster.AddNodeToCluster( node );

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
                            Node node = subCluster.clusterNodes[0];
                            LevelUpNode( level, node );
                            currentCluster.AddNodeToCluster( node );

                        }
                    }
                }
                Grid<Cell> clusterGrid = grid.GetFractionOfGrid( currentCluster.originPosition, currentCluster.size, false );
                currentCluster.AddGrid( clusterGrid );
                PathFinding pathFinding = new PathFinding( clusterGrid );
                CalculateEdge( currentCluster, currentCluster.clusterNodes, pathFinding, level );

            }
        }

        void HandleMultiLevelClusters (int level, Cluster currentCluster, Cluster nextCluster)
        {
            if (IsAdjacent( currentCluster, nextCluster ))
            {
                UpdateEntrances( currentCluster, nextCluster, level );
            }
        }
    }

    private void UpdateEntrances (Cluster cluster, Cluster nextCluster, int level)
    {
        foreach (Entrance entrance in setOfEntrances)
        {
            entrance.LevelUpEntrance( cluster, nextCluster );
            //if (cluster.IsEntranceInside( entrance.entranceTiles ) && nextCluster.IsEntranceInside( entrance.symmEntranceTiles ))
            //{
            //cluster.AddEntrance( entrance );
            //   UpdateEdges( level, entrance );

        }
        UpdateNodes( cluster, nextCluster, level );

    }


    private void UpdateNodes (Cluster cluster, Cluster nextCluster, int level)
    {
        //  Entrance entrance = cluster.entrances[0];
        foreach (Entrance entrance in cluster.entrances)
        {
            foreach (Cell c in entrance.entranceTiles)
            {
                DrawCrossInCell( c, Color.blue );
            }
            //foreach (Node node1 in entrance.entranceNodes)
            //{
            //    DrawCrossInCell( node1.cell, Color.yellow );
            //}
            Node node = entrance.entranceNodes[0];
            DrawCrossInCell( node.cell, Color.yellow );
            //foreach (Node node in entrance.entranceNodes)
            //{
            //foreach subcluster, i'm going to keep only 1 node
            if (!cluster.clusterNodes.Contains( node ))
            {
                LevelUpNode( level, node );
                cluster.AddNodeToCluster( node );
                // DrawCrossInCell( node.cell, Color.yellow );

            }
            if (!nextCluster.clusterNodes.Contains( node.pair ))
            {
                LevelUpNode( level, node );
                nextCluster.AddNodeToCluster( node.pair );
                //  DrawCrossInCell( node.pair.cell, Color.yellow );
            }
        }

    }

    private void LevelUpNode (int level, Node node)
    {
        if (node.level != level)
        {
            node.level = level;
            if (node.pair.level != level)
            {
                node.pair.level = level;
            }
        }
    }

    private void UpdateEdges (int level, Entrance entrance)
    {
        foreach (Edge edge in entrance.entranceEdges)
        {
            if (edge.Level != level)
            {
                edge.Level = level;
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
    private void DrawCrossInCell (Cell c, Color color)
    {
        float crossLength = 0.2f;
        Vector3 crossX = new Vector3( crossLength, 0, 0 );
        Vector3 crossY = new Vector3( 0, crossLength, 0 );
        Debug.DrawLine( c.worldPosition - crossX, c.worldPosition + crossX, color, 100000f );
        Debug.DrawLine( c.worldPosition - crossY, c.worldPosition + crossY, color, 100000f );
    }
    private void DrawClusters (Vector3 position, float Width, float Height, Color color)
    {
        ///drawing Cluster border///
        Debug.DrawLine( position, new Vector3( position.x, position.y + Height ), color, 10000f, false );
        Debug.DrawLine( position, new Vector3( position.x + Width, position.y ), color, 10000f, false );
        ///drawing Cluster border///
    }
    private void DrawClusters (Vector3 position, Vector2Int size, Color color)
    {
        DrawClusters( position, size.x, size.y, color );
    }
    private void ShowPathLines (List<Cell> path)
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3Int currentCellPosition = path[i].gridPosition;
                int pathX = currentCellPosition.x;
                int pathY = currentCellPosition.y;
                Vector3 position = grid.GetWorldPosition( pathX, pathY );

                Vector3Int nextCellPosition = path[i + 1].gridPosition;
                int nextPathX = nextCellPosition.x;
                int nextPathY = nextCellPosition.y;
                Vector3 positionNext = grid.GetWorldPosition( nextPathX, nextPathY );

                Vector3 centerCell = Vector3.one * 0.5f * grid.cellSize;
                Debug.DrawLine( position + centerCell, positionNext + centerCell, Color.red, 100f );
            }
        }
    }
}


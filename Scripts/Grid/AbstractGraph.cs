using System.Collections;
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

  private void Awake()
  {
    createGrid = gameObject.GetComponent<CreateGrid>();
  }
  private void Start()
  {
    grid = createGrid.grid;
    PreProcessing(Level);
  }

  void PreProcessing(int maxLevel)
  {
    AbstractMaze();
    BuildGraph();
    for (int l = 2; l <= maxLevel; l++)
    {
      AddLevelToGraph(l);
    }
  }
  void AbstractMaze()
  {
    clustersLevel1 = BuildClusters();
    for (int i = 0; i < clustersLevel1.GetLength(0); i++)
    {
      for (int j = 0; j < clustersLevel1.GetLength(1); j++)
      {
        Cluster c1 = clustersLevel1[i, j];

        for (int l = -1; l <= 1; l++)
        {
          for (int m = -1; m <= 1; m++)
          {
            //checks only up, down, left and right
            if ((l == 0 && m == 0) || (l == 1 && m == 1) || (l == -1 && m == -1) || (l == -1 && m == 1) || (l == 1 && m == -1))
            {
              continue;
            }
            if ((i + l < 0) || (i + l) >= clustersLevel1.GetLength(0))
            {
              continue;
            }
            if ((j + m < 0) || (j + m) >= clustersLevel1.GetLength(1))
            {
              continue;
            }
            Cluster nextCluster = clustersLevel1[i + l, j + m];
            HandlePairOfClusters(c1, nextCluster);
          }
        }

      }
    }

    void HandlePairOfClusters(Cluster c1, Cluster nextCluster)
    {
      KeyValuePair<string, List<Cell>>[] border = GetAdjacentBorder(c1, nextCluster);

      BuildEntrances(c1, nextCluster, border);

    }

  }
  public Cluster[,] BuildClusters(int level = 1)
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
        Cluster cluster = InstantiateCluster(level, x, y, lesserClustersArray);
        setOfClusters[x, y] = cluster;
        FillCluster(cluster);
      }
    }
    clustersLevel1 = setOfClusters;
    return setOfClusters;
  }
  private Cluster InstantiateCluster(int level, int x, int y, Cluster[,] lesserClustersArray)
  {
    int levelAdjustmentX = (int)(LevelOneClusterSize.x * Mathf.Pow(2, level - 1));
    int levelAdjustmentY = (int)(LevelOneClusterSize.y * Mathf.Pow(2, level - 1));
    Vector2Int size = new Vector2Int(levelAdjustmentX, levelAdjustmentY);
    Vector3 clusterPosition = grid.GetWorldPosition(x * size.x, y * size.y);

    Cluster cluster = new Cluster(size, clusterPosition, level);

    cluster.SetGridPosition(grid.GetGridPosition(clusterPosition));
    Color color = new Color(0.3f * level, -1f + 0.7f * level, 0.05f * level, 1);
    HPA_Utils.DrawClusters(cluster.originPosition, cluster.size, color);

    if (level > 1)
    {
      cluster.AddLesserClusters(lesserClustersArray);

    }
    return cluster;
  }
  private void FillCluster(Cluster c)
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
      Vector3 bottomCellPosition = new Vector3(i, 0, 0) + c.originPosition;
      Cell bottomCell = grid.GetGridObject(bottomCellPosition);

      bottomBorder.Add(bottomCell);
      for (int j = 0; j < c.size.y; j++)
      {
        Vector3 cellPosition = new Vector3(i, j, 0) + c.originPosition;
        Cell cell = grid.GetGridObject(cellPosition);
        if (j == c.size.y - 1)
        {
          topBorder.Add(cell);
        }
        if (i == c.size.x - 1)
        {
          rightBorder.Add(cell);
        }
        if (i == 0)
        {
          leftBorder.Add(cell);
        }
      }
    }
    c.borders.Add(bottom, bottomBorder);
    c.borders.Add(left, leftBorder);
    c.borders.Add(right, rightBorder);
    c.borders.Add(top, topBorder);

  }

  private void BuildEntrances(Cluster c1, Cluster c2, KeyValuePair<string, List<Cell>>[] AdjacentBorder)
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
      AddCellsToLists(cell1, cell2);
    }

    if (entranceCells.Count > 0 && symmEntranceCells.Count > 0)
    {
      entrance.FillEntrance(entranceCells);
      entrance.FillSymmEntrance(symmEntranceCells);
      entrance.SetClusters(c1, c2);
    }
    else
    {
      return;
    }

    c1.AddEntrance(entrance);

    string key1 = $"{c1.originPosition}->{c2.originPosition}";
    setOfEntrances.Add(key1, entrance);

    void AddCellsToLists(Cell cell1, Cell cell2)
    {
      entranceCells.Add(cell1);
      symmEntranceCells.Add(cell2);
    }
  }
  private KeyValuePair<string, List<Cell>>[] GetAdjacentBorder(Cluster c1, Cluster c2)
  {
    Vector3Int c1Origin = c1.GridPosition; //BottomLeft
    Vector3Int c1End = GetClusterEndGridPosition(c1); //UpperRight

    Vector3Int c2Origin = c2.GridPosition; //BottomLeft
    Vector3Int c2End = GetClusterEndGridPosition(c2); //UpperRight

    Vector3Int c2BotRight = new Vector3Int(c2End.x, c2Origin.y, 0);
    Vector3Int c2TopLeft = new Vector3Int(c2Origin.x, c2End.y, 0);

    Vector3Int c1BotRight = new Vector3Int(c1End.x, c1Origin.y, 0);
    Vector3Int c1TopLeft = new Vector3Int(c1Origin.x, c1End.y, 0);

    if (c1Origin == c2TopLeft && c1BotRight == c2End)
    {
      KeyValuePair<string, List<Cell>> bottom = new KeyValuePair<string, List<Cell>>("bottom", c1.borders["bottom"]);
      KeyValuePair<string, List<Cell>> top = new KeyValuePair<string, List<Cell>>("top", c2.borders["bottom"]);

      return new KeyValuePair<string, List<Cell>>[] { bottom, top };
    }
    else if (c1BotRight == c2Origin && c1End == c2TopLeft)
    {
      KeyValuePair<string, List<Cell>> right = new KeyValuePair<string, List<Cell>>("right", c1.borders["right"]);
      KeyValuePair<string, List<Cell>> left = new KeyValuePair<string, List<Cell>>("left", c2.borders["left"]);

      return new KeyValuePair<string, List<Cell>>[] { right, left };
    }
    else if (c1TopLeft == c2Origin && c1End == c2BotRight)
    {
      KeyValuePair<string, List<Cell>> top = new KeyValuePair<string, List<Cell>>("top", c1.borders["top"]);
      KeyValuePair<string, List<Cell>> bottom = new KeyValuePair<string, List<Cell>>("bottom", c2.borders["bottom"]);

      return new KeyValuePair<string, List<Cell>>[] { top, bottom };
    }
    else if (c1Origin == c2BotRight && c1TopLeft == c2End)
    {
      KeyValuePair<string, List<Cell>> left = new KeyValuePair<string, List<Cell>>("left", c1.borders["left"]);
      KeyValuePair<string, List<Cell>> right = new KeyValuePair<string, List<Cell>>("right", c2.borders["right"]);

      return new KeyValuePair<string, List<Cell>>[] { left, right };
    }
    return default;

    Vector3Int GetClusterEndGridPosition(Cluster cluster)
    {
      return grid.GetGridPosition(cluster.originPosition + new Vector3(cluster.size.x, cluster.size.y, 0));
    }
  }
  void BuildGraph()
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
          vacant.Add(c);
        }
        else
        {
          full.Add(c);
          if (vacant.Count >= 1)
          {
            middle = Mathf.FloorToInt(vacant.Count * 0.5f);
            InstantiateNode(entrance.Value, vacant[middle]);
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
        middle = Mathf.FloorToInt(vacant.Count * 0.5f);
        InstantiateNode(entrance.Value, vacant[middle]);
      }
    }
    #region buildSubGrid
    for (int i = 0; i < clustersLevel1.GetLength(0); i++)
    {
      for (int j = 0; j < clustersLevel1.GetLength(1); j++)
      {
        Cluster cluster = clustersLevel1[i, j];
        Grid<Cell> clusterGrid = grid.GetFractionOfGrid(cluster.originPosition, cluster.size, false);
        cluster.AddGrid(clusterGrid);
        PathFinding pathFinding = new PathFinding(clusterGrid);

      }
    }
    #endregion
    allClustersAllLevels.Add(clustersLevel1);
  }

  private void InstantiateNode(Entrance entrance, Cell middleCell)
  {
    if (!middleCell.isWall)
    {
      Cell symmMiddle = entrance.GetSymmetricalCell(middleCell);

      if (!symmMiddle.isWall)
      {
        Cluster c1 = entrance.cluster1;
        Cluster c2 = entrance.cluster2;

        Node c1Node = NewNode(c1, middleCell);
        Node c2Node = NewNode(c2, symmMiddle);

        c1Node.pair = c2Node;
        c2Node.pair = c1Node;

        AddNode(entrance, c1Node, 1);
        AddNode(entrance, c2Node, 1);

        c1.AddNodeToCluster(c1Node);
        c2.AddNodeToCluster(c2Node);
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
    Node NewNode(Cluster cluster, Cell CellNode)
    {
      Node node = new Node(cluster);
      node.SetEntrance(entrance);
      node.SetPositions(CellNode.worldPosition);
      node.SetCell(CellNode);
      return node;
    }

  }
  private void AddNode(Entrance entrance, Node node, int Level)
  {
    node.level = Level;
    entrance.AddNode(node);
  }

  void AddLevelToGraph(int level)
  {
    multiLevelCluster = BuildClusters(level);
    for (int i = 0; i < multiLevelCluster.GetLength(0); i++)
    {
      for (int j = 0; j < multiLevelCluster.GetLength(1); j++)
      {
        Cluster currentCluster = multiLevelCluster[i, j];

        UpdateNeighbours(currentCluster, level, i, j);

        Grid<Cell> clusterGrid = grid.GetFractionOfGrid(currentCluster.originPosition, currentCluster.size, false);
        currentCluster.AddGrid(clusterGrid);
      }

    }
    allClustersAllLevels.Add(multiLevelCluster);

    void HandleMultiLevelClusters(int level, Cluster currentCluster, Cluster nextCluster)
    {
      if (!GetAdjacentBorder(currentCluster, nextCluster).Equals(default(KeyValuePair<string, List<string>>)))
      {
        UpdateEntrances(currentCluster, nextCluster, level);
      }
    }
    void UpdateNeighbours(Cluster currentCluster, int level, int i, int j)
    {

      for (int l = -1; l <= 1; l++)
      {
        for (int m = -1; m <= 1; m++)
        {
          //checks only up, down, left and right
          if ((l == 0 && m == 0) || (l == 1 && m == 1) || (l == -1 && m == -1) || (l == -1 && m == 1) || (l == 1 && m == -1))
          {
            continue;
          }
          if ((i + l < 0) || (i + l) >= multiLevelCluster.GetLength(0))
          {
            continue;
          }
          if ((j + m < 0) || (j + m) >= multiLevelCluster.GetLength(1))
          {
            continue;
          }
          Cluster nextCluster = multiLevelCluster[i + l, j + m];
          HandleMultiLevelClusters(level, currentCluster, nextCluster);
        }
      }
    }
  }

  private void UpdateEntrances(Cluster cluster, Cluster nextCluster, int level)
  {
    Entrance leveledEntrance = new Entrance();
    foreach (Cluster subCluster in cluster.lesserLevelClusters)
    {
      foreach (var entrance in subCluster.entrances)
      {
        foreach (Node n in entrance.Value.entranceNodes)
        {
          if (nextCluster.IsPositionInside(n.pair.worldPosition))
          {
            leveledEntrance.AddNode(n);
          }
        }
      }
    }
    if (leveledEntrance.entranceNodes.Count > 0)
    {
      leveledEntrance.cluster1 = cluster;
      leveledEntrance.cluster2 = nextCluster;
    }

    foreach (KeyValuePair<string, Entrance> entrance in setOfEntrances)
    {

      if (entrance.Value.IsLeveling(cluster, nextCluster))
      {
        Entrance newEntrance = entrance.Value;
        newEntrance.SetClusters(cluster, nextCluster);
      }
    }
    UpdateNodes(cluster, nextCluster, level);

  }


  private void UpdateNodes(Cluster cluster, Cluster nextCluster, int level)
  {
    //  Entrance entrance = cluster.entrances[0];
    foreach (var entrance in cluster.entrances)
    {
      Node node = entrance.Value.entranceNodes[0];

      if (!cluster.clusterNodes.Contains(node))
      {
        LevelUpNode(level, node);
        cluster.AddNodeToCluster(node);
      }
      if (!nextCluster.clusterNodes.Contains(node.pair))
      {
        LevelUpNode(level, node);
        nextCluster.AddNodeToCluster(node.pair);
      }

    }

  }

  public static void LevelUpNode(int level, Node node)
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

}


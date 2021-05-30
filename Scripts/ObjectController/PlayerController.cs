using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  //this script sends the player position and mouse position, when clicked, as eventArg to whoever gets
  //subscribed: Hierarchical_Pathfinding.cs
  private CreateGrid createGrid;
  public Grid<Cell> grid;
  public class PlayerPositions
  {
    public Node startNode;
    public Node endNode;
    public PlayerPositions(Node startNode, Node endNode)
    {
      this.startNode = startNode;
      this.endNode = endNode;
    }
  }
  public delegate void SendPositions(object sender, PlayerPositions positions);
  public event SendPositions OnPlayerDestinationSet;
  private void Awake()
  {
    createGrid = GameObject.FindGameObjectWithTag("Grid").GetComponent<CreateGrid>();

  }
  private void Start()
  {
    grid = createGrid.grid;
  }


  private void Update()
  {
    SetNodes();
  }

  private void SetNodes()
  {
    if (Input.GetMouseButtonDown(0))
    {
      Vector3 mousePosition = Utils.GetMousePosition();
      Vector3 currentPos = GetGridWorldPosition(gameObject.transform.position);
      Vector3 destinationPos = GetGridWorldPosition(mousePosition);
      Node startNode = NewNode(currentPos);
      Node endNode = NewNode(destinationPos);
      PlayerPositions pos = new PlayerPositions(startNode, endNode);

      OnPlayerDestinationSet?.Invoke(this, pos);
    }
  }
  private Node NewNode(Vector3 position)
  {
    Node n = new Node();
    n.worldPosition = position + new Vector3(0.5f, 0.5f, 0);
    return n;
  }
  private Vector3 GetGridWorldPosition(Vector3 position)
  {
    if (grid != null)
    {
      Cell cell = grid.GetGridObject(position);
      return cell.worldPosition;
    }
    return new Vector3(-1, -1, -1);
  }
}

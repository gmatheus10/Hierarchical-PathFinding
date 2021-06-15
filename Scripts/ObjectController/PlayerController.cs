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
        public PlayerPositions (Node startNode, Node endNode)
        {
            this.startNode = startNode;
            this.endNode = endNode;
        }
    }
    public delegate void SendPositions (object sender, PlayerPositions positions);
    public event SendPositions OnPlayerDestinationSet;
    private void Awake ( )
    {
        createGrid = GameObject.FindGameObjectWithTag( "Grid" ).GetComponent<CreateGrid>();

    }
    private void Start ( )
    {
        grid = createGrid.grid;
    }


    private void Update ( )
    {
        SetNodes();
    }

    private void SetNodes ( )
    {
        if (Input.GetMouseButtonDown( 0 ))
        {
            Vector3 mousePosition = Utils.GetMousePosition();

            Node startNode = PositionToNode( gameObject.transform.position );
            Node endNode = PositionToNode( mousePosition );

            PlayerPositions pos = new PlayerPositions( startNode, endNode );

            OnPlayerDestinationSet?.Invoke( this, pos );

            // HPA_Utils.DrawCrossInPosition( startNode.WorldPosition, Color.green );
            // HPA_Utils.DrawCrossInPosition( endNode.WorldPosition, Color.green );
        }
    }

    private Node PositionToNode (Vector3 position)
    {
        if (grid != null)
        {
            Cell cell = grid.GetGridObject( position );
            Node n = NewNode( cell.worldPosition );
            n.SetGridPosition( cell.gridPosition );
            return n;
        }
        return null;
    }
    private Node NewNode (Vector3 position)
    {
        Node n = new Node();
        n.SetPosition( position );
        return n;
    }
}

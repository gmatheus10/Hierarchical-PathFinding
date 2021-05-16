using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //will  this script be on the player or on the mouse object?
    //This script needs to create a node with the mouse position
    //This script needs to create a node with the player position
    //and send them to the Hierarchical_Pahtfinding 
    private CreateGrid createGrid;
    public Grid<Cell> grid;
    public class PlayerPositions
    {
        public Vector3 currentPos;
        public Vector3 destinationPos;
        public PlayerPositions (Vector3 currentPos, Vector3 destinationPos)
        {
            this.currentPos = currentPos;
            this.destinationPos = destinationPos;
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
            Vector3 currentPos = GetGridWorldPosition( gameObject.transform.position );
            Vector3 destinationPos = GetGridWorldPosition( mousePosition );
            PlayerPositions pos = new PlayerPositions( currentPos, destinationPos );
            OnPlayerDestinationSet?.Invoke( this, pos );
            //the other script will check if this is inside the grid;
        }
    }
    //need to send it to the other script in the moment we press the mouse button
    private Vector3 GetGridWorldPosition (Vector3 position)
    {
        if (grid != null)
        {
            Cell cell = grid.GetGridObject( position );
            return cell.worldPosition;
        }
        return new Vector3( -1, -1, -1 );
    }
}

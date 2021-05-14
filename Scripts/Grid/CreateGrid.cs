using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGrid : MonoBehaviour
{
    public int cellSize;
    public Grid<Cell> grid;
    public Vector2 gridSize;
    // Start is called before the first frame update
    void Awake ( )
    {
        InstatiateNewGrid();
    }

    public delegate void SendGrid (Grid<Cell> grid);
    public event SendGrid OnGridCreation;
    private void InstatiateNewGrid ( )
    {
        int gridSizeX = Mathf.RoundToInt( gridSize.x );
        int gridSizeY = Mathf.RoundToInt( gridSize.y );
        Vector3 originPosition = gameObject.transform.position;
        grid = new Grid<Cell>( gridSizeX, gridSizeY, cellSize, originPosition, InstatiateCell, true );
        OnGridCreation?.Invoke( grid );
    }

    private Cell InstatiateCell (Grid<Cell> g, int x, int y)
    {
        return new Cell( g, x, y );
    }

    private void OnDrawGizmos ( )
    {
        Vector3 originPosition = gameObject.transform.position;
        Vector3 LEFT_UP = originPosition + new Vector3( 0, gridSize.y );
        Vector3 RIGHT_UP = originPosition + new Vector3( gridSize.x, gridSize.y );
        Vector3 RIGHT_DOWN = originPosition + new Vector3( gridSize.x, 0 );

        Gizmos.DrawLine( originPosition, LEFT_UP );
        Gizmos.DrawLine( originPosition, RIGHT_DOWN );
        Gizmos.DrawLine( RIGHT_DOWN, RIGHT_UP );
        Gizmos.DrawLine( LEFT_UP, RIGHT_UP );

    }
    public event EventHandler<OnGridEventArgs> OnGridEnter;
    public class OnGridEventArgs : EventArgs
    {
        public Grid<Cell> grid;
        public GameObject gridContainer;
        public OnGridEventArgs (Grid<Cell> grid, GameObject gridContainer)
        {
            this.grid = grid;
            this.gridContainer = gridContainer;
        }
    }
    private void OnTriggerStay2D (Collider2D collision)
    {
        OnGridEnter?.Invoke( this, new OnGridEventArgs( grid, gameObject ) );
    }
}

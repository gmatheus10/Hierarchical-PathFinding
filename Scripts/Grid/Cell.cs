using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int gCost = 0;
    public int hCost = 0;
    private int fCost = 0;
    public int FCost { get { return fCost; } }

    public Vector3 worldPosition;
    public Vector3Int gridPosition;
    public bool isWall = false;

    public Cell cameFromNode;

    private readonly Grid<Cell> grid;
    public Cell (Grid<Cell> grid, int x, int y)
    {
        this.grid = grid;
        Vector3 centerTranslocation = new Vector3( grid.cellSize, grid.cellSize ) * 0.5f;
        worldPosition = grid.GetWorldPosition( x, y ) + centerTranslocation;

        gridPosition = new Vector3Int( x, y, 0 );
        isWall = CheckIfCellIsWall( x, y );
    }
    public override string ToString ( )
    {
        return ( $"{gridPosition.x} - {gridPosition.y}" );
    }
    public void CalculateF ( )
    {
        fCost = gCost + hCost;
    }

    private bool CheckIfCellIsWall (int x, int y)
    {

        if (Physics2D.OverlapCircle( worldPosition, grid.cellSize * 0.2f, LayerMask.GetMask( "Walls" ) ) != null)
        {
            // Debug.DrawLine( grid.GetWorldPosition( x, y ), grid.GetWorldPosition( x + 1, y + 1 ), Color.red, 100f );
            return true;
        }
        else return false;
    }
}

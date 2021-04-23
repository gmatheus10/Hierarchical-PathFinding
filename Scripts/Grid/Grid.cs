using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Grid<TGridObject>
{
    public bool showGrid;
    private int width;
    public int Width { get { return width; } }
    private int height;
    public int Height { get { return height; } }
    private Vector3 gridSize;
    public TGridObject[,] gridArray { get; }

    public float cellSize;
    private Vector3 originPosition;
    public Vector3 OriginPosition { get { return originPosition; } }
    private Vector3 finalPosition;
    public Vector3 FinalPosition { get { return finalPosition; } }
    public TextMesh[,] cellText;

    public Grid (int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject, bool showGrid)
    {
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        this.width = width;
        this.height = height;
        gridArray = new TGridObject[width, height];
        cellText = new TextMesh[width, height];

        finalPosition = GetWorldPosition( width, height );
        gridSize = new Vector3( width, height ) / cellSize;


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TGridObject gridObject = createGridObject( this, x, y );

                SetGridObject( x, y, gridObject );
                if (showGrid)
                {

                    cellText[x, y] = WorldText.CreateWorldText( null, gridArray[x, y]?.ToString(), GetWorldPosition( x, y ) + new Vector3( cellSize, cellSize ) * .5f, 28, Color.white, TextAnchor.MiddleCenter, TextAlignment.Center, LayerMask.NameToLayer( "Grid" ), 20 );

                }

            }
        }
        DrawGrid( width, height, showGrid );
    }
    public Grid (Vector2Int size, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject, bool showGrid) : this( size.x, size.y, cellSize, originPosition, createGridObject, showGrid )
    {

    }
    private void DrawGrid (int width, int height, bool showGrid)
    {
        if (showGrid)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Debug.DrawLine( GetWorldPosition( x, y ), GetWorldPosition( x, y + 1 ), Color.white, 10000f, true );
                    Debug.DrawLine( GetWorldPosition( x, y ), GetWorldPosition( x + 1, y ), Color.white, 10000f, true );

                }
            }
            Debug.DrawLine( GetWorldPosition( 0, height ), GetWorldPosition( width, height ), Color.white, 10000f, true );
            Debug.DrawLine( GetWorldPosition( width, 0 ), GetWorldPosition( width, height ), Color.white, 10000f, true );
        }
    }
    public Vector3 GetWorldPosition (int x, int y)
    {
        return new Vector3( x, y ) * cellSize + originPosition;
    }
    public Vector3Int GetGridPosition (Vector3 position)
    {
        float porcX;
        float porcY;

        Vector3 positionOnGrid = position - originPosition;
        //Debug.Log( "Position on Grid:" + " " + positionOnGrid );
        porcX = ( Mathf.Clamp01( positionOnGrid.x / width ) );
        porcY = ( Mathf.Clamp01( positionOnGrid.y / height ) );
        int gridX = Mathf.FloorToInt( porcX * gridSize.x );
        int gridY = Mathf.FloorToInt( porcY * gridSize.y );
        //Debug.Log( $"{gridX} / {gridY}" );
        return new Vector3Int( gridX, gridY, 0 );
    }
    public Vector3Int GetGridPosition (int x, int y)
    {
        return GetGridPosition( new Vector3( x, y, 0 ) );
    }
    public TGridObject GetGridObject (Vector3Int gridPosition)
    {
        try
        {
            return gridArray[gridPosition.x, gridPosition.y];
        }
        catch (Exception)
        {
            Debug.Log( $"Couldn't return Grid Object at grid position: {gridPosition}" );
            return default( TGridObject );
        }
    }
    public TGridObject GetGridObject (Vector3 position)
    {
        Vector3Int gridObject = GetGridPosition( position );
        return GetGridObject( gridObject.x, gridObject.y );
    }
    public TGridObject GetGridObject (int x, int y)
    {
        try
        {
            return gridArray[x, y];
        }
        catch (Exception)
        {
            Debug.Log( $"Couldn't return Grid Object at grid position: {x} - {y}" );
            return default( TGridObject );
        }
    }
    public void SetGridObject (int x, int y, TGridObject value)
    {
        gridArray[x, y] = value;

    }
    public void SetGridObject (Vector3 worldPosition, TGridObject value)
    {
        Vector3Int gridPositions = GetGridPosition( worldPosition );
        SetGridObject( gridPositions.x, gridPositions.y, value );
    }
    public bool IsInsideGrid (int x, int y)
    {
        return ( x >= 0 && y >= 0 && x < width && y < height );
    }
    public bool IsInsideGrid (Vector3 position)
    {
        bool greaterX = position.x >= originPosition.x;
        bool smallerX = position.x < finalPosition.x;
        bool greaterY = position.y >= originPosition.y;
        bool smallerY = position.y < finalPosition.y;
        bool isInside = greaterX && smallerX && greaterY && smallerY;
        return isInside;
    }
    public Grid<TGridObject> GetFractionOfGrid (Vector3 startPosition, Vector2Int size, bool showGrid)
    {
        Vector3Int fracStart = GetGridPosition( startPosition );
        Grid<TGridObject> frac = new Grid<TGridObject>( size, this.cellSize, startPosition, (Grid<TGridObject> g, int x, int y) => gridArray[x + fracStart.x, y + fracStart.y], showGrid );

        return frac;
    }
}
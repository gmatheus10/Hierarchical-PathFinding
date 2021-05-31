using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Utils
{
    private readonly static int MOVE_DIAGONAL_COST = 14;
    private readonly static int MOVE_STRAIGHT_COST = 10;
    public static Vector3 GetMousePosition ( )
    {
        return Camera.main.ScreenToWorldPoint( Input.mousePosition ) + new Vector3( 0, 0, 10 );
    }

    public static float NodeDistance (Node a, Node b)
    {
        return ( a.WorldPosition - b.WorldPosition ).magnitude;
    }
    public static int ManhatamDistance (Vector3Int a, Vector3Int b)
    {
        int xDistance = Mathf.Abs( a.x - b.x );
        int yDistance = Mathf.Abs( a.y - b.y );
        int remaining = Mathf.Abs( xDistance - yDistance );
        return Mathf.RoundToInt( MOVE_DIAGONAL_COST * Mathf.Min( xDistance, yDistance ) + MOVE_STRAIGHT_COST * remaining );
    }
    public static int ManhatamDistance (Cell a, Cell b)
    {
        return ManhatamDistance( a.gridPosition, b.gridPosition );
    }
    public static int ManhatamDistance (Cluster a, Cluster b)
    {
        float xDistance = Mathf.Abs( a.OriginPosition.x - b.OriginPosition.x );
        float yDistance = Mathf.Abs( a.OriginPosition.y - b.OriginPosition.y );
        return Mathf.RoundToInt( xDistance + yDistance );
    }
}

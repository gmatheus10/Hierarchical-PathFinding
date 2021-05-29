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
    public static int ManhatamDistance (Cell a, Cell b)
    {
        int xDistance = Mathf.Abs( a.gridPosition.x - b.gridPosition.x );
        int yDistance = Mathf.Abs( a.gridPosition.y - b.gridPosition.y );
        int remaining = Mathf.Abs( xDistance - yDistance );
        return Mathf.RoundToInt( MOVE_DIAGONAL_COST * Mathf.Min( xDistance, yDistance ) + MOVE_STRAIGHT_COST * remaining );
    }
    public static int ManhatamDistance (Cluster a, Cluster b)
    {
        float xDistance = Mathf.Abs( a.originPosition.x - b.originPosition.x );
        float yDistance = Mathf.Abs( a.originPosition.y - b.originPosition.y );
        return Mathf.RoundToInt( xDistance + yDistance );
    }
}

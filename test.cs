using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    private BoxCollider2D bounds;
    private PathFinding pathFinding;
    private LayerMask ignoreWalls;
    public float cellSize = 1;
    private Grid<Cell> grid;
    private GameObject lastSelected;
    private List<Cell> lastPath;
    public float objectsSpeed = 1f;
    void Start ( )
    {
        ignoreWalls = LayerMask.GetMask( "Walls", "Grid" );
        lastSelected = gameObject;
        bounds = gameObject.GetComponent<BoxCollider2D>();
        // pathFinding = new PathFinding( bounds, cellSize, gameObject.transform.position );
        //grid = pathFinding.GetGrid();
        for (int i = 0; i < GetObjectsInsideBounds().Length; i++)
        {
            //pathFinding.SnapToGrid( GetObjectsInsideBounds()[i].gameObject );
        }
    }

    // UPDATE ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Update ( )
    {

        //SelectObject();
        //PathFind( lastSelected );
        //MoveObjectOnPath( lastSelected, lastPath );

    }
    private Collider2D[] GetObjectsInsideBounds ( )
    {

        return Physics2D.OverlapBoxAll( gameObject.transform.position + new Vector3( bounds.size.x, bounds.size.y, 0 ) * 0.5f, bounds.size, 0, ~ignoreWalls );
    }
    //private GameObject SelectObject ( )
    //{
    //    if (Input.GetMouseButtonDown( 0 ))
    //    {

    //        GameObject @object = Physics2D.OverlapCircle( GetMousePosition(), grid.cellSize * 0.1f, ~ignoreWalls ).gameObject;
    //        lastSelected = @object;

    //        Debug.Log( lastSelected );
    //    }
    //    return lastSelected;
    //}
    //private void PathFind (GameObject objectToPath)
    //{
    //    if (objectToPath.name != "Bounds")
    //    {
    //        Vector3 objectPosition = objectToPath.transform.position;
    //        List<Cell> path = null;
    //        if (Input.GetMouseButtonDown( 1 ))
    //        {
    //            lastPath = null;
    //            Vector3 worldPosition = GetMousePosition();
    //            if (grid.IsInsideGrid( worldPosition ))
    //            {
    //                lastPath = pathFinding.FindPath( objectPosition, worldPosition );
    //                if (path != null)
    //                {
    //                    for (int i = 0; i < path.Count - 1; i++)
    //                    {
    //                        int x = path[i].gridPosition.x;
    //                        int y = path[i].gridPosition.y;
    //                        int nextX = path[i + 1].gridPosition.x;
    //                        int nextY = path[i + 1].gridPosition.y;
    //                        Debug.DrawLine( new Vector3( x, y ) * cellSize + Vector3.one * 0.5f * cellSize, new Vector3( nextX, nextY ) * cellSize + Vector3.one * 0.5f * cellSize, Color.yellow, 5f );
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
    //private Vector3 GetMousePosition ( )
    //{
    //    Vector3 position = new Vector3( 0, 0, 10 ) + Camera.main.ScreenToWorldPoint( Input.mousePosition );
    //    return position;
    //}

    //int step;
    //float t = 0;
    //private void MoveObjectOnPath (GameObject objectToMove, List<Cell> path)
    //{
    //    if (path != null)
    //    {
    //        if (objectToMove.transform.position != path[step]?.worldPosition)
    //        {
    //            t += Time.deltaTime * objectsSpeed;
    //            objectToMove.transform.position = Vector3.Lerp( objectToMove.transform.position, path[step].worldPosition, t );
    //            if (objectToMove.transform.position == path[path.Count - 1].worldPosition)
    //            {
    //                lastPath = null;
    //                step = 0;
    //            }
    //        }
    //        else
    //        {
    //            step = ( step + 1 ) % path.Count;
    //            t = 0;
    //        }
    //    }
    //}
}

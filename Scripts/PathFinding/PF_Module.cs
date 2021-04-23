using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PF_Module : MonoBehaviour
{
    private bool isSelected = false;
    public float objectSpeed = 1f;
    public bool debugPath = true;
    private ObjectController controller;
    private PathFinding pathFinding;
    private Grid<Cell> grid;
    private List<Cell> path = null;
    Vector3 ToPosition;
    Vector3 originPosition;
    void Awake ( )
    {
        controller = FindObjectOfType<ObjectController>();
    }
    private void Start ( )
    {
        SubscribeToObjectSelectedEvent();
        SubscribeToGridChangeEvent();
        SubscribeToPathSetEvent();

    }
    void Update ( )
    {
        GetOriginPosition();
        MoveObjectOnPath();
    }
    private void GetOriginPosition ( )
    {
        if (MouseRightClick())
        {
            originPosition = gameObject.transform.position;
        }

    }

    int step;
    private void MoveObjectOnPath ( )
    {
        if (path != null && isSelected)
        {
            UnblockStartCell();
            Transform transform = gameObject.transform;
            Vector3 nextPoint = path[step].worldPosition;
            Vector3 goal = path[path.Count - 1].worldPosition;

            if (transform.position != nextPoint)
            {
                LerpTransformToMove( transform );

                if (transform.position == goal)
                {
                    ResetPathFindingState();
                }
            }
            else
            {
                AdvanceOnPathList();
                currentLerpTime = 0;
            }
        }
    }
    public List<Cell> FindPathToMove (PathFinding pathFinding)
    {

        List<Cell> path = null;
        if (isSelected)
        {
            path = pathFinding.FindPath( gameObject.transform.position, ToPosition );
        }

        ShowPathLines( path );
        return path;
    }

    float t = 0;
    float lerpTime = 0.2f;
    float currentLerpTime;
    private void LerpTransformToMove (Transform transform)
    {
        t = CalculatePercentOfLerpTime();
        transform.position = Vector3.Lerp( transform.position, path[step].worldPosition, t );
    }

    private float CalculatePercentOfLerpTime ( )
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > lerpTime)
        {
            currentLerpTime = lerpTime;
        }
        t = currentLerpTime / lerpTime;
        return t;
    }

    private void ResetPathFindingState ( )
    {
        path = null;
        step = 0;

        BlockGoalCell();

    }
    private void AdvanceOnPathList ( )
    {
        step = ( step + 1 ) % path.Count;
        t = 0;
    }
    private void UnblockStartCell ( )
    {
        if (pathFinding != null)
        {
            if (grid.GetGridObject( originPosition ).isWall)
            {
                pathFinding.SetCellWalkable( originPosition );
            }

        }
    }
    private void BlockGoalCell ( )
    {
        if (pathFinding != null)
        {
            if (!grid.GetGridObject( gameObject.transform.position ).isWall)
            {
                pathFinding.SetCellUnwalkable( gameObject.transform.position );

            }
        }
    }

    //private void HandleEventSubscription ( )
    //{
    //    if (MouseRightClick())
    //    {
    //        SubscribeToPathSetEvent();
    //        GetOriginPosition();
    //    }
    //    else
    //    {
    //        UnSubscribeToPathSetEvent();
    //    }
    //}


    private bool MouseRightClick ( )
    {
        return Input.GetMouseButtonDown( 1 );
    }

    //private void UnSubscribeToPathSetEvent ( )
    //{
    //    try
    //    {
    //        controller.OnPathSet -= PathRecieved;
    //    }
    //    catch (System.Exception)
    //    {
    //        Debug.Log( " PF_Module: Not subscribed to OnPathSet" );
    //    }
    //}


    private void PathRecieved (object sender, ObjectController.PathArgs e)
    {
        ToPosition = e.endPosition;
        path = FindPathToMove( pathFinding );
    }
    private void SubscribeToPathSetEvent ( )
    {
        controller.OnPathSet += PathRecieved;
    }
    private void ObjectRecieved (object sender, ObjectController.SelectArgs e)
    {
        CheckIfObjectIsSelected( e.ObjectSelected );
    }
    private void SubscribeToObjectSelectedEvent ( )
    {
        controller.OnObjectSelected += ObjectRecieved;
    }
    private void GridRecieved (object sender, ObjectController.GridArgs e)
    {
        grid = e.gridToPass;
        pathFinding = InstatiatePathFinding( grid );
    }
    private PathFinding InstatiatePathFinding (Grid<Cell> grid)
    {
        return new PathFinding( grid );
    }
    private void SubscribeToGridChangeEvent ( )
    {
        controller.OnGridChange += GridRecieved;
    }
    private void CheckIfObjectIsSelected (GameObject ObjectSelected)
    {
        if (ObjectSelected == this.gameObject)
        {
            isSelected = true;
        }
        else
        {
            isSelected = false;
        }
    }


    private void ShowPathLines (List<Cell> path)
    {
        if (debugPath && path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3Int currentCellPosition = path[i].gridPosition;
                int pathX = currentCellPosition.x;
                int pathY = currentCellPosition.y;
                Vector3 position = grid.GetWorldPosition( pathX, pathY );

                Vector3Int nextCellPosition = path[i + 1].gridPosition;
                int nextPathX = nextCellPosition.x;
                int nextPathY = nextCellPosition.y;
                Vector3 positionNext = grid.GetWorldPosition( nextPathX, nextPathY );

                Vector3 centerCell = Vector3.one * 0.5f * grid.cellSize;
                Debug.DrawLine( position + centerCell, positionNext + centerCell, Color.yellow, 5f );
            }
        }
    }
}

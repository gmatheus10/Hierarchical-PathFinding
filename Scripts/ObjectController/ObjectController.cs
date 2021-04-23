using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : GridDetection
{
    private GameObject selected = null;
    private Vector3 endPosition;

    void Update ( )
    {
        SelectObject();
        AssignPath();
    }
    public event EventHandler<PathArgs> OnPathSet;
    public class PathArgs : EventArgs
    {
        public Vector3 endPosition;
        public PathArgs (Vector3 endPosition)
        {
            this.endPosition = endPosition;
        }
    }

    private void AssignPath ( )
    {
        if (Input.GetMouseButtonDown( 1 ))
        {
            if (grid != null && selected != null)
            {
                GetEndPosition();
                InvokePathSet();

            }
        }
    }
    private void GetEndPosition ( )
    {
        endPosition = Utils.GetMousePosition();
    }
    private void InvokePathSet ( )
    {
        PathArgs pathArgs = new PathArgs( endPosition );
        OnPathSet?.Invoke( this, pathArgs );
    }
    public event EventHandler<SelectArgs> OnObjectSelected;
    public class SelectArgs : EventArgs
    {
        public GameObject ObjectSelected;
        public SelectArgs (GameObject ObjectSelected)
        {
            this.ObjectSelected = ObjectSelected;
        }
    }

    private void SelectObject ( )
    {
        if (Input.GetMouseButtonDown( 0 ))
        {
            float fractionOfCell = grid.cellSize * 0.01f;
            Vector3 center = Utils.GetMousePosition();
            LayerMask layersIgnored = ~LayerMask.GetMask( "Ignore Raycast" );

            FindObjectAroundTheCursor( center, fractionOfCell, layersIgnored );
            InvokeObjectSelected();
        }
    }
    private void FindObjectAroundTheCursor (Vector3 center, float fractionOfCell, LayerMask layersIgnored)
    {
        try
        {
            selected = Physics2D.OverlapCircle( center, fractionOfCell, layersIgnored ).gameObject;
        }
        catch (Exception)
        {
            selected = null;
        }
    }
    private void InvokeObjectSelected ( )
    {
        SelectArgs args = new SelectArgs( selected );
        OnObjectSelected?.Invoke( this, args );
    }
}

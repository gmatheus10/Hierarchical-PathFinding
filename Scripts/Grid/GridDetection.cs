using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDetection : MonoBehaviour
{
    public Grid<Cell> grid = null;
    public GameObject gridContainer = null;
    private Grid<Cell> LastGrid = null;
    private CreateGrid createGridScript;

    //public class GridContainer
    //{
    //    Grid<Cell> grid;
    //}
    void OnTriggerStay2D (Collider2D collision)
    {
        GameObject objectInsideGrid = collision.gameObject;
        createGridScript = objectInsideGrid.GetComponent<CreateGrid>();

        if (createGridScript != null)
        {
            SubscribeToGridEnter();
        }

        GridChangeHandler();
    }
    public event EventHandler<GridArgs> OnGridChange;
    public class GridArgs : EventArgs
    {
        public Grid<Cell> gridToPass;
        public GridArgs (Grid<Cell> gridToPass)
        {
            this.gridToPass = gridToPass;
        }
    }
    private void GridChangeHandler ( )
    {
        //add some condition for the controller 
        if (DetectChange())
        {
            UpdateLastGrid();
            InvokeGridChange();
        }
    }

    private void InvokeGridChange ( )
    {
        GridArgs args = new GridArgs( grid );
        OnGridChange?.Invoke( this, args );
    }

    public bool DetectChange ( )
    {
        return LastGrid != grid;
    }
    private void UpdateLastGrid ( )
    {
        LastGrid = grid;
    }
    void OnTriggerExit2D (Collider2D collision)
    {
        GameObject objectLeavingGrid = collision.gameObject;
        if (objectLeavingGrid.GetComponent<CreateGrid>() != null)
        {
            grid = null;

            if (createGridScript != null)
            {
                UnSubscribeToGridEnter();
                GridChangeHandler();
            }

            createGridScript = null;
        }
    }

    private void CreateGrid_OnGridEnter (object sender, CreateGrid.OnGridEventArgs e)
    {
        grid = e.grid;
        gridContainer = e.gridContainer;
    }

    private void SubscribeToGridEnter ( )
    {
        createGridScript.OnGridEnter += CreateGrid_OnGridEnter;
    }
    private void UnSubscribeToGridEnter ( )
    {
        createGridScript.OnGridEnter -= CreateGrid_OnGridEnter;
    }
}

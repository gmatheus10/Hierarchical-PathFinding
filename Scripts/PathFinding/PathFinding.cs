using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private Grid<Cell> grid;
    private List<Cell> openList;
    private List<Cell> closedList;

    public PathFinding (Grid<Cell> grid)
    {
        this.grid = grid;
    }
    ///  ////////////////////////////////////////////////////////////////////////
    public Grid<Cell> GetGrid ( )
    {
        return grid;
    }
    public List<Cell> FindPath (Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        //Debug.Log( grid.GetGridPosition( startWorldPosition ) );
        //Debug.Log( grid.GetGridPosition( endWorldPosition ) );
        Vector3Int start = grid.GetGridPosition( startWorldPosition );
        Vector3Int end = grid.GetGridPosition( endWorldPosition );
        return FindPath( start, end );

    }

    public List<Cell> FindPath (Vector3Int start, Vector3Int end)
    {
        Cell startNode = grid.GetGridObject( start );
        Cell endNode = grid.GetGridObject( end );

        openList = new List<Cell>() { startNode };
        closedList = new List<Cell>();
        ScanGridAndSetDefault();

        startNode.gCost = 0;
        // startNode.hCost = CalculateManhatamDistance( startNode, endNode );
        startNode.hCost = Utils.ManhatamDistance( startNode, endNode );
        startNode.CalculateF();
        // //
        while (openList.Count > 0)
        {
            Cell currentNode = GetLowestFCostNode( openList );

            if (currentNode == endNode)

            {
                return CalculatePath( endNode );
            }

            openList.Remove( currentNode );
            closedList.Add( currentNode );

            ScanNeighboursForBetterPath( endNode, currentNode );
        }
        // out of node on the open list
        return null;
        void ScanGridAndSetDefault ( )
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    Cell cell = grid.GetGridObject( new Vector3Int( x, y, 0 ) );
                    SetDefaultCell( cell );
                }
            }

            void SetDefaultCell (Cell cell)
            {
                cell.gCost = int.MaxValue;
                cell.CalculateF();
                cell.cameFromNode = null;
            }
        }
        Cell GetLowestFCostNode (List<Cell> pathNodeList)
        {
            Cell lowestFCostNode = pathNodeList[0];
            foreach (var node in pathNodeList)
            {
                if (node.FCost < lowestFCostNode.FCost)
                {
                    lowestFCostNode = node;
                }
            }
            return lowestFCostNode;
        }
        void ScanNeighboursForBetterPath (Cell endNode, Cell currentNode)
        {
            foreach (Cell neighbour in GetNeighboursList( currentNode ))
            {
                if (closedList.Contains( neighbour ))
                {
                    continue;
                }
                int tentativeGCost = currentNode.gCost + Utils.ManhatamDistance( currentNode, neighbour );

                if (tentativeGCost < neighbour.gCost)
                {
                    SetNeighbourCellPathValues( endNode, currentNode, neighbour, tentativeGCost );
                    IncludeNeighbourOnOpenList( neighbour );
                }
            }
            void SetNeighbourCellPathValues (Cell endNode, Cell currentNode, Cell neighbour, int tentativeGCost)
            {
                neighbour.cameFromNode = currentNode;
                neighbour.gCost = tentativeGCost;
                neighbour.hCost = Utils.ManhatamDistance( neighbour, endNode );
                //neighbour.hCost = CalculateManhatamDistance( neighbour, endNode );
                neighbour.CalculateF();
            }
            void IncludeNeighbourOnOpenList (Cell neighbour)
            {
                if (!openList.Contains( neighbour ))
                {
                    openList.Add( neighbour );
                }
            }
        }
        List<Cell> CalculatePath (Cell endNode)
        {
            List<Cell> path = new List<Cell>();
            path.Add( endNode );
            Cell queue = endNode;
            while (queue.cameFromNode != null)
            {
                path.Add( queue.cameFromNode );
                queue = queue.cameFromNode;
            }
            path.Reverse();
            return path;
        }
    }

    private List<Cell> GetNeighboursList (Cell currentNode)
    {
        float centerX = currentNode.worldPosition.x;
        float centerY = currentNode.worldPosition.y;

        float leftPosX = currentNode.worldPosition.x - grid.cellSize;
        float rightPosX = currentNode.worldPosition.x + grid.cellSize;
        float upPosY = currentNode.worldPosition.y + grid.cellSize;
        float downPosY = currentNode.worldPosition.y - grid.cellSize;

        List<Cell> neighbours = new List<Cell>();

        try
        {
            if (leftPosX >= grid.OriginPosition.x)
            {
                Cell leftCell = grid.GetGridObject( new Vector3( leftPosX, centerY, 0 ) );
                AddNonWall( leftCell );

                if (downPosY >= grid.OriginPosition.y)
                {
                    Cell downLeftCell = grid.GetGridObject( new Vector3( leftPosX, downPosY, 0 ) );
                    AddNonWall( downLeftCell );
                }

                if (upPosY < grid.FinalPosition.y)
                {
                    Cell upLeftCell = grid.GetGridObject( new Vector3( leftPosX, upPosY, 0 ) );
                    AddNonWall( upLeftCell );
                }

            }
            if (rightPosX < grid.FinalPosition.x)
            {
                Cell rightCell = grid.GetGridObject( new Vector3( rightPosX, centerY, 0 ) );
                AddNonWall( rightCell );

                if (downPosY >= grid.OriginPosition.y)
                {
                    Cell rightDownCell = grid.GetGridObject( new Vector3( rightPosX, downPosY, 0 ) );
                    AddNonWall( rightDownCell );
                }

                if (upPosY < grid.FinalPosition.y)
                {
                    Cell rightUpCell = grid.GetGridObject( new Vector3( rightPosX, upPosY, 0 ) );
                    AddNonWall( rightUpCell );
                }

            }
            if (downPosY >= grid.OriginPosition.y)
            {
                Cell downCell = grid.GetGridObject( new Vector3( centerX, downPosY, 0 ) );
                AddNonWall( downCell );
            }
            if (upPosY < grid.FinalPosition.y)
            {
                Cell upCell = grid.GetGridObject( new Vector3( centerX, upPosY, 0 ) );
                AddNonWall( upCell );
            }

        }
        catch (System.Exception)
        {
            Debug.DrawLine( currentNode.worldPosition, currentNode.worldPosition + new Vector3( 0.5f, 0.5f, 0 ), Color.blue, 100000f );
            throw;
        }
        return neighbours;

        void AddNonWall (Cell cell)
        {
            try
            {
                if (!cell.isWall)
                {
                    neighbours.Add( cell );
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
    public void SnapToGrid (GameObject objectToSnap)
    {
        //get the closest grid position to the objectToSnap world position
        //snap the object to the grid position
        Transform position = objectToSnap.GetComponent<Transform>();
        Vector3 cellCenter = new Vector3( grid.cellSize, grid.cellSize ) * 0.5f;
        position.position = grid.GetGridPosition( position.position ) + cellCenter + grid.OriginPosition;

    }
}

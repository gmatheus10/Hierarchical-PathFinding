using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointPathFinding
{
    //gives the general direction from the waypoints to the Hierarchichal_Pahtfinding script
    public int Level;
    public GameObject objectToPath;
    public List<Waypoint> waypoints = new List<Waypoint>();
    private List<Vector3> currentPath = new List<Vector3>();

    public Waypoint NodeToWaypoint (Node node)
    {
        Waypoint wayPoint = Waypoint.ConvertNode( node );
        waypoints.Add( wayPoint );
        return wayPoint;
    }
    public Waypoint FindClosestWaypoint (Vector3 target)
    {
        Waypoint closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (Waypoint wayPoint in waypoints)
        {
            float distance = ( wayPoint.position - target ).magnitude;
            if (distance < closestDistance)
            {
                closest = wayPoint;
                closestDistance = distance;
            }
        }
        if (closest != null)
        {
            return closest;
        }
        return null;
    }

    public List<Vector3> NavigateToWaypoint (Vector3 start, Vector3 destination)
    {
        Waypoint currentWaypoint = FindClosestWaypoint( start );
        Waypoint endWaypoint = FindClosestWaypoint( destination );
        if (currentWaypoint == null || endWaypoint == null || currentWaypoint == endWaypoint)
        {
            return null;
        }

        var openList = new SortedList<float, Waypoint>();
        List<Waypoint> closedList = new List<Waypoint>();

        currentWaypoint.previous = null;
        currentWaypoint.gCost = 0f;
        currentWaypoint.hCost = Mathf.Infinity;
        currentWaypoint.SetFCost();

        openList.Add( 0, currentWaypoint );

        while (openList.Count > 0)
        {
            currentWaypoint = openList.Values[0];

            if (currentWaypoint.cluster != endWaypoint.cluster)
            {
                if (openList.Values.Count > 1)
                {
                    float lowest = Mathf.Infinity;
                    Waypoint addNeighbour = null;
                    foreach (var n in endWaypoint.neighbours)
                    {
                        float dist = ( openList.Values[0].position - n.position ).magnitude;
                        if (dist < lowest)
                        {
                            addNeighbour = n;
                            lowest = dist;
                        }
                    }
                    endWaypoint.neighbours.Find( e => e == addNeighbour )?.neighbours.Add( openList.Values[0] );
                    openList.Values[0].neighbours.Add( addNeighbour );
                }
            }

            openList.RemoveAt( 0 );

            if (currentWaypoint.position == endWaypoint.position)
            {
                Debug.Log( "Found path" );
                return CalculatePath( currentWaypoint );
            }
            closedList.Add( currentWaypoint );
            foreach (Waypoint neighbour in currentWaypoint.neighbours)
            {

                if (closedList.Contains( neighbour ) || openList.ContainsValue( neighbour ))
                {
                    continue;
                }
                neighbour.previous = currentWaypoint;
                neighbour.gCost = currentWaypoint.gCost + ( neighbour.position - currentWaypoint.position ).magnitude;
                neighbour.hCost = ( neighbour.position - endWaypoint.position ).magnitude;
                neighbour.SetFCost();
                try
                {
                    openList.Add( neighbour.FCost, neighbour );
                }
                catch (System.Exception)
                {
                    int index = openList.Keys.IndexOf( neighbour.FCost );
                    if (openList.Values[index].gCost <= neighbour.gCost)
                    {
                        continue;
                    }
                    else
                    {
                        openList.RemoveAt( index );
                        openList.Add( neighbour.FCost, neighbour );
                    }
                }
                #region A*2
                //neighbour.gCost = currentWaypoint.gCost + ( neighbour.position - currentWaypoint.position ).magnitude;
                //neighbour.hCost = ( neighbour.position - endWaypoint.position ).magnitude;
                //neighbour.SetFCost();
                //neighbour.previous = currentWaypoint;

                //Waypoint openPosition = null;
                //float openF = Mathf.Infinity;
                //foreach (KeyValuePair<float, Waypoint> e in openList)
                //{
                //    if (e.Value.position == neighbour.position)
                //    {
                //        openPosition = e.Value;
                //    }
                //    if (e.Key == neighbour.FCost)
                //    {
                //        openF = e.Key;
                //    }
                //}
                //Waypoint closed = closedList.Find( c => c.position == neighbour.position );
                //if (openPosition != null)
                //{
                //    if (openPosition.FCost <= neighbour.FCost)
                //    {
                //        continue;
                //    }
                //}
                //else if (closed != null)
                //{

                //    if (closed.FCost <= neighbour.FCost)
                //    {
                //        continue;
                //    }
                //    closedList.Remove( closed );
                //    openList.Add( closed.FCost, closed );
                //}
                //else if (openF < Mathf.Infinity)
                //{
                //    int index = openList.IndexOfKey( openF );
                //    if (openList.Values[index].gCost <= neighbour.gCost)
                //    {
                //        continue;
                //    }
                //    else
                //    {
                //        openList.RemoveAt( index );
                //        openList.Add( neighbour.FCost, neighbour );

                //    }
                //}
                //else
                //{
                //    openList.Add( neighbour.FCost, neighbour );
                //}
                #endregion
            }

        }
        Debug.Log( "Couldn't find path" );
        Debug.Log( closedList.Count );
        return null;
    }
    private List<Vector3> CalculatePath (Waypoint endWaypoint)
    {

        List<Vector3> path = new List<Vector3>();
        while (endWaypoint.previous != null)
        {
            path.Add( endWaypoint.position );
            HPA_Utils.DrawCrossInPosition( endWaypoint.position, Color.red );

            endWaypoint = endWaypoint.previous;

        }
        path.Add( endWaypoint.position );
        HPA_Utils.DrawCrossInPosition( endWaypoint.position, Color.red );

        path.Reverse();
        return path;
    }
    //need to convert the big paths in a series of level n-1 waypoint paths
    //when level = 1 then it becomes pure A*
}

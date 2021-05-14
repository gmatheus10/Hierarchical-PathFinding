using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint
{
    // Start is called before the first frame update
    public List<Waypoint> neighbours;
    public Cluster cluster;
    public Waypoint previous;
    public float gCost;
    public float hCost;
    public float FCost;
    public Vector3 position;
    public Waypoint (Vector3 position)
    {
        this.position = position;
    }
    public float GetDistance (Waypoint other)
    {
        return ( other.position - this.position ).magnitude;
    }
    public void SetFCost ( )
    {
        this.FCost = this.gCost + this.hCost;
    }
    public static Waypoint ConvertNode (Node node)
    {

        return new Waypoint( node.worldPosition );
    }
    public void SetNeighbours (List<Waypoint> neighbours)
    {
        this.neighbours = neighbours;
    }
}

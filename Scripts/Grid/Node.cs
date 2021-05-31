using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int level;
    public Vector3 WorldPosition { get; private set; }
    public Vector3Int GridPosition { get; private set; }
    public Node Pair { get; private set; }
    public Cluster cluster;

    public float gCost = 0;
    public float hCost = 0;
    public float FCost { get; private set; }
    public Node Parent { get; private set; }
    public List<KeyValuePair<int, Node>> neighbours = new List<KeyValuePair<int, Node>>();
    public Node ( )
    {

    }
    public Node (Cluster cluster)
    {
        this.cluster = cluster;
    }
    public void SetFCost ( )
    {
        this.FCost = gCost + hCost;
    }
    public void SetParent (Node Parent)
    {
        this.Parent = Parent;
    }
    public void SetPair (Node pair)
    {
        this.Pair = pair;
        KeyValuePair<int, Node> p = new KeyValuePair<int, Node>( 10, Pair );
        neighbours.Add( p );
    }
    public void AddNeighbour (Node node, int Distance)
    {
        if (node.WorldPosition != this.WorldPosition)
        {
            if (node.level == this.level)
            {
                if (node.cluster == this.cluster)
                {
                    KeyValuePair<int, Node> n = new KeyValuePair<int, Node>( Distance, node );
                    neighbours.Add( n );
                }
            }
        }

    }

    public void SetPosition (Vector3 WorldPosition)
    {
        this.WorldPosition = WorldPosition;
    }
    public void SetGridPosition (Vector3Int GridPosition)
    {
        this.GridPosition = GridPosition;
    }
}

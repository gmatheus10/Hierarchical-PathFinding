using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IComparer<Node>, IEquatable<Node>
{
    public int level;
    public float gCost = 0;
    public float hCost = 0;
    public float FCost { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public Vector3Int GridPosition { get; private set; }
    public Node Pair { get; private set; }
    public Node Parent { get; private set; }
    public Cluster cluster;
    public List<Node> neighbours = new List<Node>();

    public Node ( )
    {
    }
    public Node (Cluster cluster)
    {
        this.cluster = cluster;
    }

    public void SetPosition (Vector3 WorldPosition)
    {
        this.WorldPosition = WorldPosition;
    }
    public void SetGridPosition (Vector3Int GridPosition)
    {
        this.GridPosition = GridPosition;
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
        neighbours.Add( pair );
    }
    public void AddNeighbour (Node node)
    {
        if (this.WorldPosition != node.WorldPosition)
        {
            if (!this.neighbours.Contains( node ))
            {
                this.neighbours.Add( node );
            }
        }
    }
    public int Compare (Node a, Node b)
    {
        float distance1 = Utils.ManhatamDistance( this.GridPosition, a.GridPosition );
        float distance2 = Utils.ManhatamDistance( this.GridPosition, b.GridPosition );
        return distance1.CompareTo( distance2 );
    }
    public bool Equals (Node other)
    {
        if (this.WorldPosition == other.WorldPosition && this.GridPosition == other.GridPosition && this.level == other.level)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public enum EdgeType
    {
        INTRA, INTER
    }
    public Node n1;
    public Node n2;
    public int Weight;
    public int Level;
    public EdgeType edgeType;
    public Edge (Node n1, Node n2, int Level, EdgeType type)
    {
        this.n1 = n1;
        this.n2 = n2;
        this.Level = Level;
        this.edgeType = type;
    }
    public Edge (Node n1, Node n2, int Level, EdgeType type, int Weight) : this( n1, n2, Level, type )
    {
        this.Weight = Weight;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HPA_Utils
{
    public static void DrawCrossInPosition (Vector3 position, Color color)
    {
        float crossLength = 0.2f;
        Vector3 crossX = new Vector3( crossLength, 0, 0 );
        Vector3 crossY = new Vector3( 0, crossLength, 0 );
        Debug.DrawLine( position - crossX, position + crossX, color, 100000f );
        Debug.DrawLine( position - crossY, position + crossY, color, 100000f );
    }
    public static void DrawCrossInCell (Cell c, Color color)
    {
        DrawCrossInPosition( c.worldPosition, color );
    }

    public static void DrawClusters (Vector3 position, float Width, float Height, Color color)
    {
        ///drawing Cluster border///
        Debug.DrawLine( position, new Vector3( position.x, position.y + Height ), color, 10000f, false );
        Debug.DrawLine( position, new Vector3( position.x + Width, position.y ), color, 10000f, false );
        Debug.DrawLine( new Vector3( position.x, position.y + Height ), new Vector3( position.x + Width, position.y + Height ), color, 10000f, false );
        Debug.DrawLine( new Vector3( position.x + Width, position.y ), new Vector3( position.x + Width, position.y + Height ), color, 10000f, false );
        ///drawing Cluster border///
    }
    public static void DrawClusters (Vector3 position, Vector2Int size, Color color)
    {
        DrawClusters( position, size.x, size.y, color );
    }
    public static void DrawClusters (Cluster cluster, Color color)
    {
        DrawClusters( cluster.originPosition, cluster.size, color );
    }
    public static void DrawEdge (Edge edge, Color color, float duration)
    {
        Debug.DrawLine( edge.n1.worldPosition, edge.n2.worldPosition, color, duration );
    }

    public static void DrawAllNodesInCluster (Cluster cluster)
    {
        foreach (Node node in cluster.clusterNodes)
        {
            DrawCrossInCell( node.cell, Color.yellow );
        }
    }

    public static void ShowPathLines (List<Vector3> path)
    {
        if (path != null)
        {
            for (int i = 1; i <= path.Count - 1; i++)
            {
                Debug.DrawLine( path[i], path[i - 1], Color.red, 15f );
            }
        }
    }
    public static void ShowPathLines (List<Cluster> path)
    {
        List<Vector3> pathPositions = new List<Vector3>();
        if (path != null)
        {
            foreach (var c in path)
            {
                pathPositions.Add( c.originPosition );
            }
        }
        ShowPathLines( pathPositions );
    }
}

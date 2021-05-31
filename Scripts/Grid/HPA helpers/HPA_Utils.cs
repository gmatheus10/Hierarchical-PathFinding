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
        DrawClusters( cluster.OriginPosition, cluster.size, color );
    }
    public static void DrawNodesInCluster (Cluster cluster)
    {
        foreach (Node node in cluster.clusterNodes)
        {
            DrawCrossInPosition( node.WorldPosition, Color.yellow );
        }
    }

    public static void DrawClusterEntrances (Cluster cluster)
    {
        foreach (var pair in cluster.entrances)
        {
            Entrance current = pair.Value;

            foreach (var cell in current.entranceTiles)
            {
                DrawCrossInCell( cell, Color.yellow );
            }
            foreach (var node in current.entranceNodes)
            {
                DrawCrossInPosition( node.WorldPosition, Color.blue );
                DrawCrossInPosition( node.Pair.WorldPosition, Color.red );
            }
        }
    }
    public static void DrawClusterBorders (Cluster cluster)
    {
        foreach (var border in cluster.borders)
        {
            foreach (Cell c in border.Value)
            {
                DrawCrossInCell( c, Color.yellow );
            }
        }
    }


    public static void ShowPathLines (List<Vector3> path, Color color)
    {
        if (path != null)
        {
            for (int i = 1; i <= path.Count - 1; i++)
            {
                Debug.DrawLine( path[i], path[i - 1], color, 15000f );
            }
        }
    }
    public static void ShowPathLines (List<Cluster> path, Color color)
    {
        List<Vector3> pathPositions = new List<Vector3>();
        if (path != null)
        {
            foreach (var c in path)
            {
                pathPositions.Add( c.OriginPosition + new Vector3( c.size.x, c.size.y, 0 ) * 0.5f );
            }
        }
        ShowPathLines( pathPositions, color );
    }
    public static void ShowPathClusters (List<Cluster> path, Color color)
    {
        if (path != null)
        {
            foreach (var c in path)
            {
                DrawClusters( c, color );
            }
        }
    }
    public static void ShowPathLines (List<Node> path, Color color)
    {
        if (path != null)
        {
            List<Vector3> newPath = new List<Vector3>();
            foreach (Node node in path)
            {
                newPath.Add( node.WorldPosition );
            }
            ShowPathLines( newPath, color );
        }

    }
}

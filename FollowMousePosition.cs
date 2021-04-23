using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMousePosition : MonoBehaviour
{
    public float radius;
    private void Awake ( )
    {
        MoveToMousePosition();
    }
    void Update ( )
    {
        MoveToMousePosition();
        DrawMouseCursor();
    }
    private void MoveToMousePosition ( )
    {
        Vector3 mousePosition = Utils.GetMousePosition();
        transform.position = Vector3.Lerp( transform.position, mousePosition, 1 );
    }
    private void DrawMouseCursor ( )
    {
        Vector3 currentPosition = transform.position;
        Vector3 xModifier = new Vector3( radius * 0.5f, 0 );
        Vector3 yModifier = new Vector3( 0, radius * 0.5f );
        Debug.DrawLine( currentPosition - xModifier, currentPosition + xModifier );
        Debug.DrawLine( currentPosition - yModifier, currentPosition + yModifier );
    }
}

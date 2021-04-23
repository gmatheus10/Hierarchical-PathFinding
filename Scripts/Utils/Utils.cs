using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Utils
{
    public static Vector3 GetMousePosition ( )
    {
        return Camera.main.ScreenToWorldPoint( Input.mousePosition ) + new Vector3( 0, 0, 10 );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOnClick : MonoBehaviour
{
    public delegate void OnClick (Vector3 ClickPosition);

    public event OnClick PassMousePosition;



    private void Update ( )
    {
        CallEvents();
    }

    private void CallEvents ( )
    {
        if (Input.GetMouseButtonDown( 0 ))
        {
            Vector3 MouseClick = Utils.GetMousePosition();
            PassMousePosition?.Invoke( MouseClick );
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    GameObject objectToFollow = null;
    ObjectController controller;
    //get selected object -> subscribe to the controller onObjectSelected  event 
    //center camera on object position
    //follow object when it moves
    private void Awake ( )
    {
        controller = FindObjectOfType<ObjectController>();
    }
    private void Start ( )
    {
        SubscribeToObjectSelectedEvent();
    }
    private void SubscribeToObjectSelectedEvent ( )
    {
        controller.OnObjectSelected += ObjectRecieved;
    }
    private void ObjectRecieved (object sender, ObjectController.SelectArgs e)
    {
        objectToFollow = e.ObjectSelected;
        DoubleClickCenterCamera();
    }
    private int count = 0;
    private void DoubleClickCenterCamera ( )
    {
        //PUT A TIME LIMIT TO RESET THE COUNT
        int doubleClick = 2;
        GameObject bufferObject = objectToFollow;
        if (Input.GetMouseButtonDown( 0 ))
        {
            count += 1;
            if (bufferObject == objectToFollow)
            {
                if (count >= doubleClick)
                {
                    CenterCameraOnObject();
                    count = 0;
                }
            }
            else
            {
                count = 0;
            }

        }
    }
    private void CenterCameraOnObject ( )
    {

        if (objectToFollow != null)
        {
            Vector3 endPosition = objectToFollow.transform.position;
            Vector3 cameraOffSetZ = new Vector3( 0, 0, transform.position.z );
            transform.position = Vector3.Lerp( transform.position, endPosition + cameraOffSetZ, 1 );
        }
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PF_Module : MonoBehaviour
{
    //recieve a list of vector3 and move the object through the points on the list

    private List<Vector3> path = null;
    void Update ( )
    {
        MoveObjectOnPath();
    }
    int step;
    private void MoveObjectOnPath ( )
    {
        if (path != null && path.Count > 0)
        {
            Transform transform = gameObject.transform;
            Vector3 nextPoint = path[step];
            Vector3 goal = path[path.Count - 1];

            if (transform.position != nextPoint)
            {
                LerpTransformToMove( transform );

                if (transform.position == goal)
                {
                    ResetPathFindingState();
                }
            }
            else
            {
                AdvanceOnPathList();
                currentLerpTime = 0;
            }
        }
    }

    //LERP CONFIGURATIONS:
    float t = 0;
    static float lerpTime = 0.2f;
    float currentLerpTime;
    //
    private void LerpTransformToMove (Transform transform)
    {
        t = CalculatePercentOfLerpTime();
        transform.position = Vector3.Lerp( transform.position, path[step], t );
    }

    private float CalculatePercentOfLerpTime ( )
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > lerpTime)
        {
            currentLerpTime = lerpTime;
        }
        t = currentLerpTime / lerpTime;
        return t;
    }

    private void ResetPathFindingState ( )
    {
        path = null;
        step = 0;
        currentLerpTime = 0;
    }
    private void AdvanceOnPathList ( )
    {
        step = ( step + 1 ) % path.Count;
        t = 0;
    }


}

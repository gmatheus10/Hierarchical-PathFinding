
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
  public float speed;
  private float lerpTime = 1f;
  private float currentLerpTime;


  private Hierarchical_Pathfinding hpa;

  private void Awake()
  {
    hpa = gameObject.GetComponent<Hierarchical_Pathfinding>();
  }
  private void Start()
  {
    hpa.OnTreeBuilt += TreeRecieved;
  }
  private void TreeRecieved(object sender, TreeData<List<Cluster>> data)
  {
    TreeData<List<Cluster>> showDataBro = data;

  }
  private void StartLerp()
  {
    currentLerpTime += Time.deltaTime;
    if (currentLerpTime > lerpTime)
    {
      currentLerpTime = lerpTime;
    }

    float perc = currentLerpTime / lerpTime;
    // gameObject.transform.position = Vector3.Lerp();
  }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class TrialManager : MonoBehaviour
{
    public List<GameObject> StartingPlayerSpawnPoints;
    private int SpawnPointIndex = 0;
    List<string> conditionAssignments;
    public bool isPractice;
    public GameObject XROrigin;

    // IEnumerator SetupTrial()
    // {
    //     // yield return new WaitForSeconds(0.5f);
    //     // session.BeginNextTrial();
    // }

    // Trial Start
    public void StartTrial()
    {
        // session.BeginNextTrial();
        // Debug.Log("Trial started");
        // Debug.Log("Start time:" + DateTime.Now);
    }

    // Trial End
    // public void StopTrial()
    // {
    //     StopAllCoroutines();
    // }

    

    // void Start()
    // {
    //     StartCoroutine(SetupTrial());
    // }

    public void Testfunction()
    {
        Debug.Log("Trial test function called");
    }

    public void SpawnPointSelection()
    {
        if (StartingPlayerSpawnPoints != null && StartingPlayerSpawnPoints.Count > 0)
        {
            XROrigin.transform.position = StartingPlayerSpawnPoints[SpawnPointIndex].transform.position;
            Debug.Log("Spawn point selected: " + StartingPlayerSpawnPoints[SpawnPointIndex].name);
            SpawnPointIndex++;
        }
        else
        {
            Debug.LogWarning("No spawn points available in the list.");
        }
    }
}

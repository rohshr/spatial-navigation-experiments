using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UXF;
using NUnit.Framework.Constraints;

public class SessionGenerator : MonoBehaviour
{
    // public GameObject NodeTrialSpawnPoint;
    // public GameObject FreeTeleportTrialSpawnPoint;
    // public GameObject ContinuousTrialSpawnPoint;
    // public GameObject NodePracticeSpawnPoint;
    // public GameObject FreeTeleportPracticeSpawnPoint;
    // public List<GameObject> StartingPlayerSpawnPoints;
    // private int SpawnPointIndex = 0;
    // List<string> conditionAssignments;
    // public bool isPractice;
    // public GameObject XROrigin;

    // Session Start
    public void GenerateExperiment(Session session)
    {
        // String testString = session.settings.GetString("test_string");
        // Debug.Log("Test string: " + testString);
        String locomotionMethod = session.settings.GetString("locomotion_method");
        String locomotionMethodFromUI = session.participantDetails["locomotion_method"].ToString();
        String preferredHandFromUI = session.participantDetails["preferred_hand"].ToString();
        // Debug.Log("Locomotion method from settings: " + locomotionMethod);
        Debug.Log("Locomotion method from UI: " + locomotionMethodFromUI);
        Debug.Log("Preferred hand from UI: " + preferredHandFromUI);
        session.settings.SetValue("locomotion_method", locomotionMethodFromUI);
        session.settings.SetValue("preferred_hand", preferredHandFromUI);

        // Curved corridor practice block
        Block curvedPracticeBlock = session.CreateBlock(1);

        // Angle corridor practice block
        Block angledPracticeBlock = session.CreateBlock(1);

        // Open space practice block
        Block openSpacePracticeBlock = session.CreateBlock(4);
        // Block routeFindingBlock = session.CreateBlock(nRouteFindingTrials);
        // routeFindingBlock.settings.SetValue("condition", "route-finding");
    }

    // public void SpawnPointSelection()
    // {
    //     if (StartingPlayerSpawnPoints != null && StartingPlayerSpawnPoints.Count > 0)
    //     {
    //         XROrigin.transform.position = StartingPlayerSpawnPoints[SpawnPointIndex].transform.position;
    //         Debug.Log("Spawn point selected: " + StartingPlayerSpawnPoints[SpawnPointIndex].name);
    //         SpawnPointIndex++;
    //     }
    //     else
    //     {
    //         Debug.LogWarning("No spawn points available in the list.");
    //     }
    // }
    // public void SpawnPointSelection()
    // {
    //     if (isPractice)
    //     {
    //         if (conditionAssignment == "continuous")
    //         {
    //             XROrigin.transform.position = ContinuousPracticeSpawnPoint.transform.position;
    //         }
    //         else if (conditionAssignment == "free_teleport")
    //         {
    //             XROrigin.transform.position = FreeTeleportPracticeSpawnPoint.transform.position;
    //         }
    //         else
    //         {
    //             XROrigin.transform.position = NodePracticeSpawnPoint.transform.position;
    //         }
    //     }
    //     else
    //     {
    //         if (conditionAssignment == "continuous")
    //         {
    //             XROrigin.transform.position = ContinuousTrialSpawnPoint.transform.position;
    //         }
    //         else if (conditionAssignment == "free_teleport")
    //         {
    //             XROrigin.transform.position = FreeTeleportTrialSpawnPoint.transform.position;
    //         }
    //         else
    //         {
    //             XROrigin.transform.position = NodeTrialSpawnPoint.transform.position;
    //         }
    //     }
    //     Debug.Log("Spawn point selected");
    // }
}

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
        // // retrieve the n_practice_trials setting from the session settings
        // int numPracticeTrials = session.settings.GetInt("n_practice_trials");
        // // create block 1
        // Block practiceBlock = session.CreateBlock(numPracticeTrials);
        // practiceBlock.settings.SetValue("practice", true);

        /*
            3 conditions
            1: Continuous movement
            2: Free teleportation
            3: Node-based teleportation
        */
        String testString = session.settings.GetString("test_string");
        Debug.Log("Test string: " + testString);
        int condition = UnityEngine.Random.Range(1, 4);
        // int nRouteFindingTrials = session.settings.GetInt("n_route_finding_trials");

        // if (condition == 1)
        // {
        //     // Continuous movement
        //     // session.CurrentTrial.result["condition"] = "continuous";
        //     Debug.Log("Continuous movement");
        //     conditionAssignment = "continuous";
        // }
        // else if (condition == 2)
        // {
        //     // Free teleportation
        //     // session.CurrentTrial.result["condition"] = "free-teleport";
        //     Debug.Log("Free teleportation");
        //     conditionAssignment = "free_teleport";
        // }
        // else
        // {
        //     // Node-based teleportation
        //     // session.CurrentTrial.result["condition"] = "node-teleport";
        //     Debug.Log("Node-based teleportation");
        //     conditionAssignment = "node_teleport";
        // }

        // Create free exploration block
        // Block freeExplorationBlock = session.CreateBlock(2);
        // freeExplorationBlock.settings.SetValue("condition", "practice");

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

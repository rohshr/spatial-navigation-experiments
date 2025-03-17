using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UXF;
using NUnit.Framework.Constraints;

public class SessionGenerator : MonoBehaviour
{
    public GameObject NodeTrialSpawnPoint;
    public GameObject FreeTeleportTrialSpawnPoint;
    public GameObject ContinuousTrialSpawnPoint;
    public GameObject NodePracticeSpawnPoint;
    public GameObject FreeTeleportPracticeSpawnPoint;
    public GameObject ContinuousPracticeSpawnPoint;
    string conditionAssignment;
    bool isPractice = true;
    public GameObject XROrigin;

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
        int condition = UnityEngine.Random.Range(1, 4);
        int nRouteFindingTrials = session.settings.GetInt("n_route_finding_trials");

        if (condition == 1)
        {
            // Continuous movement
            // session.CurrentTrial.result["condition"] = "continuous";
            Debug.Log("Continuous movement");
            conditionAssignment = "continuous";
        }
        else if (condition == 2)
        {
            // Free teleportation
            // session.CurrentTrial.result["condition"] = "free-teleport";
            Debug.Log("Free teleportation");
            conditionAssignment = "free_teleport";
        }
        else
        {
            // Node-based teleportation
            // session.CurrentTrial.result["condition"] = "node-teleport";
            Debug.Log("Node-based teleportation");
            conditionAssignment = "node_teleport";
        }

        // Create free exploration block
        Block freeExplorationBlock = session.CreateBlock(2);
        freeExplorationBlock.settings.SetValue("condition", "practice");

        // Block routeFindingBlock = session.CreateBlock(nRouteFindingTrials);
        // routeFindingBlock.settings.SetValue("condition", "route-finding");
    }

    public void SpawnPointSelection()
    {
        if (isPractice)
        {
            if (conditionAssignment == "continuous")
            {
                XROrigin.transform.position = ContinuousPracticeSpawnPoint.transform.position;
            }
            else if (conditionAssignment == "free_teleport")
            {
                XROrigin.transform.position = FreeTeleportPracticeSpawnPoint.transform.position;
            }
            else
            {
                XROrigin.transform.position = NodePracticeSpawnPoint.transform.position;
            }
        }
        else
        {
            if (conditionAssignment == "continuous")
            {
                XROrigin.transform.position = ContinuousTrialSpawnPoint.transform.position;
            }
            else if (conditionAssignment == "free_teleport")
            {
                XROrigin.transform.position = FreeTeleportTrialSpawnPoint.transform.position;
            }
            else
            {
                XROrigin.transform.position = NodeTrialSpawnPoint.transform.position;
            }
        }
        Debug.Log("Spawn point selected");
    }
}

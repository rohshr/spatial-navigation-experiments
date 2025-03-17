using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UXF;

public class SessionGenerator : MonoBehaviour
{
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
        }
        else if (condition == 2)
        {
            // Free teleportation
            // session.CurrentTrial.result["condition"] = "free-teleport";
            Debug.Log("Free teleportation");
        }
        else
        {
            // Node-based teleportation
            // session.CurrentTrial.result["condition"] = "node-teleport";
            Debug.Log("Node-based teleportation");
        }

        // Create free exploration block
        Block freeExplorationBlock = session.CreateBlock(2);
        freeExplorationBlock.settings.SetValue("condition", "practice");

        // Block routeFindingBlock = session.CreateBlock(nRouteFindingTrials);
        // routeFindingBlock.settings.SetValue("condition", "route-finding");
    }
}

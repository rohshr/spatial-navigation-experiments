using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UXF;

public class SessionGenerator : MonoBehaviour
{
    void Awake()
    {
        // To make sure this object does not get destroyed when a new scene loads
        DontDestroyOnLoad(gameObject);
    }

    // Session Start
    public void GenerateExperiment(Session session)
    {
        // retrieve the n_practice_trials setting from the session settings
        int numPracticeTrials = session.settings.GetInt("n_practice_trials");
        // create block 1
        Block practiceBlock = session.CreateBlock(numPracticeTrials);
        practiceBlock.settings.SetValue("practice", true);

        // // retrieve the n_main_trials setting from the session settings
        // int numMainTrials = session.settings.GetInt("n_main_trials");
        // // create block 2
        // Block mainBlock = session.CreateBlock(numMainTrials); // block 2

        // // here we set a setting for the 2nd trial of the main block as an example.
        // mainBlock.GetRelativeTrial(2).settings.SetValue("size", 10);
        // mainBlock.GetRelativeTrial(2).settings.SetValue("color", Color.red);

        // // we enable a setting if this is the first session, e.g. to show instructions
        // session.GetTrial(1).settings.SetValue("show_instructions", session.number == 1);

    }

    // Trial Start
    public void SetupTrial(Trial trial)
    {
        // Sets up each trial
    }

    // Trial End
    public void CleanupTrial(Trial trial)
    {
        // Perform scene cleanup or other actions after trial ends
        
    }
}

using System;
using System.Collections;
using UnityEngine;
using UXF;

public class TrialManager : MonoBehaviour
{
    public Session session;
    // Trial Start
    public void SetupTrial(Trial trial)
    {
        // Sets up each trial
        // Trial newTrial = trial.block.CreateTrial();
        // newTrial.Begin();
        session.BeginNextTrial();
        Debug.Log("Start time:" + DateTime.Now);
    }

    // Trial End
    public void CleanupTrial(Trial trial)
    {
        session.EndCurrentTrial();
    }

    IEnumerator SetupTrial()
    {
        yield return new WaitForSeconds(0.5f);
        session.BeginNextTrial();
    }

    // void Start()
    // {
    //     StartCoroutine(SetupTrial());
    // }
}

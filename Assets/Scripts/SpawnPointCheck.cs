using System;
using System.Collections;
using UnityEngine;
using UXF;

public class SpawnPointCheck : MonoBehaviour
{
    // IEnumerator SetupTrial()
    // {
    //     yield return new WaitForSeconds(0.5f);
    //     Session.instance.BeginNextTrial();
    // }

    // // Trial Start
    // public void StartTrial()
    // {
    //     Session.instance.BeginNextTrial();
    //     Debug.Log("Trial started");
    //     Debug.Log("Start time:" + DateTime.Now);
    // }
    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit called in SpawnPointCheck");
        if (other.CompareTag("Player") && !Session.instance.InTrial)
        {
            Debug.Log("Player exited the trigger zone in SpawnPointCheck");
            Debug.Log("Start time:" + System.DateTime.Now);
            // Session.instance.BeginNextTrial();
        }
    }
}

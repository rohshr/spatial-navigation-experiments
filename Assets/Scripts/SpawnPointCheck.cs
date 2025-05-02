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
        if (other.CompareTag("Player") && Session.instance.InTrial)
        {
            if (CompareTag("SpawnPoint"))
            {
                Debug.Log("Player exited the SpawnPoint.");
                Debug.Log("Time:" + System.DateTime.Now);
                Session.instance.BeginNextTrial();
            } else if (CompareTag("UIViewpoint"))
            {
                Debug.Log("Player exited the UIViewpoint.");
                Debug.Log("Time:" + System.DateTime.Now);
            }
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using UXF;

public class SpawnPointCheck : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && Session.instance.hasInitialised)
        {
            if (CompareTag("SpawnPoint"))
            {
                Debug.Log("Player exited a trial SpawnPoint at" + DateTime.Now);
                Session.instance.BeginNextTrial();
            } else if (CompareTag("UIViewpoint"))
            {
                Debug.Log("Player exited the UIViewpoint at " + DateTime.Now);
            }
        }
    }
}

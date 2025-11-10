using System;
using System.Collections;
using UnityEngine;
using UXF;

public class SpawnPointCheck : MonoBehaviour
{
    private SessionGenerator sessionGenerator;
    public static event Action OnPlayerExitedSpawnPoint;
    
    private void Awake()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && Session.instance.hasInitialised && Session.instance.InTrial)
        {
            if (CompareTag("SpawnPoint"))
            {
                Debug.Log("Player exited a trial SpawnPoint at " + DateTime.Now);
                if (sessionGenerator.GetCurrentBlockType() != "TimedExploration")
                    GameStopwatch.StartStopwatch();
                // Session.instance.BeginNextTrial();
                
                Session.instance.CurrentTrial.settings.SetValue("object", sessionGenerator.GetCurrentObjectToFind()?.name);

                if (sessionGenerator.GetCurrentBlockType() == "GuidedExploration")
                {
                    Debug.Log("Guided Exploration block detected. Starting dialog flow.");
                    OnPlayerExitedSpawnPoint?.Invoke();
                }
            }
            else if (CompareTag("UIViewpoint"))
            {
                Debug.Log("Player exited the UIViewpoint at " + DateTime.Now);
            }
        }
    }
}

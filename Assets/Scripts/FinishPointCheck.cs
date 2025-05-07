using System;
using UnityEngine;
using UXF;

public class FinishPointCheck : MonoBehaviour
{
    public GameObject XROrigin; // XROrigin player gameobject

    public static event Action OnFinishPointReached; // Event to notify when the finish point is reached
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Session.instance.InTrial)
        {
            // Check if the player is in the practice trial area
            if (XROrigin != null)
            {
                Debug.Log("Player reached the trial FinishPoint at " + DateTime.Now);
                Session.instance.CurrentTrial.End();
                //  XROrigin.transform.position = instructionsUIViewpoint
                OnFinishPointReached?.Invoke(); // Trigger the event
            }
            else
            {
                Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
            }
        }
    }
}

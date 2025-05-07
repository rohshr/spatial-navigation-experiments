using System;
using UnityEngine;
using UXF;

public class ObjectCollisionDetection : MonoBehaviour
{
    public GameObject XROrigin; // XROrigin player gameobject

    public static event Action OnObjectCollided;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Session.instance.InTrial)
        {
            // Check if the player is in the practice trial area
            if (XROrigin != null)
            {
                Debug.Log($"Collision detected with object: {gameObject.name} at {DateTime.Now}");
                Session.instance.CurrentTrial.End();
                OnObjectCollided?.Invoke(); // Trigger the event
            }
            else
            {
                Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
            }
        }
    }
}

using UnityEngine;
using UXF;

public class FinishPointCheck : MonoBehaviour
{
    public GameObject XROrigin; // XROrigin player gameobject

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called in PracticeTrialController");
        if (other.CompareTag("Player") && Session.instance.InTrial)
        {
            Debug.Log("Player reached the finish point");
            Debug.Log("End time:" + System.DateTime.Now);
            // Check if the player is in the practice trial area
            if (XROrigin != null)
            {                
                Session.instance.CurrentTrial.End();
            }
            else
            {
                Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
            }
        }
    }
}

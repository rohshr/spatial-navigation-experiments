using UnityEngine;

public class PracticeTrialController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject XROrigin;

    public GameObject NextSpawnPoint; // Assign this in the Inspector

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called in PracticeTrialController");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger zone in PracticeTrialController");
            // Check if the player is in the practice trial area
            if (XROrigin != null)
            {
                // Move the player to the practice trial spawn point
                XROrigin.transform.position = NextSpawnPoint.transform.position;
                
                Debug.Log("Player moved to next spawn point");
            }
            else
            {
                Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
            }
        }
    }
}

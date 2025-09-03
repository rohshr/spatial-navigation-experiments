using System;
using UnityEngine;
using UXF;

public class ObjectCollisionDetection : MonoBehaviour
{
    public GameObject XROrigin; // XROrigin player gameobject
    private SessionGenerator sessionGenerator;
    private GameObject objectToFind;
    public static event Action OnObjectCollided;

    private void Awake()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(sessionGenerator.GetCurrentBlockType() != "ObjectSearch")
            return;
        
        objectToFind = sessionGenerator.GetCurrentObjectToFind();
        if (gameObject != objectToFind)
        {
            Debug.Log($"Incorrect object found: {gameObject.name} at {DateTime.Now}. Expected: {objectToFind?.name}");
            return;
        }
        if (other.CompareTag("Player") && Session.instance.InTrial)
        {
            // Check if the player is in the practice trial area
            if (XROrigin != null)
            {
                Session.instance.CurrentTrial.settings.SetValue("object",gameObject.name);
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

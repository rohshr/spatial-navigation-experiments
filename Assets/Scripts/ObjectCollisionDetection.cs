using System;
using UnityEngine;
using Unity.XR.CoreUtils;
using UXF;

public class ObjectCollisionDetection : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    private SessionGenerator sessionGenerator;
    private PlayerPositionTracker playerPositionTracker;
    private GameObject objectToFind;
    public static event Action OnObjectCollided;

    private void Awake()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
        playerPositionTracker = FindFirstObjectByType<PlayerPositionTracker>();
        // Get XR Origin reference if not assigned
        if (xrOrigin == null)
        {
            xrOrigin = FindFirstObjectByType<XROrigin>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(Session.instance.hasInitialised && sessionGenerator.GetCurrentBlockType() != "ObjectSearch")
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
            if (xrOrigin != null)
            {
                Session.instance.CurrentTrial.settings.SetValue("object",gameObject.name);
                float distanceTravelled = playerPositionTracker.GetDistanceTravelled();
                int tileChanges  = playerPositionTracker.GetTileChanges();
        
                Session.instance.CurrentTrial.result["distance_travelled"] = distanceTravelled;
                Session.instance.CurrentTrial.result["tile_changes"] = tileChanges;
                
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

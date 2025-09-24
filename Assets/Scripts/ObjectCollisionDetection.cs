using System;
using UnityEngine;
using Unity.XR.CoreUtils;
using UXF;

public class ObjectCollisionDetection : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    private SessionGenerator sessionGenerator;
    private GameObject objectToFind;
    public static event Action OnObjectCollided;

    private void Awake()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
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
                Debug.Log($"Collision detected with object: {gameObject.name} at {DateTime.Now}");
                OnObjectCollided?.Invoke();
                Session.instance.CurrentTrial.End();
            }
            else
            {
                Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
            }
        }
    }
}

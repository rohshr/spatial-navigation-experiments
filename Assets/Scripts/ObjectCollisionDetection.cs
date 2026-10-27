using System;
using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UXF;

public class ObjectCollisionDetection : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    private SessionGenerator sessionGenerator;
    private GameObject objectToFind;
    private GameObject objectToReach;
    private bool objectFound = false;
    private bool objectReached = false;
    
    [SerializeField] private InputActionReference confirmFind;
    [SerializeField] private InputActionReference confirmReach;
    public static event Action OnObjectFound;
    public static event Action OnObjectReached;

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
        if(Session.instance.hasInitialised && sessionGenerator.GetCurrentBlockType() != "ObjectSearch" && sessionGenerator.GetCurrentBlockType() != "GuidedRoutes")
            return;

        if (sessionGenerator.GetCurrentBlockType() == "ObjectSearch")
        {
            Debug.Log("Object Search task collision");
            if (objectFound)
            {
                return; // Already processing this object, ignore duplicate triggers
            }
            
            objectToFind = sessionGenerator.GetCurrentObjectToFind();
            
            if (gameObject != objectToFind)
            {
                Debug.Log($"Encountered non-target object: {gameObject.name} at {DateTime.Now}");
                return;
            }
            
            if (other.CompareTag("Player") && Session.instance.InTrial)
            {
                // Check if the player is in the practice trial area
                if (xrOrigin != null && objectToFind != null)
                {
                    objectFound = true;
                    StartCoroutine(HandleObjectFound());
                }
                else
                {
                    Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
                }
            }
        }
        else if (sessionGenerator.GetCurrentBlockType() == "GuidedRoutes")
        {
            Debug.Log("Guided Route block");
            if (objectReached)
            {
                return; // Already processing this object, ignore duplicate triggers
            }
            
            objectToReach = sessionGenerator.GetCurrentObjectToReach();
            // Debug.Log(objectToReach.name);
            
            if (gameObject != objectToReach)
            {
                Debug.Log($"Encountered non-target object: {gameObject.name} at {DateTime.Now}");
                return;
            }
            
            if (other.CompareTag("Player") && Session.instance.InTrial)
            {
                // Check if the player is in the practice trial area
                if (xrOrigin != null && objectToReach != null)
                {
                    objectReached = true;
                    StartCoroutine(HandleObjectReached());
                }
                else
                {
                    Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
                }
            }
        }
    }
    
    private IEnumerator HandleObjectFound()
    {
        Debug.Log($"Collision detected with target object: {gameObject.name} at {DateTime.Now}");

        yield return WaitForObjectFindConfirmation();
    
        Debug.Log("Object found by participant at " + System.DateTime.Now);

        TimeSpan finalTime = GameStopwatch.StopStopwatch();
        Debug.Log($"Total exploration time: {finalTime.TotalSeconds} seconds");
        Session.instance.CurrentTrial.result["total_exploration_time"] = finalTime.TotalSeconds;

        if (FindFirstObjectByType<FloorTile>() != null)
        {
            FloorTile.FinishPosition = gameObject;
        }
        OnObjectFound?.Invoke();
        objectFound = false; // Reset flag
        Session.instance.CurrentTrial.End();
    }
    
    private IEnumerator WaitForObjectFindConfirmation()
    {
        bool inputReceived = false;
        
        System.Action<InputAction.CallbackContext> inputHandler = (context) => {
            inputReceived = true;
        };
        
        // Assume confirmFind is an InputActionReference defined elsewhere
        if (confirmFind != null)
        {
            confirmFind.action.performed += inputHandler;
        }
        
        yield return new WaitUntil(() => inputReceived);
        
        if (confirmFind != null)
        {
            confirmFind.action.performed -= inputHandler;
        }
    }
    
    private IEnumerator HandleObjectReached()
    {
        Debug.Log($"Collision detected with target object: {gameObject.name} at {DateTime.Now}");

        yield return WaitForObjectReachConfirmation();
    
        Debug.Log("Object reached by participant at " + System.DateTime.Now);

        TimeSpan finalTime = GameStopwatch.StopStopwatch();
        Debug.Log($"Total exploration time: {finalTime.TotalSeconds} seconds");
        Session.instance.CurrentTrial.result["total_exploration_time"] = finalTime.TotalSeconds;

        if (FindFirstObjectByType<FloorTile>() != null)
        {
            FloorTile.FinishPosition = gameObject;
        }
        OnObjectReached?.Invoke();
        objectReached = false; // Reset flag
        Session.instance.CurrentTrial.End();
    }
    
    private IEnumerator WaitForObjectReachConfirmation()
    {
        bool inputReceived = false;
        
        System.Action<InputAction.CallbackContext> inputHandler = (context) => {
            inputReceived = true;
        };
        
        // Assume confirmReach is an InputActionReference defined elsewhere
        if (confirmReach != null)
        {
            confirmReach.action.performed += inputHandler;
        }
        
        yield return new WaitUntil(() => inputReceived);
        
        if (confirmReach != null)
        {
            confirmReach.action.performed -= inputHandler;
        }
    }
}

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
    
    [SerializeField] private InputActionReference confirmFind;
    
    public static event Action OnObjectFound;

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
            Debug.Log($"Encountered non-target object: {gameObject.name} at {DateTime.Now}");
            return;
        }
        if (other.CompareTag("Player") && Session.instance.InTrial)
        {
            // Check if the player is in the practice trial area
            if (xrOrigin != null)
            {
                StartCoroutine(HandleObjectFound());
            }
            else
            {
                Debug.LogWarning("XROrigin is not assigned in PracticeTrialController");
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
            FloorTile.tileVisitQueue.Enqueue(gameObject);
        }
        OnObjectFound?.Invoke();
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
}

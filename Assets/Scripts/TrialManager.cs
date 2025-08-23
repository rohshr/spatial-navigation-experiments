using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    // [SerializeField] private InstructionsController instructionsController;
    [SerializeField] private GameObject XROrigin;
    [SerializeField] private GameObject UIViewpoint;
    [SerializeField] private SessionGenerator SessionGenerator;

    private List<GameObject> SpawnPointsSequence;
    private List<GameObject> ObjectSearchSequence;

    private int currentSpawnPointIndex = 0;
    private int currentObjectSearchIndex = 0;
    private GameObject pendingSpawnPoint;
    private GameObject pendingObjectSearch;
    private bool objectSearchTrialsActive = false; // Flag to check if object search trials are active
    
    private VRDialogFlowManager dialogFlowManager;
    
    private void Start()
    {
        dialogFlowManager = FindFirstObjectByType<VRDialogFlowManager>();
        SpawnPointsSequence = SessionGenerator.GetSpawnPointsSequence();
        ObjectSearchSequence = SessionGenerator.GetObjectSearchSequence();
        SetNextSpawnPoint();
    }
    
    private void OnEnable()
    {
        VRDialogFlowManager.OnDialogFlowComplete += HandleInstructionsCompleted; // Subscribe to the event when dialog flow is completed
        // VRDialogFlowManager.OnExperimentStart += HandleInstructionsCompleted; // Subscribe to the event when the experiment starts
        // ExperimenterControlScript.OnTrialSkipped += MoveToUIViewpoint; // Subscribe to the event when the session is ended
        // ObjectCollisionDetection.OnObjectCollided += MoveToUIViewpoint; // Subscribe to the event when the object collision is detected
    }

    private void OnDisable()
    {
        VRDialogFlowManager.OnDialogFlowComplete -= HandleInstructionsCompleted; // Unsubscribe from the event when dialog flow is completed
        // VRDialogFlowManager.OnExperimentStart -= HandleInstructionsCompleted; // Unsubscribe from the event when the experiment starts
        // ExperimenterControlScript.OnTrialSkipped -= MoveToUIViewpoint; // Unsubscribe from the event when the session is ended
        // ObjectCollisionDetection.OnObjectCollided -= MoveToUIViewpoint; // Unsubscribe from the event when the object collision is detected
    }

    public void SetNextSpawnPoint()
    {
        if (SpawnPointsSequence == null || SpawnPointsSequence.Count <= currentSpawnPointIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more spawn points available.");
            return;
        }

        if (!objectSearchTrialsActive)
        {
            pendingSpawnPoint = SpawnPointsSequence[currentSpawnPointIndex];
            // instructionsController.SetEnvironmentInstruction(pendingSpawnPoint.name);
        }
        else if (ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to find.");
            objectSearchTrialsActive = false; // Reset the flag when no more object search trials are available
            currentSpawnPointIndex++;
            if (SpawnPointsSequence == null || SpawnPointsSequence.Count <= currentSpawnPointIndex)
            {
                Debug.LogWarning($"[{nameof(TrialManager)}] No more spawn points available.");
                return;
            }
            pendingSpawnPoint = SpawnPointsSequence[currentSpawnPointIndex];
            // instructionsController.SetEnvironmentInstruction(pendingSpawnPoint.name);
            return;
        }

        if (pendingSpawnPoint.name == "OpenFloorSpawnPoint")
        {
            pendingObjectSearch = ObjectSearchSequence[currentObjectSearchIndex];
            // instructionsController.SetObjectSearchInstruction(pendingObjectSearch.name);
            objectSearchTrialsActive = true; // Set the flag to true when object search trials are active
            return;
        }
    }

    public void SetNextObjectSearch()
    {
        if(ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to search.");
            return;
        }

        pendingObjectSearch = ObjectSearchSequence[currentObjectSearchIndex];
        // instructionsController.SetObjectSearchInstruction(pendingObjectSearch.name);
    }

    private void HandleInstructionsCompleted()
    {
        SetNextSpawnPoint();
        if (pendingSpawnPoint == null)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No pending spawn point to move to.");
            return;
        }
        
        if (pendingSpawnPoint.name != "OpenFloorSpawnPoint")
        {
            MoveToSpawnPoint(pendingSpawnPoint);
            currentSpawnPointIndex++;
            pendingSpawnPoint = null;
        }
        else
        {
            if (ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex)
            {
                Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to search.");
                MoveToSpawnPoint(pendingSpawnPoint);
                currentSpawnPointIndex++;
                pendingSpawnPoint = null;
                objectSearchTrialsActive = false; // Reset the flag when no more object search trials are available
                return;
            }
            MoveToSpawnPoint(pendingSpawnPoint);
            currentObjectSearchIndex++;
            pendingObjectSearch = null;
        }
    }

    // Method to handle specific dialog completions
    // private void HandleSpecificDialogComplete(string dialogKey)
    // {
    //     Debug.Log($"Specific dialog completed: {dialogKey}");
    //     
    //     // Handle finish point completion dialogs (like "TrialComplete", "CurvedComplete", etc.)
    //     if (IsFinishPointCompletionDialog(dialogKey))
    //     {
    //         // Continue to next dialog in sequence (next trial instructions)
    //         if (dialogFlowManager != null)
    //         {
    //             dialogFlowManager.ContinueToNextDialog();
    //         }
    //         return;
    //     }
    //
    //     // Handle environment and object search instructions
    //     if (!IsEnvironmentInstruction(dialogKey) && !IsObjectSearchInstruction(dialogKey)) return;
    //     
    //     // Trigger the same logic as HandleInstructionsCompleted
    //     HandleInstructionsCompleted();
    //         
    //     // Pause the dialog flow - player will be moved to environment
    //     if (dialogFlowManager != null)
    //     {
    //         dialogFlowManager.PauseDialogFlow();
    //     }
    //
    // }
    //
    // Helper methods to identify dialog types
    private bool IsEnvironmentInstruction(string dialogKey)
    {
        string[] environmentDialogs = {
            "CurvedEnvironmentInstructions",
            "AngledEnvironmentInstructions", 
            "OpenEnvironmentInstructions"
        };
    
        return System.Array.Exists(environmentDialogs, dialog => dialog == dialogKey);
    }

    private bool IsObjectSearchInstruction(string dialogKey)
    {
        string[] objectDialogs = {
            "OpenObjectCube",
            "OpenObjectSphere", 
            "OpenObjectStar",
            "OpenObjectStatue"
        };
    
        return System.Array.Exists(objectDialogs, dialog => dialog == dialogKey);
    }
    
    // Helper method to identify finish point completion dialogs
    private bool IsFinishPointCompletionDialog(string dialogKey)
    {
        string[] completionDialogs = {
            "CurvedComplete",
            "AngledComplete",
            "OpenComplete",
            "TrialComplete",
            "ObjectFound",
            // Add other completion dialog keys here
        };

        return System.Array.Exists(completionDialogs, dialog => dialog == dialogKey);
    }

    
    // This should be called from UXF's OnTrialEnd event or when trial is actually complete
    public void OnTrialCompleted()
    {
        Debug.Log("Trial completed, continuing dialog flow");
    }
    
    private void MoveToSpawnPoint(GameObject spawnPoint)
    {
        XROrigin.transform.SetPositionAndRotation(
            spawnPoint.transform.position,
            spawnPoint.transform.rotation
        );
        Debug.Log($"Moved to spawn point: {spawnPoint.name}");
    }

    // public void MoveToUIViewpoint()
    // {
    //     if (UIViewpoint != null)
    //     {
    //         XROrigin.transform.SetPositionAndRotation(
    //             UIViewpoint.transform.position,
    //             UIViewpoint.transform.rotation
    //         );
    //         Debug.Log($"Moved to UIViewpoint: {UIViewpoint.name}");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"[{nameof(TrialManager)}] UIViewpoint is not assigned.");
    //     }
    // }
}

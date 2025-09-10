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
    private SessionGenerator sessionGenerator;
    
    // private List<GameObject> SpawnPointsSequence;
    private List<GameObject> ObjectSearchSequence;

    private List<LocomotionExperimentBlock> currentTrialBlocks;
    private string nextBlockType;
    private int currentBlockIndex = 0;
    private LocomotionExperimentBlock currentBlock;
    private GameObject currentSpawnPoint;
    private int nextSpawnPointIndex = 0;
    private int objectSearchIndex = 0;
    private GameObject nextSpawnPoint;
    private GameObject nextObjectSearch;
    // private bool objectSearchTrialsActive = false; // Flag to check if object search trials are active
    private VRDialogFlowManager dialogFlowManager;
    
    // Events
    public static event Action OnBlocksCompleted;
    public static event Action OnExplorationBlockCompleted;
    
    private void Start()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
        currentTrialBlocks = sessionGenerator.GetExperimentBlocks();
        currentBlock = currentTrialBlocks[currentBlockIndex];
        dialogFlowManager = FindFirstObjectByType<VRDialogFlowManager>();
        // SpawnPointsSequence = SessionGenerator.GetSpawnPointsSequence();
        // ObjectSearchSequence = SessionGenerator.GetObjectSearchSequence();
        // SetNextSpawnPoint();
        SetSpawnPoint(currentBlock);
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

    // public void SetNextSpawnPoint()
    // {
    //     if (SpawnPointsSequence == null || SpawnPointsSequence.Count <= nextSpawnPointIndex)
    //     {
    //         Debug.LogWarning($"[{nameof(TrialManager)}] No more spawn points available.");
    //         return;
    //     }
    //
    //     if (!objectSearchTrialsActive)
    //     {
    //         nextSpawnPoint = SpawnPointsSequence[nextSpawnPointIndex];
    //         // instructionsController.SetEnvironmentInstruction(pendingSpawnPoint.name);
    //     }
    //     else if (ObjectSearchSequence == null || ObjectSearchSequence.Count <= nextObjectSearchIndex)
    //     {
    //         Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to find.");
    //         objectSearchTrialsActive = false; // Reset the flag when no more object search trials are available
    //         nextSpawnPointIndex++;
    //         if (SpawnPointsSequence == null || SpawnPointsSequence.Count <= nextSpawnPointIndex)
    //         {
    //             Debug.LogWarning($"[{nameof(TrialManager)}] No more spawn points available.");
    //             return;
    //         }
    //         nextSpawnPoint = SpawnPointsSequence[nextSpawnPointIndex];
    //         // instructionsController.SetEnvironmentInstruction(pendingSpawnPoint.name);
    //         return;
    //     }
    //
    //     if (nextSpawnPoint.name == "OpenFloorSpawnPoint")
    //     {
    //         SetNextObjectSearch();
    //         // instructionsController.SetObjectSearchInstruction(pendingObjectSearch.name);
    //         objectSearchTrialsActive = true; // Set the flag to true when object search trials are active
    //         return;
    //     }
    // }
    
    /// <summary>
    /// Get the spawn point from the current block and set it as the current spawn point. Run at the start of the session and after each block ends.
    /// </summary>
    public void SetSpawnPoint(LocomotionExperimentBlock block)
    {
        if (block.GetSpawnPoint() == null)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}]: The spawn point for block {currentBlock.blockName} is null.");
            return;
        }
        currentSpawnPoint = block.GetSpawnPoint();
    }

    /// <summary>
    /// Setup next block and spawn point for the block. Assign in UXF Rig On Block End event.
    /// </summary>
    public void SetupNextBlock()
    {
        currentBlockIndex++;
        if (currentBlockIndex < currentTrialBlocks.Count)
        {
            currentBlock = currentTrialBlocks[currentBlockIndex];
            if (currentBlock?.GetBlockType() == "ObjectSearch")
            {
                // objectSearchTrialsActive = true;
                objectSearchIndex = 0; // Reset the object search index for new object search block
            }

            if (currentBlock?.GetBlockType() == "TimedExploration")
            {
                var timeTrialBlock = currentBlock as TimedExplorationBlock;
                var timeForExploration = timeTrialBlock.GetTimeForExplorationInSeconds();
                Debug.Log($"Time for exploration: {timeForExploration} seconds.");
                StartCoroutine(EndTrialAfterDelay(timeForExploration)); // Start the timer for the exploration block (after the object search block)
            }
            SetSpawnPoint(currentBlock);
        }
        else
        {
            Debug.Log("No more blocks available.");
            OnBlocksCompleted?.Invoke(); // Trigger the event to notify that all blocks are completed
        }
    }

    public void SetNextObjectSearch()
    {
        if(ObjectSearchSequence == null || ObjectSearchSequence.Count <= objectSearchIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to search.");
            return;
        }

        nextObjectSearch = ObjectSearchSequence[objectSearchIndex];
        // instructionsController.SetObjectSearchInstruction(pendingObjectSearch.name);
    }

    public void InstantiateExplorationTrial()
    {
        if (currentBlock?.GetBlockType() == "TimedExploration")
        {
            var timeTrialBlock = currentBlock as TimedExplorationBlock;
            var timeForExploration = timeTrialBlock.GetTimeForExplorationInSeconds();
            Debug.Log($"Time for exploration: {timeForExploration} seconds.");
            StartCoroutine(EndTrialAfterDelay(timeForExploration)); // Start the timer for the exploration block (after the object search block)
        }
        
        if (currentBlock?.GetBlockType() == "GuidedExploration")
        {
            var guidedBlock = currentBlock as GuidedExplorationBlock;
            guidedBlock?.EnableNavigationGuides();
        }
    }

    public void SetupNextTrial()
    {
        
    }

    private void HandleInstructionsCompleted()
    {
        SetSpawnPoint(currentBlock);
        MoveToSpawnPoint(currentSpawnPoint);
        InstantiateExplorationTrial();
    }

    private IEnumerator EndTrialAfterDelay(float timeInSeconds)
    {
        Debug.Log($"Ending trial after {timeInSeconds} seconds.");
        yield return new WaitForSeconds(timeInSeconds);
        SetupNextBlock();
        OnExplorationBlockCompleted?.Invoke(); // Trigger the event to notify that the exploration block is completed
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
        nextSpawnPointIndex++;
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

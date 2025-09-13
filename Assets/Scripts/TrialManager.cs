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
    private GameObject nextSpawnPoint;
    private GameObject nextObjectSearch;
    
    // Events
    public static event Action OnBlocksCompleted;
    public static event Action OnExplorationBlockCompleted;
    
    private void Start()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
        currentTrialBlocks = sessionGenerator.GetExperimentBlocks();
        currentBlock = currentTrialBlocks[currentBlockIndex];
        SetSpawnPoint(currentBlock);
    }
    
    private void OnEnable()
    {
        VRDialogFlowManager.OnDialogFlowComplete += HandleInstructionsCompleted; // Subscribe to the event when dialog flow is completed
    }

    private void OnDisable()
    {
        VRDialogFlowManager.OnDialogFlowComplete -= HandleInstructionsCompleted; // Unsubscribe from the event when dialog flow is completed
    }
    
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

    private void HandleInstructionsCompleted()
    {
        // Determine the appropriate spawn point based on block type
        GameObject spawnPoint = GetAppropriateSpawnPoint();
        MoveToSpawnPoint(spawnPoint);
        InstantiateExplorationTrial();
    }
    
    /// <summary>
    /// Get the appropriate spawn point based on the current block type and trial
    /// </summary>
    /// <returns>GameObject representing the spawn point to move to</returns>
    private GameObject GetAppropriateSpawnPoint()
    {
        if (Session.instance == null || !Session.instance.hasInitialised)
        {
            Debug.Log("Session not ready, using current block spawn point");
            return currentSpawnPoint;
        }
        if (currentBlock?.GetBlockType() == "ObjectSearch")
        {
            // For object search blocks, try to get the task-specific spawn location
            var objectSearchSpawnPoint = sessionGenerator.GetCurrentObjectSearchSpawnLocation(currentBlock);
            if (objectSearchSpawnPoint != null)
            {
                Debug.Log($"Using object search task spawn location: {objectSearchSpawnPoint.name}");
                return objectSearchSpawnPoint;
            }
            else
            {
                Debug.Log($"No task-specific spawn location found, using block spawn point: {currentSpawnPoint.name}");
                return currentSpawnPoint;
            }
        }
        
        // For other block types, use the standard block spawn point
        return currentSpawnPoint;
    }


    private IEnumerator EndTrialAfterDelay(float timeInSeconds)
    {
        Debug.Log($"Ending trial after {timeInSeconds} seconds.");
        yield return new WaitForSeconds(timeInSeconds);
        SetupNextBlock();
        OnExplorationBlockCompleted?.Invoke(); // Trigger the event to notify that the exploration block is completed
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

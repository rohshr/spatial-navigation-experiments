using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UXF;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    // [SerializeField] private InstructionsController instructionsController;
    [SerializeField] private GameObject XROrigin;
    [SerializeField] private GameObject TeleportationHandler;
    [SerializeField] private GameObject UIViewpoint;
    
    private SessionGenerator sessionGenerator;

    private List<LocomotionExperimentBlock> currentTrialBlocks;
    private string nextBlockType;
    private int currentBlockIndex = 0;
    private LocomotionExperimentBlock currentBlock;
    private GameObject currentSpawnPoint;
    
    private TeleportationProvider teleportationProvider;
    private GameObject teleportInteractor;
    
    // Events
    public static event Action OnBlocksCompleted;
    public static event Action OnExplorationBlockCompleted;
    
    private void Start()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>(); 
        currentTrialBlocks = sessionGenerator.GetExperimentBlocks();
        currentBlock = currentTrialBlocks[currentBlockIndex];
        SetSpawnPoint(currentBlock);
        
        // Cache the teleportation provider reference
        if (TeleportationHandler == null)
        {
            Debug.LogError("TeleportationHandler is not assigned in the inspector.");
            return;
        }

        teleportationProvider = TeleportationHandler.GetComponent<TeleportationProvider>();
        if (teleportationProvider == null)
        {
            Debug.LogWarning("TeleportationProvider not found in scene");
        }
        
        var leftController = InputHandler.GetLeftHandController();
        if (leftController != null)
        {
            teleportInteractor = leftController.transform.Find("Teleport Interactor")?.gameObject;
        }    
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
        if (currentBlockIndex < Session.instance.blocks.Count)
        {
            currentBlock = currentTrialBlocks[currentBlockIndex];

            // if (currentBlock?.GetBlockType() == "TimedExploration")
            // {
            //     var timeTrialBlock = currentBlock as TimedExplorationBlock;
            //     var timeForExploration = timeTrialBlock.GetTimeForExplorationInSeconds();
            //     Debug.Log($"Time for exploration: {timeForExploration} seconds.");
            //     StartCoroutine(EndTrialAfterDelay(timeForExploration)); // Start the timer for the exploration block (after the object search block)
            // }
            SetSpawnPoint(currentBlock);
        }
        else
        {
            Debug.Log("No more blocks available.");
            OnBlocksCompleted?.Invoke(); // Trigger the event to notify that all blocks are completed
        }
    }

    private void InstantiateExplorationTrial()
    {
        if (currentBlock?.GetBlockType() == "TimedExploration")
        {
            var timeTrialBlock = currentBlock as TimedExplorationBlock;

            if (timeTrialBlock == null)
            {
                Debug.LogError("Time trial block not initialized.");
                return;
            }
            var timeForExploration = timeTrialBlock.GetTimeForExplorationInSeconds();
            Debug.Log("Starting timed exploration...");
            Debug.Log($"Time for exploration: {timeForExploration} seconds.");
            Session.instance.BeginNextTrial();
            StartCoroutine(EndTrialAfterDelay(timeForExploration)); // Start the timer for the exploration block (after the object search block)
        }
        
        if (currentBlock?.GetBlockType() == "GuidedExploration")
        {
            var guidedBlock = currentBlock as GuidedExplorationBlock;
            guidedBlock?.EnableNavigationGuides();
        }
    }
    
    /// <summary>
    /// Cancels any ongoing teleportation movement
    /// </summary>
    public void CancelOngoingMovement()
    {
        if (!teleportationProvider)
        {
            Debug.LogWarning("TeleportationProvider reference is missing, cannot cancel ongoing movement.");
            return;
        }

        // Disable the teleportation provider to interrupt any ongoing teleportation
        teleportationProvider.enabled = false;
    
        // Wait a frame before re-enabling to ensure the teleportation state is reset
        StartCoroutine(ReenableTeleportationProvider());
        
        // Disable teleport interactor in left hand controller
        if (teleportInteractor)
        {
            teleportInteractor.SetActive(false);
            StartCoroutine(ReenableInteractor());
        }
        else
        {
            Debug.LogWarning("Teleport Interactor not found in left hand controller.");
        }
        
        Debug.Log("Canceled incomplete teleportation");

        // // Also disable and re-enable locomotion components to reset state
        // var locomotionProviders = XROrigin.GetComponentsInChildren<LocomotionProvider>();
        // foreach (var provider in locomotionProviders)
        // {
        //     provider.enabled = false;
        //     provider.enabled = true;
        // }
    }
    
    private IEnumerator ReenableTeleportationProvider()
    {
        yield return null; // Wait one frame
        if (teleportationProvider != null)
        {
            teleportationProvider.enabled = true;
        }
    }
    
    private IEnumerator ReenableInteractor()
    {
        yield return null;
        if (teleportInteractor != null)
        {
            teleportInteractor.SetActive(true);
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
            Debug.LogError("Session instance is not initialized.");
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
        
        CancelOngoingMovement();
        OnExplorationBlockCompleted?.Invoke(); // Trigger the event to notify that the exploration block is completed
        Session.instance.CurrentTrial.End();
    }
    
    // This should be called from UXF's OnTrialEnd event or when trial is actually complete
    public void OnTrialCompleted()
    {
        Debug.Log("Trial completed, continuing dialog flow");
        CancelOngoingMovement();
    }
    
    private void MoveToSpawnPoint(GameObject spawnPoint)
    {
        StartCoroutine(MoveToSpawnPointCoroutine(spawnPoint));
    }
    
    private IEnumerator MoveToSpawnPointCoroutine(GameObject spawnPoint)
    {
        // Wait for end of frame to ensure physics has settled
        yield return new WaitForEndOfFrame();
    
        // Disable character controller if present
        var characterController = XROrigin.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
    
        // Set position and rotation
        XROrigin.transform.SetPositionAndRotation(
            spawnPoint.transform.position,
            spawnPoint.transform.rotation
        );
    
        // Re-enable character controller
        if (characterController != null)
        {
            yield return null; // Wait one frame
            characterController.enabled = true;
        }
    
        Debug.Log($"Moved to spawn point: {spawnPoint.name} at position {spawnPoint.transform.position}");
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

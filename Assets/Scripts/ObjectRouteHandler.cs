using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ObjectRouteHandler
{
    private int objectRouteIndex = 0;
    private GameObject objectToReach;
    
    public GameObject CurrentObjectToReach => objectToReach;
    
    /// <summary>
    /// Get initial object route instructions for the first block if it's an ObjectRoute block
    /// </summary>
    public List<GameObject> GetInitialObjectRouteInstructions(LocomotionExperimentBlock block)
    {
        var instructions = new List<GameObject>();
        
        if (!IsObjectRouteBlock(block, out var objectRouteBlock))
            return instructions;
            
        objectRouteIndex = 0;
        var firstTask = GetObjectGuidedRoute(objectRouteBlock, objectRouteIndex);
        
        if (firstTask != null)
        {
            objectToReach = firstTask.objectToReach;

            if (firstTask.taskInstructionsDialogPrefab != null)
                instructions.Add(firstTask.taskInstructionsDialogPrefab);
        }
        
        return instructions;
    }
    
    /// <summary>
    /// Get instructions for the next object route task within the current block
    /// </summary>
    public (List<GameObject> instructions, bool isSessionComplete) GetNextObjectRouteInstructions(List<LocomotionExperimentBlock> experimentBlocks)
    {
        var instructions = new List<GameObject>();
        bool isSessionComplete = false;
        
        if (!TryGetCurrentBlock(experimentBlocks, out var currentBlock) ||
            !IsObjectRouteBlock(currentBlock, out var objectRouteBlock))
        {
            return (instructions, true); // Session should end if we can't get current block
        }
        
        // Add completion message for previous task
        var previousTask = GetObjectGuidedRoute(objectRouteBlock, objectRouteIndex);
        if (previousTask?.taskCompleteMessageDialogPrefab != null)
        {
            instructions.Add(previousTask.taskCompleteMessageDialogPrefab);
        }
        
        // Move to next task
        var nextTaskIndex = ++objectRouteIndex;
        
        if (IsValidTaskIndex(objectRouteBlock, nextTaskIndex))
        {
            // More tasks in current block
            var nextTask = GetObjectGuidedRoute(objectRouteBlock, nextTaskIndex);
            if (nextTask != null)
            {
                objectToReach = nextTask.objectToReach;
                
                if (nextTask?.taskInstructionsDialogPrefab != null)
                {
                    instructions.Add(nextTask.taskInstructionsDialogPrefab);
                }
            }
        }
        else
        {
            // CurrentBlock.number is 1-based, so it equals the next block's 0-based index.
            isSessionComplete = Session.instance.CurrentBlock.number >= experimentBlocks.Count;
        }
        
        return (instructions, isSessionComplete);
    }
    
    /// <summary>
    /// Get the spawn location for the current object route task using a specific block
    /// </summary>
    public GameObject GetCurrentObjectRouteSpawnLocation(LocomotionExperimentBlock block)
    {
        if (!IsObjectRouteBlock(block, out var objectRouteBlock))
        {
            return null;
        }
        
        var currentTask = GetObjectGuidedRoute(objectRouteBlock, objectRouteIndex);
        
        // Return task-specific spawn location if set, otherwise return block's spawn point
        return currentTask?.objectGuidedRouteStartLocation ?? block.environmentSpawnPoint;
    }
    
    /// <summary>
    /// Reset the handler for a new session
    /// </summary>
    public void Reset()
    {
        objectRouteIndex = 0;
        objectToReach = null;
    }

    #region Private Helper Methods
    
    private bool IsObjectRouteBlock(LocomotionExperimentBlock block, out GuidedRouteBlock objectRouteBlock)
    {
        objectRouteBlock = block as GuidedRouteBlock;
        return objectRouteBlock != null && objectRouteBlock.GetBlockType() == "GuidedRoutes";
    }
    
    private bool TryGetCurrentBlock(List<LocomotionExperimentBlock> experimentBlocks, out LocomotionExperimentBlock currentBlock)
    {
        currentBlock = null;
        
        if (Session.instance == null || experimentBlocks == null)
            return false;
        
        if (!Session.instance.hasInitialised)
        {
            Debug.LogWarning("Session not fully initialized or no blocks created yet.");
            return false;
        }

        var blockIndex = Session.instance.CurrentBlock.number - 1;
        if (blockIndex < 0 || blockIndex >= experimentBlocks.Count)
        {
            Debug.LogWarning($"CurrentBlock.number {Session.instance.CurrentBlock.number} is out of bounds for experimentBlocks (Count: {experimentBlocks.Count}).");
            return false;
        }
        
        currentBlock = experimentBlocks[blockIndex];
        return currentBlock != null;
    }
    
    private ObjectGuidedRoute GetObjectGuidedRoute(GuidedRouteBlock block, int index)
    {
        if (block?.objectGuidedRoutes == null || index < 0 || index >= block.objectGuidedRoutes.Count)
            return null;

        return block.objectGuidedRoutes[index];
    }
    
    private bool IsValidTaskIndex(GuidedRouteBlock block, int index)
    {
        return block?.objectGuidedRoutes != null && index >= 0 && index < block.objectGuidedRoutes.Count;
    }
    
    #endregion
}

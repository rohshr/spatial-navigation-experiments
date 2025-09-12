using System.Collections.Generic;
using UnityEngine;
using UXF;

public class ObjectSearchHandler
{
    private int objectSearchIndex = 0;
    private GameObject objectToFind;
    
    public GameObject CurrentObjectToFind => objectToFind;
    
    /// <summary>
    /// Get initial object search instructions for the first block if it's an ObjectSearch block
    /// </summary>
    public List<GameObject> GetInitialObjectSearchInstructions(LocomotionExperimentBlock block)
    {
        var instructions = new List<GameObject>();
        
        if (!IsObjectSearchBlock(block, out var objectSearchBlock))
            return instructions;
            
        objectSearchIndex = 0;
        var firstTask = GetObjectSearchTask(objectSearchBlock, objectSearchIndex);
        
        if (firstTask != null && firstTask?.taskInstructionsDialogPrefab != null)
        {
            objectToFind = firstTask.objectToFind;
            instructions.Add(firstTask.taskInstructionsDialogPrefab);
        }
        
        return instructions;
    }
    
    /// <summary>
    /// Get instructions for the next object search task within the current block
    /// </summary>
    public (List<GameObject> instructions, bool isSessionComplete) GetNextObjectSearchInstructions(List<LocomotionExperimentBlock> experimentBlocks)
    {
        var instructions = new List<GameObject>();
        bool isSessionComplete = false;
        
        if (!TryGetCurrentBlock(experimentBlocks, out var currentBlock) || 
            !IsObjectSearchBlock(currentBlock, out var objectSearchBlock))
        {
            return (instructions, true); // Session should end if we can't get current block
        }
        
        // Add completion message for previous task
        var previousTask = GetObjectSearchTask(objectSearchBlock, objectSearchIndex);
        if (previousTask?.taskCompleteMessageDialogPrefab != null)
        {
            instructions.Add(previousTask.taskCompleteMessageDialogPrefab);
        }
        
        // Move to next task
        var nextTaskIndex = ++objectSearchIndex;
        
        if (IsValidTaskIndex(objectSearchBlock, nextTaskIndex))
        {
            // More tasks in current block
            var nextTask = GetObjectSearchTask(objectSearchBlock, nextTaskIndex);
            if (nextTask?.taskInstructionsDialogPrefab != null)
            {
                instructions.Add(nextTask.taskInstructionsDialogPrefab);
                objectToFind = nextTask.objectToFind;
            }
        }
        else
        {
            // No more tasks in current block, check for next block
            var currentBlockIndex = Session.instance.CurrentBlock.number - 1;
            var nextBlockIndex = currentBlockIndex + 1;
            
            if (nextBlockIndex < experimentBlocks.Count)
            {
                // There is a next block
                var nextBlock = experimentBlocks[nextBlockIndex];
                if (nextBlock?.startMessageDialogPrefab != null)
                {
                    instructions.Add(nextBlock.startMessageDialogPrefab);
                }
            }
            else
            {
                // No more blocks, session should end
                isSessionComplete = true;
            }
        }
        
        return (instructions, isSessionComplete);
    }
    
    /// <summary>
    /// Reset the handler for a new session
    /// </summary>
    public void Reset()
    {
        objectSearchIndex = 0;
        objectToFind = null;
    }

    #region Private Helper Methods
    
    private bool IsObjectSearchBlock(LocomotionExperimentBlock block, out ObjectSearchBlock objectSearchBlock)
    {
        objectSearchBlock = block as ObjectSearchBlock;
        return objectSearchBlock != null && objectSearchBlock.GetBlockType() == "ObjectSearch";
    }
    
    private bool TryGetCurrentBlock(List<LocomotionExperimentBlock> experimentBlocks, out LocomotionExperimentBlock currentBlock)
    {
        currentBlock = null;
        
        if (Session.instance == null || experimentBlocks == null)
            return false;
            
        var blockIndex = Session.instance.CurrentBlock.number - 1;
        if (blockIndex < 0 || blockIndex >= experimentBlocks.Count)
        {
            Debug.LogWarning($"CurrentBlock.number {Session.instance.CurrentBlock.number} is out of bounds for experimentBlocks (Count: {experimentBlocks.Count}).");
            return false;
        }
        
        currentBlock = experimentBlocks[blockIndex];
        return currentBlock != null;
    }
    
    private ObjectSearchTask GetObjectSearchTask(ObjectSearchBlock block, int index)
    {
        if (block?.objectSearchTasks == null || index < 0 || index >= block.objectSearchTasks.Count)
            return null;
            
        return block.objectSearchTasks[index];
    }
    
    private bool IsValidTaskIndex(ObjectSearchBlock block, int index)
    {
        return block?.objectSearchTasks != null && index >= 0 && index < block.objectSearchTasks.Count;
    }
    
    private List<GameObject> GetNextBlockStartInstructions(List<LocomotionExperimentBlock> experimentBlocks)
    {
        var instructions = new List<GameObject>();
        var nextBlockIndex = Session.instance.CurrentBlock.number; // 1-based index
        
        if (nextBlockIndex < experimentBlocks.Count)
        {
            var nextBlock = experimentBlocks[nextBlockIndex];
            if (nextBlock?.startMessageDialogPrefab != null)
            {
                instructions.Add(nextBlock.startMessageDialogPrefab);
            }
        }
        
        return instructions;
    }
    
    #endregion
}

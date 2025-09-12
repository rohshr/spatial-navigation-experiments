using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The type of environment for the block
public enum EnvironmentType { Curved, Angled, OpenSpace, Maze }
        
[System.Serializable]
public class TrialTask
{
    public string taskName;
    public GameObject taskInstructionsDialogPrefab;
    public GameObject taskCompleteMessageDialogPrefab;
}
    
[System.Serializable]
public class ObjectSearchTask : TrialTask
{
    [Tooltip("Location to start object search from")]   
    public GameObject objectSearchStartLocation;
    [Tooltip("Object to be found")]
    public GameObject objectToFind; // Reference to the object to be found
}
    
/// <summary>
/// Generic locomotion experiment block class. Specific block types (e.g., ObjectSearchBlock, ExplorationBlock) will inherit from this class.
/// </summary>
[System.Serializable]
public class LocomotionExperimentBlock
{
    [Tooltip("Unique name for Locomotion Experiment Block")]
    public string blockName;
            
    [Header("Environment Configuration")]
    [Space(5)]
    [Tooltip("Type of environment for the block")]
    public EnvironmentType environment;
    [Tooltip("Reference to the environment spawn point")]
    public GameObject environmentSpawnPoint; // Reference to the environment spawn point
    [Tooltip("Reference to the environment finish point. Not applicable for tasks with multiple possible end points, like the object search task.")]
    public GameObject environmentFinishPoint; // Reference to the environment finish point
            
    [Header("Block Instructions Configuration")]
    [Space(5)]
    [Tooltip("Dialog prefab to show at the start of block")]
    public GameObject startMessageDialogPrefab;
    [Tooltip("Dialog prefab to show at the end of block")]
    public GameObject endMessageDialogPrefab;
            
    // [Header("Trial Settings")]
    // [Space(5)]
    // [Tooltip("Enable this to randomize the order of trial tasks within the block. Only applicable if multiple tasks are defined.")]
    // public bool randomizeTrialTasksSequence = false;
            
    public virtual int GetTrialCount() => 1;
    public virtual string GetBlockType() => "Generic";
    public virtual string GetEnvironmentType() => environment.ToString();
    public virtual GameObject GetSpawnPoint() => environmentSpawnPoint;
}
    
[System.Serializable]
public class ObjectSearchBlock : LocomotionExperimentBlock
{
    [Header("Object Search Configuration")]
    [Tooltip("List of object search tasks to include in the block. Each task should specify the object to find and associated instructions.")]
    public List<ObjectSearchTask> objectSearchTasks = new List<ObjectSearchTask>();
        
    public override int GetTrialCount() => objectSearchTasks.Count;
    public override string GetBlockType() => "ObjectSearch";
        
    /// <summary>
    /// Get the instruction sequence for an object searches in an object search block.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetObjectSearchSequence()
    {
        var objectSearchSequence = new List<GameObject>();
    
        objectSearchSequence.AddRange(
            objectSearchTasks
                .Where(task => task.objectToFind != null)
                .Select(task => task.objectToFind)
        );
    
        return objectSearchSequence;
    }
}
    
[System.Serializable]
public class TimedExplorationBlock : LocomotionExperimentBlock
{
    [Tooltip("Time to allow exploration in minutes")]
    public float timeForExploration = 5f; // Default to 5 minutes
        
    public override string GetBlockType() => "TimedExploration";
    public float GetTimeForExplorationInSeconds() => (timeForExploration * 60f);
}

[System.Serializable]
public class GuidedExplorationBlock : LocomotionExperimentBlock
{
    [Header("Guided Exploration Configuration")]
    [Tooltip("Reference to the environment navigation guides (e.g., arrows, lights) to assist the participant in navigation. Assign the parent GameObject which contains all the guides as children.")]
    public GameObject environmentNavigationGuides;
    public override string GetBlockType() => "GuidedExploration";
    
    /// <summary>
    /// Enable the finish point when the player exits the spawn point.
    /// </summary>
    public void EnableFinishPoint()
    {
        Debug.Log("Enabling finish point...");
        if (environmentFinishPoint != null)
            environmentFinishPoint.SetActive(true);
    }

    public void DisableFinishPoint()
    {
        Debug.Log("Disabling finish point...");
        if (environmentFinishPoint != null)
            environmentFinishPoint.SetActive(false);
    }
    
    public void EnableNavigationGuides()
    {
        if (environmentNavigationGuides != null)
            environmentNavigationGuides.SetActive(true);
    }
    
    public void DisableNavigationGuides()
    {
        if (environmentNavigationGuides != null)
            environmentNavigationGuides.SetActive(false);
    }
}
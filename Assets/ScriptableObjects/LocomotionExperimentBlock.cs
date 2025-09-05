// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// // The type of environment for the block
// public enum EnvironmentType { Curved, Angled, OpenSpace, Maze }
//         
// [System.Serializable]
// public class TrialTask
// {
//     public string taskName;
//     public GameObject taskInstructionsDialogPrefab;
//     public GameObject taskCompleteMessageDialogPrefab;
// }
//     
// [System.Serializable]
// public class ObjectSearchTask : TrialTask
// {
//     [Tooltip("Object to be found")]
//     public string objectToFindID; // String ID reference to the object to be found
// }
//     
// /// <summary>
// /// Generic locomotion experiment block class. Specific block types (e.g., ObjectSearchBlock, ExplorationBlock) will inherit from this class.
// /// </summary>
// [CreateAssetMenu(fileName = "New Locomotion Block", menuName = "Experiment Blocks/Generic Block")]
// public class LocomotionExperimentBlock : ScriptableObject
// {
//     [Tooltip("Unique name for Locomotion Experiment Block")]
//     public string blockName;
//             
//     [Header("Environment Configuration")]
//     [Space(5)]
//     [Tooltip("Type of environment for the block")]
//     public EnvironmentType environment;
//     [Tooltip("Reference to the environment spawn point")]
//     public string environmentSpawnPointID; // String ID reference to the environment spawn point
//     [Tooltip("Reference to the environment finish point. Not applicable for tasks with multiple possible end points, like the object search task.")]
//     public string environmentFinishPointID; // String ID reference to the environment finish point
//             
//     [Header("Block Instructions Configuration")]
//     [Space(5)]
//     [Tooltip("Dialog prefab to show at the start of block")]
//     public GameObject startMessageDialogPrefab;
//     [Tooltip("Dialog prefab to show at the end of block")]
//     public GameObject endMessageDialogPrefab;
//             
//     // [Header("Trial Settings")]
//     // [Space(5)]
//     // [Tooltip("Enable this to randomize the order of trial tasks within the block. Only applicable if multiple tasks are defined.")]
//     // public bool randomizeTrialTasksSequence = false;
//             
//     public virtual int GetTrialCount() => 1;
//     public virtual string GetBlockType() => "Generic";
//     
//     // Runtime method to get spawn point by ID
//     public virtual GameObject GetSpawnPoint()
//     {
//         if (string.IsNullOrEmpty(environmentSpawnPointID))
//         {
//             Debug.LogWarning($"[{nameof(SessionGenerator)}]: The spawn point for block {blockName} is null.");
//             return null;
//         }
//         return SceneObjectRegistry.Instance?.GetObject(environmentSpawnPointID);
//     }
//     
//     public virtual GameObject GetFinishPoint()
//     {
//         if (string.IsNullOrEmpty(environmentFinishPointID))
//         {
//             Debug.LogWarning($"[{nameof(SessionGenerator)}]: The finish point for block {blockName} is null.");
//             return null;
//         }
//         return SceneObjectRegistry.Instance?.GetObject(environmentFinishPointID);
//     }
// }
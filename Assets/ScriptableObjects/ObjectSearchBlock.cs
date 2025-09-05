// using System.Collections.Generic;
// using UnityEngine;
//
// /// <summary>
// /// Block for object search
// /// </summary>
// [CreateAssetMenu(fileName = "New Object Search Block", menuName = "Experiment Blocks/Object Search Block")]
// public class ObjectSearchBlock : LocomotionExperimentBlock
// {
//     [Header("Object Search Configuration")]
//     [Tooltip("List of object search tasks to include in the block. Each task should specify the object to find and associated instructions.")]
//     public List<ObjectSearchTask> objectSearchTasks = new List<ObjectSearchTask>();
//         
//     public override int GetTrialCount() => objectSearchTasks.Count;
//     public override string GetBlockType() => "ObjectSearch";
//         
//     /// <summary>
//     /// Get the instructions sequence for an object searches in an object search block.
//     /// </summary>
//     /// <returns></returns>
//     public List<GameObject> GetObjectSearchSequence()
//     {
//         var objectSearchSequence = new List<GameObject>();
//     
//         foreach (var task in objectSearchTasks)
//         {
//             if (!string.IsNullOrEmpty(task.objectToFindID))
//             {
//                 var gameObject = SceneObjectRegistry.Instance?.GetObject(task.objectToFindID);
//                 if (gameObject != null)
//                 {
//                     objectSearchSequence.Add(gameObject);
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"Could not find GameObject with ID: {task.objectToFindID}");
//                 }
//             }
//         }
//         return objectSearchSequence;
//     }
//     
//     /// <summary>
//     /// Get a specific object to find by index
//     /// </summary>
//     /// <param name="index"></param>
//     /// <returns></returns>
//     public GameObject GetObjectToFind(int index)
//     {
//         if (index < 0 || index >= objectSearchTasks.Count)
//             return null;
//             
//         var task = objectSearchTasks[index];
//         if (string.IsNullOrEmpty(task.objectToFindID))
//             return null;
//             
//         return SceneObjectRegistry.Instance?.GetObject(task.objectToFindID);
//     }
// }
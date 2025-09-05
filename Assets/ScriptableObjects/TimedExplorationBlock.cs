// using UnityEngine;
//
// /// <summary>
// /// Block for timed free exploration of an environment
// /// </summary>
// [CreateAssetMenu(fileName = "New Timed Exploration Block", menuName = "Experiment Blocks/Timed Exploration Block")]
// public class TimedExplorationBlock : LocomotionExperimentBlock
// {
//     [Tooltip("Time to allow exploration in minutes")]
//     public float timeForExploration = 3f; // Default to 3 minutes
//         
//     public override string GetBlockType() => "Exploration";
//     public float GetTimeForExplorationInSeconds() => (timeForExploration * 60f);
// }
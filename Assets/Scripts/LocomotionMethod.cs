using UnityEngine;
using System.Linq;

public class LocomotionMethod : MonoBehaviour
{
    // public enum LocomotionType { Unset, Continuous, Teleport, NodeBased }
    // public LocomotionType locomotionChoice; // Dropdown in the Unity Editor

    public static void UpdateFloors(string locomotionArgument)
    {
        // Find all GameObjects with the tag "Floor"
        GameObject[] floors = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.CompareTag("Floor")).ToArray();

        // Enable or disable floors based on the locomotionChoice
        foreach (GameObject floor in floors)
        {
            
            if (locomotionArgument == "continuous" && floor.name == "ContinuousFloor")
            {
                floor.SetActive(true);
            }
            else if (locomotionArgument == "teleport" && floor.name == "TeleportFloor")
            {
                floor.SetActive(true);
            }
            else if (locomotionArgument == "nodebased" && floor.name == "NodeFloor")
            {
                floor.SetActive(true);
            }
            else
            {
                floor.SetActive(false);
            }
        }
    }

    // /// <summary>
    // /// Selects the maze based on the locomotion argument.
    // /// </summary>
    // /// <param name="locomotionArgument"></param>
    // /// <returns></returns>
    // public static GameObject SelectMaze(string locomotionArgument)
    // {
    //     GameObject[] mazes = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.CompareTag("Maze")).ToArray();
    //     
    //     if (mazes.Length == 0) return null;
    //     
    //     GameObject currentMaze = null;
    //     
    //     foreach (GameObject mazeObject in mazes)
    //     {
    //         if (locomotionArgument == "continuous" && mazeObject.name == "ContinuousMaze" ||
    //             locomotionArgument == "teleport" && mazeObject.name == "TeleportMaze" ||
    //             locomotionArgument == "nodebased" && mazeObject.name == "NodeMaze")
    //         {
    //             mazeObject.SetActive(true);
    //             currentMaze = mazeObject;
    //         }
    //         else
    //         {
    //             mazeObject.SetActive(false);
    //         }
    //     }
    //
    //     return currentMaze;
    // }

    // // Script for debugging purpose in the Editor
    // private void UpdateFloorsEditor()
    // {
    //     // Find all GameObjects with the tag "Floor"
    //     GameObject[] floors = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.CompareTag("Floor")).ToArray();
    //
    //     // Enable or disable floors based on the locomotionChoice
    //     foreach (GameObject floor in floors)
    //     {
    //         if (locomotionChoice == LocomotionType.Continuous && floor.name == "ContinuousFloor")
    //         {
    //             floor.SetActive(true);
    //         }
    //         else if (locomotionChoice == LocomotionType.Teleport && floor.name == "TeleportFloor")
    //         {
    //             floor.SetActive(true);
    //         }
    //         else if (locomotionChoice == LocomotionType.NodeBased && floor.name == "NodeFloor")
    //         {
    //             floor.SetActive(true);
    //         }
    //         else if (locomotionChoice == LocomotionType.Unset)
    //         {
    //             floor.SetActive(true);
    //         }
    //         else
    //         {
    //             floor.SetActive(false);
    //         }
    //     }
    // }
    //
    // // This method can be called manually in the Unity Editor to update floors when the choice changes
    // private void OnValidate()
    // {
    //     UpdateFloorsEditor();
    // }
}

using UnityEngine;
using System.Linq;

public class LocomotionMethod : MonoBehaviour
{
    public enum LocomotionType { Unset, Continuous, Teleport, NodeBased }
    public LocomotionType locomotionChoice; // Dropdown in the Unity Editor

    public static void UpdateFloors(string locomotionArgument)
    {
        // Find all GameObjects with the tag "Floor"
        GameObject[] floors = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.CompareTag("Floor")).ToArray();

        // Enable or disable floors based on the locomotionChoice
        foreach (GameObject floor in floors)
        {
            
            if (locomotionArgument == "Continuous" && floor.name == "ContinuousFloor")
            {
                floor.SetActive(true);
            }
            else if (locomotionArgument == "Teleport" && floor.name == "TeleportFloor")
            {
                floor.SetActive(true);
            }
            else if (locomotionArgument == "NodeBased" && floor.name == "NodeFloor")
            {
                floor.SetActive(true);
            }
            else
            {
                floor.SetActive(false);
            }
        }
    }

    // Script for debugging purpose in the Editor
    private void UpdateFloorsEditor()
    {
        // Find all GameObjects with the tag "Floor"
        GameObject[] floors = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.CompareTag("Floor")).ToArray();

        // Enable or disable floors based on the locomotionChoice
        foreach (GameObject floor in floors)
        {
            if (locomotionChoice == LocomotionType.Continuous && floor.name == "ContinuousFloor")
            {
                floor.SetActive(true);
            }
            else if (locomotionChoice == LocomotionType.Teleport && floor.name == "TeleportFloor")
            {
                floor.SetActive(true);
            }
            else if (locomotionChoice == LocomotionType.NodeBased && floor.name == "NodeFloor")
            {
                floor.SetActive(true);
            }
            else if (locomotionChoice == LocomotionType.Unset)
            {
                floor.SetActive(true);
            }
            else
            {
                floor.SetActive(false);
            }
        }
    }

    // This method can be called manually in the Unity Editor to update floors when the choice changes
    private void OnValidate()
    {
        UpdateFloorsEditor();
    }
}

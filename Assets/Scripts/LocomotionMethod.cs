using UnityEngine;
using System.Linq;

public class LocomotionMethod : MonoBehaviour
{
    public enum LocomotionType { Continuous, Teleport, NodeBased }
    public LocomotionType locomotionChoice; // Dropdown in the Unity Editor

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateFloors();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void UpdateFloors()
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
            else
            {
                floor.SetActive(false);
            }
        }
    }

    // This method can be called manually in the Unity Editor to update floors when the choice changes
    private void OnValidate()
    {
        UpdateFloors();
    }
}

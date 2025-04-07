using UnityEngine;

public class PracticeModeSwitcher : MonoBehaviour
{
    public GameObject ContinuousFloor;
    public GameObject FreeTeleportFloor;
    public GameObject NodeFloor;

    // drop down menu for selecting the floor type
    public enum FloorType
    {
        Continuous,
        FreeTeleport,
        Node
    }
    [SerializeField] public FloorType selectedFloorType;
    
    // if selectedFloorType is changed, set the floor type to the selected floor type
    private void OnValidate()
    {
        switch (selectedFloorType)
        {
            case FloorType.Continuous:
                ContinuousFloor.SetActive(true);
                FreeTeleportFloor.SetActive(false);
                NodeFloor.SetActive(false);
                break;
            case FloorType.FreeTeleport:
                ContinuousFloor.SetActive(false);
                FreeTeleportFloor.SetActive(true);
                NodeFloor.SetActive(false);
                break;
            case FloorType.Node:
                ContinuousFloor.SetActive(false);
                FreeTeleportFloor.SetActive(false);
                NodeFloor.SetActive(true);
                break;
        }
    }
}
using UnityEngine;

public class HandControllerForceActive : MonoBehaviour
{
    public static GameObject LeftHandController;
    public static GameObject RightHandController;

    void Start()
    {
        // Force the hand controllers to be active
        SetHandControllersActive(true);
    }

    public static void SetHandControllersActive(bool isActive)
    {
        if (LeftHandController != null)
        {
            LeftHandController.SetActive(isActive);
            Debug.Log("Left hand controller: " + isActive);
        }
        if (RightHandController != null)
        {
            RightHandController.SetActive(isActive);
            Debug.Log("Right hand controller: " + isActive);
        }
    }
}

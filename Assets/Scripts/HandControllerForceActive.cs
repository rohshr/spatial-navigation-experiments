using UnityEngine;

public class HandControllerForceActive : MonoBehaviour
{
    public GameObject leftHandController;
    public GameObject rightHandController;

    void Start()
    {
        // Force the hand controllers to be active
        SetHandControllersActive(true);
    }

    private void SetHandControllersActive(bool isActive)
    {
        if (leftHandController != null)
        {
            leftHandController.SetActive(isActive);
        }
        if (rightHandController != null)
        {
            rightHandController.SetActive(isActive);
        }
    }
}

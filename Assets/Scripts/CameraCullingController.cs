using UnityEngine;

public class CameraCullingController : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        InstructionsController.OnInstructionsCompleted += SetCullingMaskToEverything;
        FinishPointCheck.OnFinishPointReached += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the finish point is reached
        ExperimenterControlScript.OnTrialSkipped += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the object collision is detected
    }

    private void OnDisable()
    {
        InstructionsController.OnInstructionsCompleted -= SetCullingMaskToEverything;
        FinishPointCheck.OnFinishPointReached -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the finish point is reached
        ExperimenterControlScript.OnTrialSkipped -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the object collision is detected
    }

    public void SetCullingMaskToUIOnlyWithHandController()
    {
        if (mainCamera != null)
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            int controllerLayer = LayerMask.NameToLayer("Controller");
            mainCamera.cullingMask = (1 << uiLayer) | (1 << controllerLayer);
        }
        else
        {
            Debug.LogWarning("Main Camera is not assigned.");
        }
    }

    public void SetCullingMaskToEverything()
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask = -1; // -1 sets the culling mask to everything
        }
        else
        {
            Debug.LogWarning("Main Camera is not assigned.");
        }
    }
}

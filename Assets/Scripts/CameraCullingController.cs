using UnityEngine;
using PointingTask;

public class CameraCullingController : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera
    private void OnEnable()
    {
        VRDialogFlowManager.OnDialogFlowComplete += SetCullingMaskToEverything; // Subscribe to the event when dialog flow is completed
        // VRDialogFlowManager.OnSpecificDialogComplete += OnSpecificDialogCompleteHandler; // Subscribe to the event when a specific dialog is completed
        // VRDialogFlowManager.OnExperimentStart += SetCullingMaskToEverything; // Subscribe to the event when the experiment starts        FinishPointCheck.OnFinishPointReached += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the finish point is reached
        VRDialogFlowManager.OnDialogPrefabDisplay += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when a dialog prefab is displayed
        FinishPointCheck.OnFinishPointReached += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the finish point is reached
        TrialManager.OnExplorationBlockCompleted += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the exploration block is completed
        ExperimenterControlScript.OnTrialSkipped += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the object collision is detected
        InputHandler.ProceedTrialEvent += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the proceed trial button is pressed
        PointingEstimationTask.OnPointingComplete += SetCullingMaskToUIOnlyWithHandController; // Subscribe to the event when the pointing task is completed
        //temporary
        PointingEstimationTask.OnPointingTaskStart += SetCullingMaskToEverything;
    }

    private void OnDisable()
    {
        VRDialogFlowManager.OnDialogFlowComplete -= SetCullingMaskToEverything; // Unsubscribe from the event when dialog flow is completed
        // VRDialogFlowManager.OnSpecificDialogComplete -= OnSpecificDialogCompleteHandler; // Unsubscribe from the event when a specific dialog is completed
        // VRDialogFlowManager.OnExperimentStart -= SetCullingMaskToEverything; // Unsubscribe from the event when the experiment starts
        VRDialogFlowManager.OnDialogPrefabDisplay -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when a dialog prefab is displayed
        FinishPointCheck.OnFinishPointReached -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the finish point is reached
        TrialManager.OnExplorationBlockCompleted -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the exploration block is completed       
        ExperimenterControlScript.OnTrialSkipped -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the object collision is detected
        InputHandler.ProceedTrialEvent -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the proceed trial button is pressed       
        PointingEstimationTask.OnPointingComplete -= SetCullingMaskToUIOnlyWithHandController; // Unsubscribe from the event when the pointing task is completed
        //temporary
        PointingEstimationTask.OnPointingTaskStart -= SetCullingMaskToEverything;
    }

    public void SetCullingMaskToUIOnlyWithHandController()
    {
        if (mainCamera != null)
        {
            var uiLayer = LayerMask.NameToLayer("UI");
            var controllerLayer = LayerMask.NameToLayer("Controller");
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
    
    private void OnSpecificDialogCompleteHandler(string _)
    {
        SetCullingMaskToEverything();
    }
}

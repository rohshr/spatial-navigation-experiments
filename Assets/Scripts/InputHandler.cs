using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.XR.CoreUtils;
using UXF;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class InputHandler : MonoBehaviour
{
    private static XROrigin _xrOrigin;
    private GameObject xrLocomotionMediator;
    
    private static GameObject _leftHandController;
    private static GameObject _rightHandController;

    // public InputActionReference proceedAction;
    public InputActionReference backAction;
    public InputActionReference skipTrial;
    public delegate void OnProceed();
    public static event OnProceed ProceedEvent;

    public delegate void OnBack();
    public static event OnBack BackEvent;
    
    public delegate void OnSkipTrial();
    public static event OnSkipTrial SkipTrialEvent;

    void Start()
    {
        // Get XR Origin reference from the scene
        _xrOrigin = GameObject.FindFirstObjectByType<XROrigin>();
        
        if (_xrOrigin != null)
        {
            Transform[] allChildren = _xrOrigin.GetComponentsInChildren<Transform>(true);
        
            _leftHandController = allChildren.FirstOrDefault(t => t.CompareTag("LeftController"))?.gameObject;
            _rightHandController = allChildren.FirstOrDefault(t => t.CompareTag("RightController"))?.gameObject;

            xrLocomotionMediator = _xrOrigin.transform.Find("Locomotion")?.gameObject;
        }
        else
        {
            Debug.LogError("XROrigin not found in the scene.");
        }
        
        SetHandControllers(true);
        
        // Enable the input actions
        if (backAction != null)
        {
            backAction.action.Enable();
        }

        if (skipTrial != null)
        {
            skipTrial.action.Enable();
        }
        
        DisableLocomotion();
    }

    private void OnEnable()
    {
        VRDialogFlowManager.OnDialogFlowComplete += EnableLocomotion; // Subscribe to the event when dialog flow is completed
        // VRDialogFlowManager.OnSpecificDialogComplete += OnSpecificDialogCompleteHandler; // Subscribe to the event when a specific dialog is completed
        // VRDialogFlowManager.OnExperimentStart += EnableLocomotion; // Subscribe to the event when the experiment starts
        VRDialogFlowManager.OnDialogPrefabDisplay += DisableLocomotion;
        FinishPointCheck.OnFinishPointReached += DisableLocomotion; // Subscribe to the event when the finish point is reached
        TrialManager.OnExplorationBlockCompleted += DisableLocomotion; // Subscribe to the event when the exploration block is completed
        ExperimenterControlScript.OnTrialSkipped += DisableLocomotion; // Subscribe to the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided += DisableLocomotion; // Subscribe to the event when the object collision is detected
    }

    private void OnDisable()
    {
        VRDialogFlowManager.OnDialogFlowComplete -= EnableLocomotion; // Unsubscribe from the event when dialog flow is completed
        // VRDialogFlowManager.OnSpecificDialogComplete -= OnSpecificDialogCompleteHandler;
        // VRDialogFlowManager.OnExperimentStart -= EnableLocomotion; // Unsubscribe from the event when the experiment starts
        VRDialogFlowManager.OnDialogPrefabDisplay -= DisableLocomotion;
        FinishPointCheck.OnFinishPointReached -= DisableLocomotion; // Unsubscribe from the event when the finish point is reached
        TrialManager.OnExplorationBlockCompleted -= DisableLocomotion; // Unsubscribe from the event when the exploration block is completed       
        ExperimenterControlScript.OnTrialSkipped -= DisableLocomotion; // Unsubscribe from the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided -= DisableLocomotion; // Unsubscribe from the event when the object collision is detected
    }

    void Update()
    {
        // if (proceedAction != null && proceedAction.action.triggered)
        // {
        //     ProceedEvent?.Invoke(); // Trigger the proceed event
        // } // Handled in VRDialogFlowManager

        if (backAction != null && backAction.action.triggered)
        {
            BackEvent?.Invoke(); // Trigger the back event
        }

        if (skipTrial != null && skipTrial.action.triggered)
        {
            DisableLocomotion();
            SkipTrialEvent?.Invoke(); // Trigger the skip trial event
        }
    }
    
    private static void SetHandControllers(bool isActive)
    {
        if (_leftHandController != null)
        {
            _leftHandController.SetActive(isActive);
            Debug.Log("Left hand controller: " + isActive);
        }
        if (_rightHandController != null)
        {
            _rightHandController.SetActive(isActive);
            Debug.Log("Right hand controller: " + isActive);
        }
    }

    private void EnableLocomotion()
    {
        if (!xrLocomotionMediator.activeSelf)
        {
            xrLocomotionMediator.SetActive(true); // Enable the XRLocomotionMediator
        }
    }

    private void DisableLocomotion()
    {
        if (xrLocomotionMediator.activeSelf)
        {
            xrLocomotionMediator.SetActive(false); // Disable the XRLocomotionMediator
        }
    }
    
    // Update the locomotion controls based on the selected locomotion method
    public static void UpdateLocomotionControls(string locomotionMethod)
    {
        SetHandControllers(true); // Enable the hand controllers in case they are disabled
        if (_xrOrigin != null)
        {
            var leftInputActionManager = _leftHandController.GetComponent<ControllerInputActionManager>();
            var rightInputActionManager = _rightHandController.GetComponent<ControllerInputActionManager>();
            
            if (leftInputActionManager is null) 
            {
                Debug.LogError("LeftInputActionManager not found in the left hand controls.");
                return;
            }
            
            if (locomotionMethod.ToLower() == "continuous")
            {
                leftInputActionManager.smoothMotionEnabled = true;
            }
            else
            {
                leftInputActionManager.smoothMotionEnabled = false;
            }

        }
        else
        {
            Debug.LogError("XROrigin not found in the scene.");
            return;
        }
    }
}
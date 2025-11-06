using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using PointingTask;
using Unity.XR.CoreUtils;
using UnityEngine.Serialization;
using UXF;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class InputHandler : MonoBehaviour
{
    private static XROrigin _xrOrigin;
    private GameObject xrLocomotionMediator;
    
    private static GameObject _leftHandController;
    private static GameObject _rightHandController;

    // public InputActionReference proceedAction;
    [Header("Experimenter Controls")]
    [FormerlySerializedAs("skipTrial")] public InputActionReference proceedTrial;
    
    public delegate void OnProceedTrial();
    public static event OnProceedTrial ProceedTrialEvent;

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
        if (proceedTrial != null)
        {
            proceedTrial.action.Enable();
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
        PointingEstimationSessionGenerator.OnPointingEstimationSessionStart += EnableRotationOnly;
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
        PointingEstimationSessionGenerator.OnPointingEstimationSessionStart -= EnableRotationOnly; // Unsubscribe from the event when the session starts
    }

    void Update()
    {
        // if (proceedTrial != null && proceedTrial.action.triggered)
        // {
        //     DisableLocomotion();
        //     ProceedTrialEvent?.Invoke(); // Trigger the proceed trial event
        //     Session.instance.CurrentTrial?.End(); // End the current trial
        // }
    }
    
    public IEnumerator WaitForProceedTrialInput()
    {
        bool inputReceived = false;
    
        System.Action<InputAction.CallbackContext> inputHandler = (context) => {
            inputReceived = true;
        };

        if (proceedTrial != null)
        {
            proceedTrial.action.performed += inputHandler;
        }

        yield return new WaitUntil(() => inputReceived);

        if (proceedTrial != null)
        {
            proceedTrial.action.performed -= inputHandler;
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

    private void EnableRotationOnly()
    {
        EnableLocomotion();
        // Enable only the turn controls in the XRLocomotionMediator
        // Loop through all children and disable all except "Turn"
        foreach (Transform child in xrLocomotionMediator.transform)
        {
            child.gameObject.SetActive(child.name == "Turn");
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
    
    public static GameObject GetLeftHandController() => _leftHandController;
    public static GameObject GetRightHandController() => _rightHandController;
}
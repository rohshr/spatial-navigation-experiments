using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UXF;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class InputHandler : MonoBehaviour
{
    public GameObject XROrigin;
    private GameObject XRLocomotionMediator;

    public InputActionReference proceedAction;
    public InputActionReference backAction;

    public delegate void OnProceed();
    public static event OnProceed ProceedEvent;

    public delegate void OnBack();
    public static event OnBack BackEvent;

    void Start()
    {
        if (XROrigin != null)
        {
            XRLocomotionMediator = XROrigin.transform.Find("Locomotion")?.gameObject;
        }
        else
        {
            Debug.LogError("XROrigin not found in the scene.");
        }
        // Enable the input actions
        if (proceedAction != null)
        {
            proceedAction.action.Enable();
        }

        if (backAction != null)
        {
            backAction.action.Enable();
        }
        
        DisableLocomotion();
    }

    private void OnEnable()
    {
        InstructionsController.OnInstructionsCompleted += EnableLocomotion;
        FinishPointCheck.OnFinishPointReached += DisableLocomotion; // Subscribe to the event when the finish point is reached
        ExperimenterControlScript.OnTrialSkipped += DisableLocomotion; // Subscribe to the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided += DisableLocomotion; // Subscribe to the event when the object collision is detected
    }

    private void OnDisable()
    {
        InstructionsController.OnInstructionsCompleted -= EnableLocomotion;
        FinishPointCheck.OnFinishPointReached -= DisableLocomotion; // Unsubscribe from the event when the finish point is reached
        ExperimenterControlScript.OnTrialSkipped -= DisableLocomotion; // Unsubscribe from the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided -= DisableLocomotion; // Unsubscribe from the event when the object collision is detected
    }

    void Update()
    {
        if (proceedAction != null && proceedAction.action.triggered)
        {
            ProceedEvent?.Invoke(); // Trigger the proceed event
        }

        if (backAction != null && backAction.action.triggered)
        {
            BackEvent?.Invoke(); // Trigger the back event
        }
    }

    public void EnableLocomotion()
    {
        if (!XRLocomotionMediator.activeSelf)
        {
            XRLocomotionMediator.SetActive(true); // Enable the XRLocomotionMediator
        }
    }

    public void DisableLocomotion()
    {
        if (XRLocomotionMediator.activeSelf)
        {
            XRLocomotionMediator.SetActive(false); // Disable the XRLocomotionMediator
        }
    }

    public static void UpdateLocomotionControls(string locomotionMethod)
    {
        GameObject XROrigin = GameObject.FindWithTag("Player"); // Find the XROrigin GameObject in the scene
        if (XROrigin != null)
        {
            GameObject[] leftHandControls = XROrigin.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("LeftController"))
            .Select(t => t.gameObject)
            .ToArray();
            GameObject[] rightHandControls = XROrigin.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("RightController"))
            .Select(t => t.gameObject)
            .ToArray();

            Debug.Log("Left hand controls: " + leftHandControls.Length);
            Debug.Log("Right hand controls: " + rightHandControls.Length);
            
            var leftInputActionManager = leftHandControls.Length > 0 ? leftHandControls[0].GetComponent<ControllerInputActionManager>() : null;
            var rightInputActionManager = rightHandControls.Length > 0 ? rightHandControls[0].GetComponent<ControllerInputActionManager>() : null;

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

    // public static void UpdateHandPreference(string handPreference, bool smoothMotion)
    // {
    //     // WORKS ONLY WHEN THE HEADSET IS ACTIVE
    //     // Update the hand preference in the InputHandler or any other relevant class
    //     // This is a placeholder for the actual implementation
    //     Debug.Log("Hand preference updated to: " + handPreference);

    //     GameObject XROrigin = GameObject.FindWithTag("Player"); // Find the XROrigin GameObject in the scene
    //     if (XROrigin != null)
    //     {
    //         GameObject[] leftHandControls = XROrigin.GetComponentsInChildren<Transform>()
    //         .Where(t => t.CompareTag("LeftController"))
    //         .Select(t => t.gameObject)
    //         .ToArray();
    //         GameObject[] rightHandControls = XROrigin.GetComponentsInChildren<Transform>()
    //         .Where(t => t.CompareTag("RightController"))
    //         .Select(t => t.gameObject)
    //         .ToArray();

    //         if (handPreference == "Left")
    //         {
    //             foreach (GameObject leftHandControl in leftHandControls)
    //             {
    //                 leftHandControl.SetActive(true);
    //                 var controllerManager = leftHandControl.GetComponent<ControllerInputActionManager>();

    //                 if (controllerManager == null)
    //                 {
    //                     continue;
    //                 }
    //                 if (smoothMotion)
    //                 {
    //                     controllerManager.smoothMotionEnabled = true;
    //                 } else
    //                 {
    //                     controllerManager.smoothMotionEnabled = false;
    //                 }
    //             }
    //             foreach (GameObject rightHandControl in rightHandControls)
    //             {
    //                 rightHandControl.SetActive(false);
    //             }
    //         }
    //         else if (handPreference == "Right")
    //         {
    //             foreach (GameObject leftHandControl in leftHandControls)
    //             {
    //                 leftHandControl.SetActive(false);
    //             }
    //             foreach (GameObject rightHandControl in rightHandControls)
    //             {
    //                 rightHandControl.SetActive(true);
    //                 var controllerManager = rightHandControl.GetComponent<ControllerInputActionManager>();

    //                 if (controllerManager == null)
    //                 {
    //                     continue;
    //                 }

    //                 Debug.Log("ControllerManager found: " + controllerManager.name);
    //                 Debug.Log(controllerManager.smoothMotionEnabled);
    //                 if (smoothMotion)
    //                 {
    //                     controllerManager.smoothMotionEnabled = true;
    //                 } else
    //                 {
    //                     controllerManager.smoothMotionEnabled = false;
    //                 }
    //             }
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("XROrigin not found in the scene.");
    //     }
    // }

    
}
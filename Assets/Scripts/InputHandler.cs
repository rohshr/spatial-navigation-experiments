using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UXF;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class InputHandler : MonoBehaviour
{
    public InputActionReference proceedAction;
    public InputActionReference backAction;

    public delegate void OnProceed();
    public static event OnProceed ProceedEvent;

    public delegate void OnBack();
    public static event OnBack BackEvent;
    public GameObject XRLocomotionMediator;


    void Start()
    {

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
        ObjectCollisionDetection.OnObjectCollided += DisableLocomotion; // Subscribe to the event when the object collision is detected
    }

    private void OnDisable()
    {
        InstructionsController.OnInstructionsCompleted -= EnableLocomotion;
        FinishPointCheck.OnFinishPointReached -= DisableLocomotion; // Unsubscribe from the event when the finish point is reached
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

    public static void UpdateHandPreference(string handPreference, bool smoothMotion)
    {
        // WORKS ONLY WHEN THE HEADSET IS ACTIVE
        // Update the hand preference in the InputHandler or any other relevant class
        // This is a placeholder for the actual implementation
        Debug.Log("Hand preference updated to: " + handPreference);

        GameObject xrOrigin = GameObject.FindWithTag("Player"); // Find the XROrigin GameObject in the scene
        if (xrOrigin != null)
        {
            GameObject[] leftHandControls = xrOrigin.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("LeftController"))
            .Select(t => t.gameObject)
            .ToArray();
            GameObject[] rightHandControls = xrOrigin.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("RightController"))
            .Select(t => t.gameObject)
            .ToArray();

            if (handPreference == "Left")
            {
                foreach (GameObject leftHandControl in leftHandControls)
                {
                    leftHandControl.SetActive(true);
                    var controllerManager = leftHandControl.GetComponent<ControllerInputActionManager>();

                    if (controllerManager == null)
                    {
                        continue;
                    }
                    if (smoothMotion)
                    {
                        controllerManager.smoothMotionEnabled = true;
                    } else
                    {
                        controllerManager.smoothMotionEnabled = false;
                    }
                }
                foreach (GameObject rightHandControl in rightHandControls)
                {
                    rightHandControl.SetActive(false);
                }
            }
            else if (handPreference == "Right")
            {
                foreach (GameObject leftHandControl in leftHandControls)
                {
                    leftHandControl.SetActive(false);
                }
                foreach (GameObject rightHandControl in rightHandControls)
                {
                    rightHandControl.SetActive(true);
                    var controllerManager = rightHandControl.GetComponent<ControllerInputActionManager>();

                    if (controllerManager == null)
                    {
                        continue;
                    }

                    Debug.Log("ControllerManager found: " + controllerManager.name);
                    Debug.Log(controllerManager.smoothMotionEnabled);
                    if (smoothMotion)
                    {
                        controllerManager.smoothMotionEnabled = true;
                    } else
                    {
                        controllerManager.smoothMotionEnabled = false;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("XROrigin not found in the scene.");
        }
    }

    public static void SetSmoothMotion(bool isSmoothMotion, string handPreference)
    {

    }
}
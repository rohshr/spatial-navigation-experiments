using UnityEngine;
using UnityEngine.InputSystem;
using UXF;

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

        XRLocomotionMediator.SetActive(false); // Disable the XRLocomotionMediator at the start
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
}
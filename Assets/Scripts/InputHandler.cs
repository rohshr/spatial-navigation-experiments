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
}
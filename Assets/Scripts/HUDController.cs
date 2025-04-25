using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UXF;

public class HUDController : MonoBehaviour
{   
    public GameObject HUDCanvas; // Reference to the HUD canvas

    // Input Action for toggling the canvas
    public InputActionReference toggleAction;

    void Awake()
    {
        HUDCanvas.SetActive(false);
    }
    void OnEnable()
    {
        // Subscribe to the input action
        if (toggleAction != null)
        {
            toggleAction.action.performed += OnToggleAction;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the input action
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnToggleAction;
        }
    }
    private void OnToggleAction(InputAction.CallbackContext context)
    {
        // Toggle the visibility of the HUDCanvas
        HUDCanvas.SetActive(!HUDCanvas.activeSelf);
    }
}

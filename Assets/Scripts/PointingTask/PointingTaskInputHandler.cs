using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PointingTask
{
    public class PointingTaskInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference submitAction;
        private PointingEstimationSessionGenerator pointingEstimationSessionGenerator;

        void Start()
        {
            pointingEstimationSessionGenerator = FindFirstObjectByType<PointingEstimationSessionGenerator>();
            if (submitAction != null)
            {
                submitAction.action.performed += OnSubmitPerformed;
                submitAction.action.Enable();
            }
        }

        void OnDestroy()
        {
            if (submitAction != null)
            {
                submitAction.action.performed -= OnSubmitPerformed;
                submitAction.action.Disable();
            }
        }
        
        private void OnSubmitPerformed(InputAction.CallbackContext context)
        {
            if (pointingEstimationSessionGenerator != null)
            {
                pointingEstimationSessionGenerator.OnPointingSubmitted();
            }
        }
    }
}

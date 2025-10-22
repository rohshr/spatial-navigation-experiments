using UnityEngine;
using UnityEngine.InputSystem;

namespace PointingTask
{
    public class PointingTaskInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference submitAction;
        private PointingEstimationSessionGenerator sessionGenerator;

        void Start()
        {
            sessionGenerator = FindFirstObjectByType<PointingEstimationSessionGenerator>();
        
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
            }
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context)
        {
            if (sessionGenerator != null)
            {
                sessionGenerator.OnPointingSubmitted();
            }
        }
    }
}

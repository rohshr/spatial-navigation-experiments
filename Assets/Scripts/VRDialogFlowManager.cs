using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class VRDialogFlowManager : MonoBehaviour
{
    [Header("Dialog Configuration")]
    [SerializeField] private List<GameObject> dialogPrefabs = new List<GameObject>();
    [SerializeField] private float dialogDistance = 2.0f;
    [SerializeField] private float dialogScale = 1.0f;
    
    [Header("Follow Behavior")]
    [SerializeField] private float followThresholdAngle = 45.0f;
    [SerializeField] private float followSpeed = 2.0f;
    [SerializeField] private float followSmoothTime = 0.3f;
    [SerializeField] private bool enableFollowBehavior = true;
    
    [Header("Input Configuration")]
    [SerializeField] private InputActionReference advanceInputAction;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Canvas dialogCanvas;
    [SerializeField] private XROrigin xrOrigin;
    
    // Private variables
    private int currentDialogIndex = 0;
    private GameObject currentDialogInstance;
    private bool isTransitioning = false;
    private bool experimentStarted = false;
    
    // Follow behavior variables
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;
    private Coroutine followCoroutine;
    
    // Events
    public System.Action OnDialogFlowComplete;
    public System.Action<int> OnDialogChanged;
    public System.Action OnExperimentStart;
    
    private void Start()
    {
        InitializeDialogSystem();
        SetupInputActions();
    }
    
    private void OnEnable()
    {
        if (advanceInputAction != null)
        {
            advanceInputAction.action.Enable();
        }
    }
    
    private void OnDisable()
    {
        if (advanceInputAction != null)
        {
            advanceInputAction.action.Disable();
        }
    }
    
    private void Update()
    {
        if (!experimentStarted && !isTransitioning)
        {
            if (enableFollowBehavior && currentDialogInstance != null)
            {
                CheckFollowBehavior();
            }
        }
    }
    
    private void SetupInputActions()
    {
        if (advanceInputAction != null)
        {
            advanceInputAction.action.performed += OnAdvanceInputPerformed;
        }
        else
        {
            Debug.LogWarning("VRDialogFlowManager: No advance input action assigned!");
        }
    }
    
    private void InitializeDialogSystem()
    {
        // Get XR Origin reference if not assigned
        if (xrOrigin == null)
        {
            xrOrigin = FindFirstObjectByType<XROrigin>();
        }
        
        // Get camera reference if not assigned
        if (cameraTransform == null)
        {
            if (xrOrigin != null)
            {
                cameraTransform = xrOrigin.Camera.transform;
            }
            else
            {
                cameraTransform = Camera.main?.transform;
            }
        }
        
        // Create canvas if not assigned
        if (dialogCanvas == null)
        {
            CreateDialogCanvas();
        }
        
        // Validate dialog prefabs
        if (dialogPrefabs.Count == 0)
        {
            Debug.LogError("VRDialogFlowManager: No dialog prefabs assigned!");
            return;
        }
        
        // Start with first dialog
        ShowDialog(0);
    }
    
    private void CreateDialogCanvas()
    {
        GameObject canvasObject = new GameObject("VR Dialog Canvas");
        dialogCanvas = canvasObject.AddComponent<Canvas>();
        dialogCanvas.renderMode = RenderMode.WorldSpace;
        dialogCanvas.worldCamera = Camera.main;
        
        // Add Canvas Scaler for proper scaling
        var canvasScaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster for UI interactions
        canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    }
    
    private void ShowDialog(int dialogIndex)
    {
        if (dialogIndex < 0 || dialogIndex >= dialogPrefabs.Count)
        {
            Debug.LogError($"VRDialogFlowManager: Invalid dialog index {dialogIndex}");
            return;
        }
        
        StartCoroutine(ShowDialogCoroutine(dialogIndex));
    }
    
    private IEnumerator ShowDialogCoroutine(int dialogIndex)
    {
        isTransitioning = true;
        
        // Store the previous dialog's transform before destroying it
        Vector3 previousPosition = targetPosition;
        Quaternion previousRotation = targetRotation;
        
        // Hide current dialog with fade out
        if (currentDialogInstance != null)
        {
            previousPosition = currentDialogInstance.transform.position;
            previousRotation = currentDialogInstance.transform.rotation;
            yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
            Destroy(currentDialogInstance);
        }
        
        // Update index
        currentDialogIndex = dialogIndex;
        
        // Calculate position and rotation
        CalculateDialogTransform();
        
        // Instantiate new dialog
        currentDialogInstance = Instantiate(dialogPrefabs[dialogIndex], dialogCanvas.transform);
        
        // Immediately set transform without any interpolation
        var rectTransform = currentDialogInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = previousPosition;
            rectTransform.rotation = previousRotation;
            rectTransform.localScale = Vector3.one * dialogScale;
        }
        else
        {
            currentDialogInstance.transform.position = previousPosition;
            currentDialogInstance.transform.rotation = previousRotation;
            currentDialogInstance.transform.localScale = Vector3.one * dialogScale;
        }
        
        // Force immediate transform update
        Canvas.ForceUpdateCanvases();
        
        // Fade in new dialog
        yield return StartCoroutine(FadeDialog(currentDialogInstance, true));
        
        isTransitioning = false;
        
        // Invoke event
        OnDialogChanged?.Invoke(currentDialogIndex);
        
        // Start follow behavior
        if (enableFollowBehavior)
        {
            StartFollowBehavior();
        }
    }
    
    private void CalculateDialogTransform()
    {
        Vector3 cameraPosition = cameraTransform.position;
        Vector3 cameraForward = cameraTransform.forward;
        
        // Position dialog in front of camera
        targetPosition = cameraPosition + cameraForward * dialogDistance;
        
        // Make dialog face the camera
        Vector3 lookDirection = (cameraPosition - targetPosition).normalized;
        lookDirection.y = 0;
        lookDirection = lookDirection.normalized;
        targetRotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
    }
    
    private void CalculateCanvasTransform()
    {
        Vector3 cameraPosition = cameraTransform.position;
        Vector3 cameraForward = cameraTransform.forward;
        
        Transform canvasTransform = dialogCanvas.transform;
        Vector3 startPosition = canvasTransform.position;
        Quaternion startRotation = canvasTransform.rotation;
        
        // Position dialog in front of camera
        canvasTransform.position = cameraPosition + cameraForward * dialogDistance;
        
        // Make dialog face the camera
        Vector3 lookDirection = (cameraPosition - targetPosition).normalized;
        canvasTransform.rotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
    }
    
    private IEnumerator FadeDialog(GameObject dialog, bool fadeIn)
    {
        if (dialog == null) yield break;
        
        CanvasGroup canvasGroup = dialog.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dialog.AddComponent<CanvasGroup>();
        }
        
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float duration = 0.3f;
        float elapsed = 0f;
        
        canvasGroup.alpha = startAlpha;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
    private void OnAdvanceInputPerformed(InputAction.CallbackContext context)
    {
        if (!experimentStarted && !isTransitioning)
        {
            AdvanceDialog();
        }
    }
    
    private void AdvanceDialog()
    {
        if (isTransitioning) return;
        
        if (currentDialogIndex < dialogPrefabs.Count - 1)
        {
            // Recalculate position based on current camera view before showing next dialog
            // CalculateCanvasTransform();
            CalculateDialogTransform();
            // Show next dialog
            ShowDialog(currentDialogIndex + 1);
        }
        else
        {
            // All dialogs complete
            CompleteDialogFlow();
        }
    }
    
    private void CompleteDialogFlow()
    {
        StartCoroutine(CompleteDialogFlowCoroutine());
    }
    
    private IEnumerator CompleteDialogFlowCoroutine()
    {
        isTransitioning = true;
        
        // Stop follow behavior
        StopFollowBehavior();
        
        // Fade out final dialog
        if (currentDialogInstance != null)
        {
            yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
            Destroy(currentDialogInstance);
            currentDialogInstance = null;
        }
        
        experimentStarted = true;
        
        // Invoke completion events
        OnDialogFlowComplete?.Invoke();
        OnExperimentStart?.Invoke();
        
        Debug.Log("VR Dialog Flow Complete - Experiment Starting");
    }
    
    private void CheckFollowBehavior()
    {
        if (currentDialogInstance == null || cameraTransform == null) return;
        
        Vector3 cameraPosition = cameraTransform.position;
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 dialogPosition = currentDialogInstance.transform.position;
        
        // Calculate angle between camera forward and dialog direction
        Vector3 dialogDirection = (dialogPosition - cameraPosition).normalized;
        float angle = Vector3.Angle(cameraForward, dialogDirection);
        
        // Check if dialog is outside threshold
        if (angle > followThresholdAngle)
        {
            // Calculate new target position
            CalculateDialogTransform();
            
            // Start smooth movement if not already moving
            if (followCoroutine == null)
            {
                followCoroutine = StartCoroutine(SmoothFollowDialog());
            }
        }
    }
    
    private IEnumerator SmoothFollowDialog()
    {
        if (currentDialogInstance == null) yield break;
        
        Transform dialogTransform = currentDialogInstance.transform;
        Vector3 startPosition = dialogTransform.position;
        Quaternion startRotation = dialogTransform.rotation;
        
        float elapsed = 0f;
        float duration = 1f / followSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Use smooth step for easing
            t = Mathf.SmoothStep(0f, 1f, t);
            
            dialogTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            dialogTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        dialogTransform.position = targetPosition;
        dialogTransform.rotation = targetRotation;
        
        followCoroutine = null;
    }
    
    private void StartFollowBehavior()
    {
        StopFollowBehavior();
        // Follow behavior is handled in Update()
    }
    
    private void StopFollowBehavior()
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
    }
    
    // Public methods for external control
    public void RestartDialogFlow()
    {
        StopAllCoroutines();
        followCoroutine = null;
        
        if (currentDialogInstance != null)
        {
            Destroy(currentDialogInstance);
        }
        
        currentDialogIndex = 0;
        isTransitioning = false;
        experimentStarted = false;
        
        ShowDialog(0);
    }
    
    public void SetDialogDistance(float distance)
    {
        dialogDistance = Mathf.Max(0.5f, distance);
    }
    
    public void SetFollowThreshold(float angleDegrees)
    {
        followThresholdAngle = Mathf.Clamp(angleDegrees, 10f, 180f);
    }
    
    public void SetFollowSpeed(float speed)
    {
        followSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void ToggleFollowBehavior(bool enabled)
    {
        enableFollowBehavior = enabled;
        if (!enabled)
        {
            StopFollowBehavior();
        }
    }
    
    // Getters
    public bool IsExperimentStarted => experimentStarted;
    public bool IsTransitioning => isTransitioning;
    public int CurrentDialogIndex => currentDialogIndex;
    public int TotalDialogs => dialogPrefabs.Count;
    
    private void OnDestroy()
    {
        StopAllCoroutines();
        
        // Unsubscribe from input actions
        if (advanceInputAction != null)
        {
            advanceInputAction.action.performed -= OnAdvanceInputPerformed;
        }
    }
}
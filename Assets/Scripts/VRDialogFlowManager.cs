using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UXF;
using Canvas = UnityEngine.Canvas;

/// <summary>
/// VRDialogFlowManager - Manages sequential dialog presentation in VR environments with smooth transitions and user follow behavior
/// </summary>
public class VRDialogFlowManager : MonoBehaviour
{
    [Tooltip("Specify how far in front of the user the dialog should appear.")]
    [SerializeField] private float dialogDistance = 2.0f;
    [Tooltip("Specify the visual scale of the dialog UI.")]
    [SerializeField] private float dialogScale = 1.0f;
    
    [Header("Flow Configuration")]
    private List<GameObject> dialogPrefabsSequence = new List<GameObject>();
    private Queue<GameObject> dialogQueue = new Queue<GameObject>();
    
    
    [Header("Follow Behavior")]
    [Tooltip("Enable or disable the follow behavior for the dialog UI. When enabled, the dialog will smoothly follow the user's head movements.")]
    [SerializeField] private bool enableFollowBehavior = true;
    [Tooltip("Angle threshold (in degrees) to trigger follow behavior when the user turns their head. The UI dialog will follow user's view if they look away beyond this angle.")]
    [SerializeField] private float followThresholdAngle = 30.0f;
    [Tooltip("Speed at which the dialog UI follows the user's head movement.")]
    [SerializeField] private float followSpeed = 2.0f;
    
    [Header("Input Configuration")]
    [Tooltip("Input action reference for advancing the dialog. This should be set up in the Input System.")]
    [SerializeField] private InputActionReference advanceInputAction;
    
    [Header("References")]
    [Tooltip("Reference to the XR Origin in the scene. This is used to get the user's head position and orientation.")]
    [SerializeField] private XROrigin xrOrigin;
    [Tooltip("Reference to the main camera. This is the camera GameObject inside the XR Origin.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Reference to the canvas that will display the UI dialogs.")]
    [SerializeField] private Canvas dialogCanvas;
    
    // Private variables
    private int currentDialogIndex = 0;
    private GameObject currentDialogInstance;
    private bool isTransitioning = false;
    private bool trialStarted = false; // Flag to indicate if a trial is in session
    private bool sessionEnded = false; // Flag to indicate if the session has ended
    private GameObject endSessionDialogPrefab; // Dialog prefab to show at session end
    
    // Follow behavior variables
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;
    private Coroutine followCoroutine;
    
    // Events
    public static event System.Action OnDialogFlowComplete;
    public static event System.Action OnDialogPrefabChanged;
    public static event Action OnDialogPrefabDisplay; // Event triggered when any dialog prefab is displayed
    
    private void Start()
    {
        InitializeDialogSystem();
    }
    
    private void OnEnable()
    {
        if (advanceInputAction != null)
        {
            advanceInputAction.action.Enable();
        }
        SessionGenerator.OnPlayStart += ShowDialogPrefab;
        SessionGenerator.OnSessionGenerate += ShowDialogSequence;
        PointingEstimationSessionGenerator.OnPlayStart += ShowDialogPrefab;
        PointingEstimationSessionGenerator.OnSessionGenerate += ShowDialogSequence;
        PointingEstimationSessionGenerator.OnShowNextInstruction += ShowDialogPrefab;
        SessionGenerator.OnBlockEnd += ShowDialogSequence;
        SessionGenerator.OnTrialEnd += ShowDialogSequence;
        SessionGenerator.OnSessionEnd += EndSession;
    }
    
    private void OnDisable()
    {
        if (advanceInputAction != null)
        {
            advanceInputAction.action.Disable();
        }
        SessionGenerator.OnPlayStart -= ShowDialogPrefab;
        SessionGenerator.OnSessionGenerate -= ShowDialogSequence;
        PointingEstimationSessionGenerator.OnPlayStart -= ShowDialogPrefab;
        PointingEstimationSessionGenerator.OnSessionGenerate -= ShowDialogSequence;
        PointingEstimationSessionGenerator.OnShowNextInstruction -= ShowDialogPrefab;
        SessionGenerator.OnBlockEnd -= ShowDialogSequence;
        SessionGenerator.OnTrialEnd -= ShowDialogSequence;
        SessionGenerator.OnSessionEnd -= EndSession;
    }
    
    private void Update()
    {
        if (!isTransitioning && currentDialogInstance != null)
        {
            if (enableFollowBehavior)
            {
                CheckFollowBehavior();
            }
        }
    }
    
    private IEnumerator WaitForAdvanceInput()
    {
        bool inputReceived = false;
    
        System.Action<InputAction.CallbackContext> inputHandler = (context) => {
            inputReceived = true;
        };

        if (advanceInputAction != null)
        {
            advanceInputAction.action.performed += inputHandler;
        }

        yield return new WaitUntil(() => inputReceived);

        if (advanceInputAction != null)
        {
            advanceInputAction.action.performed -= inputHandler;
        }
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
        if (currentDialogInstance == null)
        {
            followCoroutine = null;
            yield break;
        }
        
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
        
        if (currentDialogInstance != null && dialogTransform != null)
        {
            dialogTransform.position = targetPosition;
            dialogTransform.rotation = targetRotation;
        }
        
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
        
        // Start with first dialog
        dialogPrefabsSequence.Clear();
        dialogQueue.Clear();
    }
    
    // Method to show a single dialog
    public void ShowDialogPrefab(GameObject dialogPrefab)
    {
        Debug.Log($"Showing dialog: {dialogPrefab.name}");
        if (dialogPrefab == null)
        {
            Debug.LogError("VRDialogFlowManager: Cannot show dialog - prefab is null");
            return;
        }
        
        StartCoroutine(ShowDialogPrefabCoroutine(dialogPrefab));
    }
    
    private IEnumerator ShowDialogPrefabCoroutine(GameObject dialogPrefab)
    {
        OnDialogPrefabDisplay?.Invoke();
        isTransitioning = true;
        
        // Store the previous dialog's transform before destroying it
        Vector3 previousPosition = targetPosition;
        Quaternion previousRotation = targetRotation;
        
        // Hide current dialog with fade out
        if (currentDialogInstance != null)
        {
            StopFollowBehavior();
            yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
            Destroy(currentDialogInstance);
        }
        
        // Calculate position and rotation
        CalculateDialogTransform();
        
        // Instantiate new dialog
        currentDialogInstance = Instantiate(dialogPrefab, dialogCanvas.transform);
        
        // Immediately set transform without any interpolation
        var rectTransform = currentDialogInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = targetPosition;
            rectTransform.rotation = targetRotation;
            rectTransform.localScale = Vector3.one * dialogScale;
        }
        else
        {
            currentDialogInstance.transform.position = targetPosition;
            currentDialogInstance.transform.rotation = targetRotation;
            currentDialogInstance.transform.localScale = Vector3.one * dialogScale;
        }
        
        // Force immediate transform update
        Canvas.ForceUpdateCanvases();
        
        // Fade in new dialog
        yield return StartCoroutine(FadeDialog(currentDialogInstance, true));
        
        isTransitioning = false;
        
        // Invoke event
        OnDialogPrefabChanged?.Invoke();
        
        // Start follow behavior
        if (enableFollowBehavior)
        {
            StartFollowBehavior();
        }

        if (dialogPrefab != endSessionDialogPrefab)
        {
            yield return StartCoroutine(WaitForAdvanceInput());
        }
    }
    
    // Method to call at the end of the session
    private void EndSession(GameObject endDialogPrefab)
    {
        sessionEnded = true;
        endSessionDialogPrefab = endDialogPrefab;
    }
    
    // Method to show multiple dialogs in sequence
    private void ShowDialogSequence(List<GameObject> dialogPrefabs)
    {
        dialogQueue.Clear();
        foreach (GameObject dialog in dialogPrefabs)
        {
            if (dialog != null)
            {
                dialogQueue.Enqueue(dialog);
            }
        }
        
        if (dialogQueue.Count > 0)
        {
            StartCoroutine(ProcessDialogQueue());
        }
    }
    
    private IEnumerator ProcessDialogQueue()
    {
        Debug.Log($"Processing dialog queue with {dialogQueue.Count} dialogs");
        while (dialogQueue.Count > 0)
        {
            Debug.Log($"Current dialog in queue: {dialogQueue.Peek().name}");
            GameObject dialogPrefab = dialogQueue.Dequeue();
            yield return StartCoroutine(ShowDialogPrefabCoroutine(dialogPrefab));
        }
        
        // All dialogs complete
        if (sessionEnded)
        {
            yield return StartCoroutine(ShowDialogPrefabCoroutine(endSessionDialogPrefab));
            yield break;
        }
        
        StartCoroutine(CompleteDialogPrefabFlowCoroutine());
    }
    
    private void CompleteDialogPrefabFlow()
    {
        StartCoroutine(CompleteDialogPrefabFlowCoroutine());
    }
    
    private IEnumerator CompleteDialogPrefabFlowCoroutine()
    {
        isTransitioning = true;
        
        // Stop follow behavior
        StopFollowBehavior();
        
        // Fade out final dialog
        if (currentDialogInstance != null)
        {
            yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
            Destroy(currentDialogInstance);
        }
        
        trialStarted = true;
        
        // Invoke completion events
        OnDialogFlowComplete?.Invoke();
        
        Debug.Log("VR Dialog Flow Complete - Experiment Starting");
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
    
    public void ToggleFollowBehavior(bool followStatus)
    {
        enableFollowBehavior = followStatus;
        if (!followStatus)
        {
            StopFollowBehavior();
        }
    }
    
    // Getters
    public bool IsTrialStarted => trialStarted;
    public bool IsTransitioning => isTransitioning;
    public int CurrentDialogIndex => currentDialogIndex;
    public int TotalDialogs => dialogQueue.Count;
    
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
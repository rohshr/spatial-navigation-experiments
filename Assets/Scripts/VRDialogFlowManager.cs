// VRDialogFlowManager - Manages sequential dialog presentation in VR environments with smooth transitions and user follow behavior

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

// [System.Serializable]
// public class DialogSequence
// {
//     public string sequenceName;
//     public List<GameObject> dialogPrefabs = new List<GameObject>();
// }
//
// [System.Serializable]
// public class DialogCategory
// {
//     public string categoryName;
//     public List<DialogSequence> sequences = new List<DialogSequence>();
// }

// [System.Serializable]
// public class DialogData
// {
//     public string dialogKey;
//     public GameObject dialogPrefab;
// }

// [System.Serializable]
// public struct LocomotionMethodDialog
// {
//     public GameObject continuousMethodInstructions;
//     public GameObject teleportMethodInstructions;
//     public GameObject nodeMethodInstructions;
// }
//
// [System.Serializable]
// public class ScenarioMessageDialogs
// {
//     public GameObject startMessageDialog;
//     public GameObject endMessageDialog;
// }
//
// [System.Serializable]
// public class ObjectSearchMessageDialogs
// {
//     public string objectKey;
//     public GameObject objectSearchInstructionDialog;
//     public GameObject objectFoundDialog;
// }
//
// [System.Serializable]
// public class ObjectSearchTrialsDialogs: ScenarioMessageDialogs
// {
//     public List<ObjectSearchMessageDialogs> objectsToSearch;
// }

// [System.Serializable]
// public struct PracticeSessionInstructions
// {
//     public GameObject practiceSessionStartDialog;
//     public GameObject practiceSessionEndDialog;
//     
//     // Locomotion method instructions
//     public ScenarioMessageDialogs curvedEnvironmentInstructions;
//     public ScenarioMessageDialogs angledEnvironmentInstructions;
//     public ObjectSearchTrialsDialogs openEnvironmentInstructions;
// }

public class VRDialogFlowManager : MonoBehaviour
{
    [Header("UI Dialog Configuration")]
    // [SerializeField] private List<DialogData> allDialogs = new List<DialogData>();
    // [Tooltip("Assign dialog that will should be shown by default before the start of the experiment")]
    // [SerializeField] private GameObject welcomeDialog;
    //
    // [Tooltip("Assign dialogs that should be shown at the start of the experiment session. These will be shown after the welcome dialog in the sequence they are added here.")]
    // [SerializeField] private List<GameObject> startDialogs;
    //
    // [Tooltip("Assign dialogs that should be shown at the end of the experiment session. These will be shown after all other dialogs in the sequence they are added here.")]
    // [SerializeField] private List<GameObject> endDialogs;
    //
    // [Tooltip("Assign dialogs for locomotion method instructions.")]
    // [SerializeField] private LocomotionMethodDialog locomotionMethodDialogs;
    //
    // [Tooltip("Assign dialogs for practice session.")]
    // [SerializeField] private PracticeSessionInstructions practiceSessionInstructions;
    
    // [Tooltip("Assign dialogs for experimental session.")]
    // [SerializeField] private ScenarioMessageDialogs experimentalSessionMessageDialogs;
    
    // [SerializeField] private List<DialogCategory> dialogCategories = new List<DialogCategory>
    // {
    //     new DialogCategory
    //     {
    //         categoryName = "Info",
    //         sequences = new List<DialogSequence>
    //         {
    //             new DialogSequence { sequenceName = "General Info", dialogPrefabs = new List<GameObject>() }
    //         }
    //     },
    //     new DialogCategory
    //     {
    //         categoryName = "Locomotion Instructions",
    //         sequences = new List<DialogSequence>
    //         {
    //             new DialogSequence { sequenceName = "Movement Techniques", dialogPrefabs = new List<GameObject>() }
    //         }
    //     },
    //     new DialogCategory
    //     {
    //         categoryName = "Practice Sessions",
    //         sequences = new List<DialogSequence>
    //         {
    //             new DialogSequence { sequenceName = "Curved Environment", dialogPrefabs = new List<GameObject>() },
    //             new DialogSequence { sequenceName = "Angled Environment", dialogPrefabs = new List<GameObject>() },
    //             new DialogSequence { sequenceName = "Open Environment", dialogPrefabs = new List<GameObject>() }
    //         }
    //     }
    // };
    
    [Header("Triggered Dialogs")]
    private bool isDialogFlowPaused = false;
    // private List<string> pausedDialogSequence = new List<string>();
    private List<GameObject> pausedDialogPrefabsSequence = new List<GameObject>();
    private int pausedDialogIndex = 0;
    
    [Tooltip("Specify how far in front of the user the dialog should appear.")]
    [SerializeField] private float dialogDistance = 2.0f;
    [Tooltip("Specify the visual scale of the dialog UI.")]
    [SerializeField] private float dialogScale = 1.0f;
    
    [Header("Flow Configuration")]
    // private List<string> currentDialogSequence = new List<string>();
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
    
    // Follow behavior variables
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;
    private Coroutine followCoroutine;
    
    // Events
    public static event System.Action OnDialogFlowComplete;
    public static event System.Action<string> OnSpecificDialogComplete;
    public static event System.Action<int> OnDialogChanged;
    public static event System.Action OnDialogPrefabChanged;
    // public static event System.Action OnExperimentStart;
    public static event Action OnDialogPrefabDisplay; // Event triggered when any dialog prefab is displayed
    
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
        SessionGenerator.OnPlayStart += ShowDialogPrefab;
        SessionGenerator.OnSessionGenerate += ShowDialogSequence;
        SessionGenerator.OnBlockEnd += ShowDialogSequence;
    }
    
    private void OnDisable()
    {
        if (advanceInputAction != null)
        {
            advanceInputAction.action.Disable();
        }
        SessionGenerator.OnPlayStart -= ShowDialogPrefab;
        SessionGenerator.OnSessionGenerate -= ShowDialogSequence;
        SessionGenerator.OnBlockEnd -= ShowDialogSequence;
    }
    
    private void Update()
    {
        // if (!trialStarted && !isTransitioning)
        // {
        //     if (enableFollowBehavior && currentDialogInstance != null)
        //     {
        //         CheckFollowBehavior();
        //     }
        // }
        // Allow follow behavior for both main dialog flow and triggered dialogs
        if (!isTransitioning && currentDialogInstance != null)
        {
            if (enableFollowBehavior)
            {
                CheckFollowBehavior();
            }
        }
    }
    
    private void SetupInputActions()
    {
        // if (advanceInputAction != null)
        // {
        //     advanceInputAction.action.performed += OnAdvanceInputPerformed;
        // }
        // else
        // {
        //     Debug.LogWarning("VRDialogFlowManager: No advance input action assigned!");
        // }
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
        // Build dialog sequence based on session variables
        // BuildDialogSequence();
        
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
        
        // // Validate dialog prefabs
        // if (currentDialogSequence.Count == 0)
        // {
        //     Debug.LogError("VRDialogFlowManager: No dialog prefabs assigned!");
        //     return;
        // }
        
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
            // previousPosition = currentDialogInstance.transform.position;
            // previousRotation = currentDialogInstance.transform.rotation;
            yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
            Destroy(currentDialogInstance);
        }
        
        // Calculate position and rotation
        CalculateDialogTransform();
        
        // Instantiate new dialog
        currentDialogInstance = Instantiate(dialogPrefab, dialogCanvas.transform);
        Debug.Log($"Showing dialog: {currentDialogInstance.name}");
        
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
        
        yield return StartCoroutine(WaitForAdvanceInput());
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
        StartCoroutine(CompleteDialogPrefabFlowCoroutine());
    }
    
    // private void ShowNextDialogPrefab()
    // {
    //     if (currentDialogIndex < dialogPrefabsSequence.Count - 1)
    //     {
    //         // Recalculate position based on current camera view before showing next dialog
    //         // CalculateCanvasTransform();
    //         CalculateDialogTransform();
    //         // Show next dialog
    //         ShowDialogPrefab(dialogPrefabsSequence[currentDialogIndex + 1]);
    //     }
    //     else
    //     {
    //         // All dialogs complete
    //         CompleteDialogPrefabFlow();
    //     }
    // }
    
    // private void OnAdvanceInputPerformed(InputAction.CallbackContext context)
    // {
    //     if (!trialStarted && !isTransitioning && Session.instance.hasInitialised)
    //     {
    //         StartCoroutine(ProcessDialogQueue());
    //     }
    // }
    
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
        // OnExperimentStart?.Invoke();
        
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
        
        // // Unsubscribe from input actions
        // if (advanceInputAction != null)
        // {
        //     advanceInputAction.action.performed -= OnAdvanceInputPerformed;
        // }
    }
    
    // Not required anymore because sequencing is initiated by SessionGenerator
    // public void BuildDialogSequence()
    // {
    //     // currentDialogSequence.Clear();
    //     // currentDialogSequence.Add("StartDialog");
    //     
    //     dialogPrefabsSequence.Clear();
    //     dialogPrefabsSequence.AddRange(startDialogs);
    //     
    //     // Session information
    //     bool isPractice = Convert.ToBoolean(Session.instance.participantDetails["is_practice"]);
    //     String locomotionMethodFromUI = Session.instance.participantDetails["locomotion_method"].ToString().ToLower();
    //
    //     if (isPractice)
    //     {
    //         // Practice session sequence
    //         // currentDialogSequence.Add("PracticeBriefing");
    //         
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.practiceSessionStartDialog);
    //     
    //         // Add locomotion-specific instructions
    //         switch (locomotionMethodFromUI.ToLower())
    //         {
    //             case "continuous":
    //                 // currentDialogSequence.Add("ContinuousInstructions");
    //                 dialogPrefabsSequence.Add(locomotionMethodDialogs.continuousMethodInstructions);
    //                 break;
    //             case "teleport":
    //                 // currentDialogSequence.Add("TeleportInstructions");
    //                 dialogPrefabsSequence.Add(locomotionMethodDialogs.teleportMethodInstructions);
    //                 break;
    //             case "nodebased":
    //                 // currentDialogSequence.Add("NodeInstructions");
    //                 dialogPrefabsSequence.Add(locomotionMethodDialogs.nodeMethodInstructions);
    //                 break;
    //         }
    //         
    //         // Add environment-specific instructions
    //         // currentDialogSequence.Add("CurvedEnvironmentInstructions");
    //         // currentDialogSequence.Add("AngledEnvironmentInstructions");
    //         // currentDialogSequence.Add("OpenEnvironmentInstructions");
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.curvedEnvironmentInstructions.startMessageDialog);
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.curvedEnvironmentInstructions.endMessageDialog);
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.angledEnvironmentInstructions.startMessageDialog);
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.angledEnvironmentInstructions.endMessageDialog);
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.openEnvironmentInstructions.startMessageDialog);
    //         foreach (var objectSearch in practiceSessionInstructions.openEnvironmentInstructions.objectsToSearch)
    //         {
    //             dialogPrefabsSequence.Add(objectSearch.objectSearchInstructionDialog);
    //             dialogPrefabsSequence.Add(objectSearch.objectFoundDialog);
    //         }
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.openEnvironmentInstructions.endMessageDialog);
    //         
    //         // // Add object search instructions
    //         // currentDialogSequence.Add("OpenObjectCube");
    //         // currentDialogSequence.Add("OpenObjectSphere");
    //         // currentDialogSequence.Add("OpenObjectStar");
    //         // currentDialogSequence.Add("OpenObjectStatue");
    //         //
    //         // Add practice end dialog
    //         currentDialogSequence.Add("PracticeEnd");
    //         dialogPrefabsSequence.Add(practiceSessionInstructions.practiceSessionEndDialog);
    //     }
    //     else
    //     {
    //         // Experimental session sequence
    //         currentDialogSequence.Add("experiment_briefing");
    //     
    //         // Add locomotion-specific instructions if needed for experimental sessions
    //         switch (locomotionMethodFromUI.ToLower())
    //         {
    //             case "continuous":
    //                 currentDialogSequence.Add("continuous_method_instructions");
    //                 break;
    //             case "teleport":
    //                 currentDialogSequence.Add("teleport_method_instructions");
    //                 break;
    //             case "node":
    //                 currentDialogSequence.Add("node_method_instructions");
    //                 break;
    //         }
    //     
    //         currentDialogSequence.Add("experiment_end");
    //     }
    //     
    //     // ShowDialog(0);
    //     ShowDialogPrefab(welcomeDialog);
    //     Debug.Log($"Built dialog sequence with {dialogPrefabsSequence.Count} dialogs for {locomotionMethodFromUI} locomotion (Practice: {isPractice})");
    // }
    
    // private GameObject FindDialogPrefab(string dialogKey)
    // {
    //     foreach (var category in dialogCategories)
    //     {
    //         foreach (var sequence in category.sequences)
    //         {
    //             // Check if the sequence name matches the dialog key
    //             if (sequence.sequenceName.ToLower().Replace(" ", "_") == dialogKey)
    //             {
    //                 // Return first prefab in the sequence
    //                 if (sequence.dialogPrefabs.Count > 0 && sequence.dialogPrefabs[0] != null)
    //                 {
    //                     return sequence.dialogPrefabs[0];
    //                 }
    //             }
    //         
    //             // Also check individual prefabs by index if you want to support that
    //             foreach (var prefab in sequence.dialogPrefabs)
    //             {
    //                 if (prefab != null && prefab.name.ToLower().Contains(dialogKey.ToLower()))
    //                 {
    //                     return prefab;
    //                 }
    //             }
    //         }
    //     }
    //
    //     Debug.LogError($"VRDialogFlowManager: Dialog prefab not found for key: {dialogKey}");
    //     return null;
    // }
    
    // private void ShowDialog(int dialogIndex)
    // {
    //     if (dialogIndex < 0 || dialogIndex >= currentDialogSequence.Count)
    //     {
    //         Debug.LogError($"VRDialogFlowManager: Invalid dialog index {dialogIndex}");
    //         return;
    //     }
    //     
    //     string dialogKey = currentDialogSequence[dialogIndex];
    //     GameObject dialogPrefab = FindDialogPrefab(dialogKey);
    //     
    //     if (dialogPrefab == null)
    //     {
    //         Debug.LogError($"VRDialogFlowManager: Dialog prefab not found for key: {dialogKey}");
    //         return;
    //     }
    //     
    //     StartCoroutine(ShowDialogCoroutine(dialogIndex, dialogPrefab));
    // }
    
    // public void ShowTriggeredDialog(string dialogKey, bool showImmediately = true)
    // {
    //     GameObject dialogPrefab = FindDialogPrefab(dialogKey);
    //
    //     if (dialogPrefab == null)
    //     {
    //         Debug.LogError($"VRDialogFlowManager: Cannot show triggered dialog - prefab not found for key: {dialogKey}");
    //         return;
    //     }
    //
    //     if (showImmediately)
    //     {
    //         StartCoroutine(ShowTriggeredDialogCoroutine(dialogPrefab, dialogKey));
    //     }
    //     else
    //     {
    //         // Add to current sequence at next position
    //         InjectDialogToSequence(dialogKey);
    //     }
    // }
    
    // private IEnumerator ShowTriggeredDialogCoroutine(GameObject dialogPrefab, string dialogKey)
    // {
    //     // Store current state if there's an active dialog
    //     GameObject previousDialog = currentDialogInstance;
    //     int savedDialogIndex = currentDialogIndex; // Save the current index
    //     
    //     isTransitioning = true;
    //
    //     // Hide current dialog if exists
    //     if (previousDialog != null)
    //     {
    //         yield return StartCoroutine(FadeDialog(previousDialog, false));
    //         Destroy(previousDialog); // Destroy the previous dialog since we won't restore it
    //     }
    //
    //     // Calculate position and rotation
    //     CalculateDialogTransform();
    //
    //     // Instantiate triggered dialog
    //     currentDialogInstance = Instantiate(dialogPrefab, dialogCanvas.transform);
    //     Debug.Log($"Showing triggered dialog: {dialogKey}");
    //
    //     // Set transform to calculated position
    //     var rectTransform = currentDialogInstance.GetComponent<RectTransform>();
    //     if (rectTransform != null)
    //     {
    //         rectTransform.position = targetPosition;
    //         rectTransform.rotation = targetRotation;
    //         rectTransform.localScale = Vector3.one * dialogScale;
    //     }
    //     else
    //     {
    //         currentDialogInstance.transform.position = targetPosition;
    //         currentDialogInstance.transform.rotation = targetRotation;
    //         currentDialogInstance.transform.localScale = Vector3.one * dialogScale;
    //     }
    //
    //     Canvas.ForceUpdateCanvases();
    //
    //     // Fade in triggered dialog
    //     yield return StartCoroutine(FadeDialog(currentDialogInstance, true));
    //
    //     isTransitioning = false;
    //
    //     // Wait for input to continue
    //     yield return StartCoroutine(WaitForAdvanceInput());
    //
    //     // Hide triggered dialog
    //     yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
    //     Destroy(currentDialogInstance);
    //     currentDialogInstance = null;
    //
    //     // // Restore previous dialog if it existed and flow is not paused
    //     // if (previousDialog != null && !isDialogFlowPaused)
    //     // {
    //     //     currentDialogInstance = previousDialog;
    //     //     yield return StartCoroutine(FadeDialog(currentDialogInstance, true));
    //     // }
    //     // else if (isDialogFlowPaused)
    //     // {
    //     //     currentDialogInstance = null;
    //     // }
    //     
    //     // Always trigger completion event for triggered dialogs
    //     // Let TrialManager decide what to do next
    //     OnSpecificDialogComplete?.Invoke(dialogKey);
    //
    //     isTransitioning = false;
    // }

    // public void InjectDialogToSequence(string dialogKey)
    // {
    //     int insertIndex = currentDialogIndex + 1;
    //     currentDialogSequence.Insert(insertIndex, dialogKey);
    //     Debug.Log($"Injected dialog '{dialogKey}' into sequence at position {insertIndex}");
    // }

    // public void PauseDialogFlow()
    // {
    //     if (!isDialogFlowPaused)
    //     {
    //         isDialogFlowPaused = true;
    //         pausedDialogSequence = new List<string>(currentDialogSequence);
    //         pausedDialogIndex = currentDialogIndex;
    //         Debug.Log("Dialog flow paused");
    //     }
    // }

    // public void ResumeDialogFlow()
    // {
    //     if (isDialogFlowPaused)
    //     {
    //         isDialogFlowPaused = false;
    //         currentDialogSequence = pausedDialogSequence;
    //         currentDialogIndex = pausedDialogIndex;
    //     
    //         if (currentDialogIndex < currentDialogSequence.Count)
    //         {
    //             ShowDialog(currentDialogIndex);
    //         }
    //     
    //         Debug.Log("Dialog flow resumed");
    //     }
    // }
    
    // private IEnumerator ShowDialogCoroutine(int dialogIndex, GameObject dialogPrefab)
    // {
    //     isTransitioning = true;
    //     
    //     // Store the previous dialog's transform before destroying it
    //     Vector3 previousPosition = targetPosition;
    //     Quaternion previousRotation = targetRotation;
    //     
    //     // Hide current dialog with fade out
    //     if (currentDialogInstance != null)
    //     {
    //         previousPosition = currentDialogInstance.transform.position;
    //         previousRotation = currentDialogInstance.transform.rotation;
    //         yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
    //         Destroy(currentDialogInstance);
    //     }
    //     
    //     // Update index
    //     currentDialogIndex = dialogIndex;
    //     
    //     // Calculate position and rotation
    //     CalculateDialogTransform();
    //     
    //     // Instantiate new dialog
    //     currentDialogInstance = Instantiate(dialogPrefab, dialogCanvas.transform);
    //     string currentDialogKey = currentDialogSequence[dialogIndex];
    //     Debug.Log($"Showing dialog: {currentDialogSequence[dialogIndex]}");
    //     
    //     // Immediately set transform without any interpolation
    //     var rectTransform = currentDialogInstance.GetComponent<RectTransform>();
    //     if (rectTransform != null)
    //     {
    //         rectTransform.position = previousPosition;
    //         rectTransform.rotation = previousRotation;
    //         rectTransform.localScale = Vector3.one * dialogScale;
    //     }
    //     else
    //     {
    //         currentDialogInstance.transform.position = previousPosition;
    //         currentDialogInstance.transform.rotation = previousRotation;
    //         currentDialogInstance.transform.localScale = Vector3.one * dialogScale;
    //     }
    //     
    //     // Force immediate transform update
    //     Canvas.ForceUpdateCanvases();
    //     
    //     // Fade in new dialog
    //     yield return StartCoroutine(FadeDialog(currentDialogInstance, true));
    //     
    //     isTransitioning = false;
    //     
    //     // Check if this is a special dialog that should trigger trial actions
    //     if (TriggerTrialAction(currentDialogKey))
    //     {
    //         // Wait for input
    //         yield return StartCoroutine(WaitForAdvanceInput());
    //     
    //         // Hide dialog immediately
    //         yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
    //         Destroy(currentDialogInstance);
    //         currentDialogInstance = null;
    //     
    //         // Trigger the specific dialog completion event
    //         OnSpecificDialogComplete?.Invoke(currentDialogKey);
    //     
    //         // Don't continue with normal flow - let TrialManager handle what's next
    //         yield break;
    //     }
    //     
    //     // Invoke event
    //     OnDialogChanged?.Invoke(currentDialogIndex);
    //     
    //     // Start follow behavior
    //     if (enableFollowBehavior)
    //     {
    //         StartFollowBehavior();
    //     }
    // }

    
    
    // // Check if the dialog should trigger trial actions instead of continuing the flow
    // private bool TriggerTrialAction(string dialogKey)
    // {
    //     // List of dialogs that should trigger trial actions instead of continuing flow
    //     string[] actionTriggerDialogs = {
    //         "CurvedEnvironmentInstructions",
    //         "AngledEnvironmentInstructions", 
    //         "OpenEnvironmentInstructions",
    //         "OpenObjectCube",
    //         "OpenObjectSphere", 
    //         "OpenObjectStar",
    //         "OpenObjectStatue"
    //     };
    //
    //     return System.Array.Exists(actionTriggerDialogs, dialog => dialog == dialogKey);
    // }
    //
    // // Resume dialog flow from a specific index after trial completion
    // public void ResumeDialogFlowFromIndex(int index)
    // {
    //     if (index >= 0 && index < currentDialogSequence.Count)
    //     {
    //         currentDialogIndex = index;
    //         ShowDialog(currentDialogIndex);
    //     }
    // }
    
    // // Method to continue with the next dialog in sequence
    // public void ContinueToNextDialog()
    // {
    //     if (currentDialogIndex < currentDialogSequence.Count - 1)
    //     {
    //         ShowDialog(currentDialogIndex + 1);
    //         ShowDialogPrefab(dialogPrefabsSequence[currentDialogIndex + 1]);
    //     }
    //     else
    //     {
    //         CompleteDialogFlow();
    //         CompleteDialogPrefabFlow();
    //     }
    // }
    
    // private void AdvanceDialog()
    // {
    //     if (isTransitioning) return;
    //     
    //     if (currentDialogIndex < currentDialogSequence.Count - 1)
    //     {
    //         // Recalculate position based on current camera view before showing next dialog
    //         // CalculateCanvasTransform();
    //         CalculateDialogTransform();
    //         // Show next dialog
    //         ShowDialog(currentDialogIndex + 1);
    //     }
    //     else
    //     {
    //         // All dialogs complete
    //         CompleteDialogFlow();
    //     }
    // }
    
    // private void CompleteDialogFlow()
    // {
    //     StartCoroutine(CompleteDialogFlowCoroutine());
    // }
    
    // private IEnumerator CompleteDialogFlowCoroutine()
    // {
    //     isTransitioning = true;
    //     
    //     // Stop follow behavior
    //     StopFollowBehavior();
    //     
    //     // Fade out final dialog
    //     if (currentDialogInstance != null)
    //     {
    //         yield return StartCoroutine(FadeDialog(currentDialogInstance, false));
    //         Destroy(currentDialogInstance);
    //         currentDialogInstance = null;
    //     }
    //     
    //     trialStarted = true;
    //     
    //     // Invoke completion events
    //     OnDialogFlowComplete?.Invoke();
    //     OnExperimentStart?.Invoke();
    //     
    //     Debug.Log("VR Dialog Flow Complete - Experiment Starting");
    // }
    
    // Public methods for external control
    // public void RestartDialogFlow()
    // {
    //     StopAllCoroutines();
    //     followCoroutine = null;
    //     
    //     if (currentDialogInstance != null)
    //     {
    //         Destroy(currentDialogInstance);
    //     }
    //     
    //     // currentDialogIndex = 0;
    //     isTransitioning = false;
    //     trialStarted = false;
    //     
    //     // currentDialogSequence.Clear();
    //     // currentDialogSequence.Add("WelcomeDialog"); 
    //     // ShowDialog(0);
    //     
    //     dialogPrefabsSequence.Clear();
    //     dialogPrefabsSequence.Add(welcomeDialog);
    //     ShowDialogPrefab(dialogPrefabsSequence[0]);
    // }
}
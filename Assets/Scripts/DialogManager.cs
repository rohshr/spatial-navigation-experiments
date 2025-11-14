using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogManager : MonoBehaviour
{
    [Header("Dialog Configuration")]
    [SerializeField] private List<DialogData> allDialogs = new List<DialogData>();
    [SerializeField] private Transform dialogParent;
    [SerializeField] private InputActionReference proceedInput;
    
    [Header("Display Settings")]
    [SerializeField] private float dialogDistance = 2f;
    [SerializeField] private float fadeSpeed = 3f;
    
    private Camera playerCamera;
    private GameObject currentDialog;
    private Queue<DialogData> dialogQueue = new Queue<DialogData>();
    private bool isWaitingForInput;
    
    // Events
    public static event Action<string> OnDialogCompleted;
    public static event Action OnAllDialogsCompleted;
    
    private void Start()
    {
        // playerCamera = Camera.main;
        // if (playerCamera == null)
        //     playerCamera = FindFirstObjectByType<Camera>();
        //     
        proceedInput.action.performed += OnProceedPressed;
    }
    
    private void OnDestroy()
    {
        proceedInput.action.performed -= OnProceedPressed;
    }
    
    // Simple method to show a single dialog
    public void ShowDialog(string dialogKey)
    {
        DialogData dialog = GetDialogByKey(dialogKey);
        if (dialog != null)
        {
            StartCoroutine(DisplayDialogCoroutine(dialog));
        }
    }
    
    // Method to show multiple dialogs in sequence
    public void ShowDialogSequence(List<string> dialogKeys)
    {
        dialogQueue.Clear();
        foreach (string key in dialogKeys)
        {
            DialogData dialog = GetDialogByKey(key);
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
        while (dialogQueue.Count > 0)
        {
            DialogData nextDialog = dialogQueue.Dequeue();
            yield return StartCoroutine(DisplayDialogCoroutine(nextDialog));
        }
        
        OnAllDialogsCompleted?.Invoke();
    }
    
    private IEnumerator DisplayDialogCoroutine(DialogData dialog)
    {
        // Hide current dialog
        if (currentDialog != null)
        {
            yield return StartCoroutine(HideDialog(currentDialog));
        }
        
        // Show new dialog
        currentDialog = Instantiate(dialog.dialogPrefab, dialogParent);
        PositionDialog();
        
        yield return StartCoroutine(ShowDialog(currentDialog));
        
        // Wait for player input
        isWaitingForInput = true;
        yield return new WaitUntil(() => !isWaitingForInput);
        
        // Hide dialog
        yield return StartCoroutine(HideDialog(currentDialog));
        
        // Fire completion event
        OnDialogCompleted?.Invoke(dialog.dialogKey);
    }
    
    private void PositionDialog()
    {
        if (currentDialog == null || playerCamera == null) return;
        
        Vector3 cameraPos = playerCamera.transform.position;
        Vector3 cameraForward = playerCamera.transform.forward;
        
        currentDialog.transform.position = cameraPos + cameraForward * dialogDistance;
        currentDialog.transform.LookAt(cameraPos);
        currentDialog.transform.Rotate(0, 180, 0); // Face player
    }
    
    private IEnumerator ShowDialog(GameObject dialog)
    {
        CanvasGroup canvasGroup = dialog.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = dialog.AddComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator HideDialog(GameObject dialog)
    {
        CanvasGroup canvasGroup = dialog.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;
        
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        Destroy(dialog);
    }
    
    private void OnProceedPressed(InputAction.CallbackContext context)
    {
        if (isWaitingForInput)
        {
            isWaitingForInput = false;
        }
    }
    
    private DialogData GetDialogByKey(string key)
    {
        return allDialogs.Find(d => d.dialogKey == key);
    }
}

[System.Serializable]
public class DialogData
{
    public string dialogKey;
    public GameObject dialogPrefab;
}
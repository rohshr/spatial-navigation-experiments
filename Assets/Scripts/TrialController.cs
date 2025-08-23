using System.Collections.Generic;
using UnityEngine;
using UXF;

public class TrialController : MonoBehaviour
{
    [Header("Trial Configuration")]
    [SerializeField] private List<TrialStep> trialSteps = new List<TrialStep>();
    [SerializeField] private Transform player;
    [SerializeField] private Transform uiViewpoint;
    
    private int currentStepIndex = 0;
    private DialogManager dialogManager;
    
    private void Start()
    {
        dialogManager = FindFirstObjectByType<DialogManager>();
        StartTrialSequence();
    }
    
    private void OnEnable()
    {
        DialogManager.OnDialogCompleted += HandleDialogCompleted;
        FinishPointCheck.OnFinishPointReached += HandleFinishPointReached;
    }
    
    private void OnDisable()
    {
        DialogManager.OnDialogCompleted -= HandleDialogCompleted;
        FinishPointCheck.OnFinishPointReached -= HandleFinishPointReached;
    }
    
    private void StartTrialSequence()
    {
        if (currentStepIndex < trialSteps.Count)
        {
            ExecuteCurrentStep();
        }
    }
    
    private void ExecuteCurrentStep()
    {
        TrialStep step = trialSteps[currentStepIndex];
        
        switch (step.stepType)
        {
            case StepType.ShowDialog:
                MovePlayerTo(uiViewpoint);
                dialogManager.ShowDialog(step.dialogKey);
                break;
                
            case StepType.StartTrial:
                MovePlayerTo(step.spawnPoint);
                Session.instance.BeginNextTrial();
                break;
        }
    }
    
    private void HandleDialogCompleted(string dialogKey)
    {
        Debug.Log($"Dialog completed: {dialogKey}");
        AdvanceToNextStep();
    }
    
    private void HandleFinishPointReached()
    {
        // End UXF trial first
        if (Session.instance.InTrial)
        {
            Session.instance.CurrentTrial.End();
        }
        
        // Move to UI viewpoint
        MovePlayerTo(uiViewpoint);
        
        // Show completion dialog
        TrialStep currentStep = trialSteps[currentStepIndex];
        if (!string.IsNullOrEmpty(currentStep.completionDialogKey))
        {
            dialogManager.ShowDialog(currentStep.completionDialogKey);
        }
        else
        {
            AdvanceToNextStep();
        }
    }
    
    private void AdvanceToNextStep()
    {
        currentStepIndex++;
        
        if (currentStepIndex < trialSteps.Count)
        {
            ExecuteCurrentStep();
        }
        else
        {
            Debug.Log("All trial steps completed!");
        }
    }
    
    private void MovePlayerTo(Transform target)
    {
        if (player != null && target != null)
        {
            player.SetPositionAndRotation(target.position, target.rotation);
        }
    }
}

[System.Serializable]
public class TrialStep
{
    public string stepName;
    public StepType stepType;
    public string dialogKey;           // Dialog to show for ShowDialog steps
    public string completionDialogKey; // Dialog to show after trial completion
    public Transform spawnPoint;       // Where to spawn player for StartTrial steps
}

public enum StepType
{
    ShowDialog,
    StartTrial
}
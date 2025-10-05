using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UXF;

[System.Serializable]
public class ObjectPointingTask
{
    public GameObject referenceObject;
    public GameObject targetObject;
}

public class PointingEstimationSessionGenerator : MonoBehaviour
{
    #region Inspector Fields
    [Header("---- SESSION SETTINGS ----")]
    [Space(5)]
    [Tooltip("Enable this to run the experiment in non-VR mode. Useful for testing without VR headset.")]
    [SerializeField] private bool nonVRMode = false;
    
    [Header("---- SESSION MESSAGE DIALOGS ----")]
    [Space(5)]
    [Tooltip("Dialog prefab to show while waiting for the session to start")]
    [SerializeField] private GameObject sessionWaitingDialogPrefab;
    
    [Tooltip("Dialog prefab to show when the session starts")]
    [SerializeField] private GameObject sessionStartDialogPrefab;
    
    [Tooltip("Dialog prefab to show when the session ends")]
    [SerializeField] private GameObject sessionEndDialogPrefab;
    
    [Tooltip("Instruction dialog prefab for pointing tasks")]
    [SerializeField] private GameObject instructionUIPrefab;
    
    [Header("---- Object Pointing Tasks ----")]
    [Tooltip("Sequence of object pointing tasks to include in the session")]
    [Space(5)]
    [SerializeField] public List<ObjectPointingTask> objectPointingTasks = new ();
    private int currentTaskIndex = 0;
    #endregion
    
    // Events
    public static event Action<GameObject> OnPlayStart; // Pass the dialog prefab to show before the start of the session
    public static event Action<List<GameObject>> OnSessionGenerate; // Pass the list of dialog prefabs to show at the start of the session
    public static event Action<List<GameObject>> OnBlockEnd; // Pass the list of dialog prefabs to show at the end of each block
    public static event Action<List<GameObject>> OnTrialEnd; // Pass the list of dialog prefabs to show at the end of each trial (for object search tasks)
    public static event Action<GameObject> OnSessionEnd; // Pass the dialog prefab to show at the end of the session
    public static event Action<GameObject> OnShowNextInstruction;
    
    
    // Private variables
    
    
    void Start()
    {
        OnPlayStart?.Invoke(sessionWaitingDialogPrefab);
    }

    private void OnEnable()
    {
        // TrialManager.OnExplorationBlockCompleted += ShowNextInstructions;
        // InputHandler.SkipTrialEvent += ShowNextInstructions;
    }
    
    private void OnDisable()
    {
        // TrialManager.OnExplorationBlockCompleted -= ShowNextInstructions;
        // InputHandler.SkipTrialEvent -= ShowNextInstructions;
    }
    
    // Session Start
    public void GenerateExperiment(Session session)
    {
        currentTaskIndex = 0;
        ConfigureSessionSettings(session);
        var instructionSequence = BuildSessionInstructionSequence();
        
        // Invoke event to send the instruction sequence to VRDialogFlowManager
        OnSessionGenerate?.Invoke(instructionSequence);
        
        CreateObjectPointingBlock(session);
    }
    
    // Getters

    
    public void EndExperiment()
    {
        Debug.Log("Starting end session delay...");
        // Wait for 5 seconds before ending the session
        StartCoroutine(EndSessionAfterDelay(5f));
    }

    #region Private Methods
    private void ConfigureSessionSettings(Session session)
    {
        
    }
    
    /// <summary>
    /// Build the sequence of instruction dialogs to show at the start of the session.
    /// This includes the session start dialog and the instruction dialog for the first task.
    /// </summary>
    /// <returns>Ordered list of instruction/message dialog prefabs.</returns>
    private List<GameObject> BuildSessionInstructionSequence()
    {
        var instructionSequence = new List<GameObject> { sessionStartDialogPrefab };
        instructionSequence.Add(GetCurrentTaskInstruction());
        return instructionSequence;
    }
    
    /// <summary>
    /// Create UXF block for the pointing tasks session.
    /// </summary>
    /// <param name="session"></param>
    private void CreateObjectPointingBlock(Session session)
    {
        session.CreateBlock(objectPointingTasks.Count);
    }
    
    public GameObject CreateInstructionUI(string text)
    {
        // Basic instantiation
        GameObject instance = Instantiate(instructionUIPrefab);
    
        // Modify the text
        TextMeshProUGUI textComponent = instance.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    
        return instance;
    }
    
    /// <summary>
    /// Show the next instruction dialog.
    /// </summary>
    private GameObject GetCurrentTaskInstruction()
    {
        var instructionText = "Using " + objectPointingTasks[currentTaskIndex].referenceObject.name + " as reference, point to the " + objectPointingTasks[currentTaskIndex].targetObject.name;
        GameObject instructionUI = CreateInstructionUI(instructionText);
        currentTaskIndex++;
        return instructionUI;
    }
    
    /// <summary>
    /// End the session after a specified delay.
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator EndSessionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Session ended.");
        Session.instance.End();
    }
    #endregion

}

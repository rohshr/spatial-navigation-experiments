using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UXF;

public class SessionGenerator : MonoBehaviour
{
    #region Inspector Fields
    [Header("---- LOCOMOTION METHOD INSTRUCTIONS ----")]
    [Space(5)]
    [Tooltip("Specify the dialog prefab with instructions for continuous locomotion method.")]
    [SerializeField] private GameObject continuousLocomotionInstructionDialogPrefab;
    
    [Tooltip("Specify the dialog prefab with instructions for teleport locomotion method.")]
    [SerializeField] private GameObject teleportLocomotionInstructionDialogPrefab;
    
    [Tooltip("Specify the dialog prefab with instructions for node-based locomotion method.")]
    [SerializeField] private GameObject nodeLocomotionInstructionDialogPrefab;

    [Header("---- SESSION SETTINGS ----")]
    [Space(5)]
    [Tooltip("Enable this to run the experiment in non-VR mode. Useful for testing without VR headset.")]
    [SerializeField] private bool nonVRMode = false;
    
    [Tooltip("Check to label this session as practice. Practice sessions may have different settings.")]
    [SerializeField] private bool isPracticeSession = false;
    
    [Header("---- SESSION MESSAGE DIALOGS ----")]
    [Space(5)]
    [Tooltip("Dialog prefab to show while waiting for the session to start")]
    [SerializeField] private GameObject sessionWaitingDialogPrefab;
    
    [Tooltip("Dialog prefab to show when the session starts")]
    [SerializeField] private GameObject sessionStartDialogPrefab;
    
    [Tooltip("Dialog prefab to show when the session ends")]
    [SerializeField] private GameObject sessionEndDialogPrefab;
    
    // TODO: Create scriptable object for experiment blocks
    [Header("---- EXPERIMENT BLOCKS CONFIGURATION ----")]
    [Tooltip("Mixed list of blocks that can be either Generic or ObjectSearch blocks")]
    [Space(5)]
    [SerializeReference] private List<LocomotionExperimentBlock> experimentBlocks = new ();
    #endregion
    
    // Events
    public static event Action<GameObject> OnPlayStart; // Pass the dialog prefab to show before the start of the session
    public static event Action<List<GameObject>> OnSessionGenerate; // Pass the list of dialog prefabs to show at the start of the session
    public static event Action<List<GameObject>> OnBlockEnd; // Pass the list of dialog prefabs to show at the end of each block
    public static event Action<List<GameObject>> OnTrialEnd; // Pass the list of dialog prefabs to show at the end of each trial (for object search tasks)
    public static event Action<GameObject> OnSessionEnd; // Pass the dialog prefab to show at the end of the session
    
    // Private variables
    private string locomotionMethodFromUI;
    private string preferredHandFromUI;
    private ObjectSearchHandler objectSearchHandler = new ObjectSearchHandler();
    
    
    void Start()
    {
        OnPlayStart?.Invoke(sessionWaitingDialogPrefab);
    }

    private void OnEnable()
    {
        FinishPointCheck.OnFinishPointReached += ShowNextInstructions;
        ObjectCollisionDetection.OnObjectFound += ShowNextObjectSearchInstructions;
        TrialManager.OnExplorationBlockCompleted += ShowNextInstructions;
        InputHandler.ProceedTrialEvent += ShowNextInstructions;
        ExperimenterControlScript.OnTrialSkipped += ForceSkipTrial;
        SpawnPointCheck.OnPlayerExitedSpawnPoint += SetupGuidedExplorationFinishPoint;
        FinishPointCheck.OnPlayerFinishedGuidedExploration += EndGuidedExploration;
    }
    
    private void OnDisable()
    {
        FinishPointCheck.OnFinishPointReached -= ShowNextInstructions;
        ObjectCollisionDetection.OnObjectFound -= ShowNextObjectSearchInstructions;
        TrialManager.OnExplorationBlockCompleted -= ShowNextInstructions;
        InputHandler.ProceedTrialEvent -= ShowNextInstructions;
        ExperimenterControlScript.OnTrialSkipped -= ForceSkipTrial;
        SpawnPointCheck.OnPlayerExitedSpawnPoint -= SetupGuidedExplorationFinishPoint;
        FinishPointCheck.OnPlayerFinishedGuidedExploration -= EndGuidedExploration;
    }
    
    // Session Start
    public void GenerateExperiment(Session session)
    {
        // Reset the object search handler for a new session
        objectSearchHandler.Reset();
        
        ConfigureSessionSettings(session);
        ConfigureLocomotion();
        var instructionSequence = BuildSessionInstructionSequence();
        
        // Invoke event to send the instruction sequence to VRDialogFlowManager
        OnSessionGenerate?.Invoke(instructionSequence);
        
        CreateBlocks(session);
    }
    
    // Getters

    /// <summary>
    /// Get the list of trial blocks defined for the session.
    /// </summary>
    /// <returns></returns>
    public List<LocomotionExperimentBlock> GetExperimentBlocks() => experimentBlocks;
    
    /// <summary>
    /// Get the spawn points sequence for the environments in the blocks.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetSpawnPointsSequence()
    {
        var spawnPointsSequence = new List<GameObject>();
        foreach (var block in experimentBlocks)
        {
            if (block?.environmentSpawnPoint != null)
            {
                spawnPointsSequence.Add(block.environmentSpawnPoint);
            }
            else
            {
                Debug.LogWarning($"Block {block} does not have an spawn point assigned.");
            }
        }
        return spawnPointsSequence;
    }
    
    /// <summary>
    /// Get the instrcutions sequence for an object searches in an object search block.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetObjectSearchSequence(LocomotionExperimentBlock block)
    {
        var objectSearchSequence = new List<GameObject>();
        
        if (block is not ObjectSearchBlock objectSearchBlock)
        {
            Debug.LogWarning($"Block {block} is not an ObjectSearchBlock.");
            return new List<GameObject>();
        }

        objectSearchSequence.AddRange(
            objectSearchBlock.objectSearchTasks
                .Where(task => task.objectToFind != null)
                .Select(task => task.objectToFind)
        );

        return objectSearchSequence;
    }
    
    /// <summary>
    /// Return the current object that the participant needs to find in an object search task.
    /// </summary>
    /// <returns>GameObject for object in current object search task.</returns>
    public GameObject GetCurrentObjectToFind() => objectSearchHandler.CurrentObjectToFind;

    public string GetCurrentBlockType()
    {
        if (Session.instance != null)
        {
            return (experimentBlocks[Session.instance.CurrentBlock.number - 1].GetBlockType());
        }
        return null;
    }
    
    //// <summary>
    /// Get the spawn location for the current object search task using a specific block
    /// </summary>
    /// <param name="block">The block to get the spawn location from</param>
    /// <returns>GameObject for the current object search task spawn location, or null if not applicable</returns>
    public GameObject GetCurrentObjectSearchSpawnLocation(LocomotionExperimentBlock block)
    {
        return objectSearchHandler.GetCurrentObjectSearchSpawnLocation(block);
    }

    
    public void EndExperiment()
    {
        Debug.Log("Starting end session delay...");
        // Wait for 5 seconds before ending the session
        StartCoroutine(EndSessionAfterDelay(5f));
    }

    #region Private Methods
    private void ConfigureSessionSettings(Session session)
    {
        session.participantDetails["is_practice"] = isPracticeSession;
        locomotionMethodFromUI = session.participantDetails["locomotion_method"].ToString().ToLower();
        preferredHandFromUI = session.participantDetails["preferred_hand"].ToString().ToLower();
        
        // Logging session details into session data
        session.settings.SetValue("locomotion_method", locomotionMethodFromUI);
    }
    
    /// <summary>
    /// Setup the floors and locomotion controls based on the selected locomotion method.
    /// </summary>
    private void ConfigureLocomotion()
    {
        LocomotionManager.UpdateFloors(locomotionMethodFromUI);

        if (!nonVRMode)
        {
            InputHandler.UpdateLocomotionControls(locomotionMethodFromUI);
        }
        else
        {
            Debug.Log("Dev mode is enabled. Skipping InputHandler.UpdateLocomotionControls()");
        }
    }
    
    /// <summary>
    /// Show the appropriate locomotion instruction dialog based on the selected locomotion method.
    /// </summary>
    /// <param name="locomotionMethod"></param>
    /// <returns>Prefab for the appropriate locomotion instruction dialog.</returns>
    private GameObject GetLocomotionInstructionDialog(string locomotionMethod)
    {
        return locomotionMethod switch
        {
            "continuous" => continuousLocomotionInstructionDialogPrefab,
            "teleport" => teleportLocomotionInstructionDialogPrefab,
            "nodebased" => nodeLocomotionInstructionDialogPrefab,
            _ => null
        };
    }
    
    /// <summary>
    /// Build the sequence of instruction dialogs to show at the start of the session.
    /// This includes the session start dialog, locomotion method instructions, and the start message of the first block.
    /// </summary>
    /// <returns>Ordered list of instruction/message dialog prefabs.</returns>
    private List<GameObject> BuildSessionInstructionSequence()
    {
        var instructionSequence = new List<GameObject> { sessionStartDialogPrefab };
        
        var locomotionMethodInstructionDialog = GetLocomotionInstructionDialog(locomotionMethodFromUI);
        
        if (locomotionMethodInstructionDialog != null)
            instructionSequence.Add(locomotionMethodInstructionDialog);
        else
            Debug.LogWarning($"No locomotion instruction dialog found for method: {locomotionMethodFromUI}");

        if (experimentBlocks.Count > 0 && experimentBlocks[0]?.startMessageDialogPrefab != null)
        {
            instructionSequence.Add(experimentBlocks[0].startMessageDialogPrefab);
            var objectSearchInstructions = objectSearchHandler.GetInitialObjectSearchInstructions(experimentBlocks[0]);
            instructionSequence.AddRange(objectSearchInstructions);
        }
        else if (experimentBlocks.Count == 0)
        {
            Debug.LogWarning("No blocks defined in the session generator.");
        }

        return instructionSequence;
    }
    
    /// <summary>
    /// Create UXF blocks based on the defined trial blocks and their configurations.
    /// </summary>
    /// <param name="session"></param>
    private void CreateBlocks(Session session)
    {
        foreach (var block in experimentBlocks)
        {
            if (!block.skipBlock)
            {
                var newBlock = session.CreateBlock(block.GetTrialCount());
                newBlock.settings.SetValue("block_type", block.GetBlockType());
                newBlock.settings.SetValue("environment", block.environment.ToString().ToLower());
            }
        }
    }
    
    /// <summary>
    /// Add the end message of the previous block and the start message of the next block (if they exist) to the instruction sequence.
    /// </summary>
    private void ShowNextInstructions()
    {
        var instructionSequence = new List<GameObject>();
        var nextBlockIndex = Session.instance.CurrentBlock.number; // CurrentBlock.number is 1-based index
        var previousBlockIndex = nextBlockIndex - 1;

        if (previousBlockIndex >= 0)
        {
            var lastBlock = experimentBlocks[previousBlockIndex];
            if (lastBlock?.endMessageDialogPrefab != null)
            {
                instructionSequence.Add(lastBlock.endMessageDialogPrefab);
            }
        }
        if (nextBlockIndex < Session.instance.blocks.Count)
        {
            var nextBlock = experimentBlocks[nextBlockIndex];
            
            if (nextBlock?.startMessageDialogPrefab != null)
            {
                instructionSequence.Add(nextBlock.startMessageDialogPrefab);
            }
            // If the next block is an ObjectSearch block, add the instructions for the first object search task.
            if (nextBlock?.GetBlockType() == "ObjectSearch")
            {
                objectSearchHandler.Reset();

                var objectSearchInstructions = objectSearchHandler.GetInitialObjectSearchInstructions(nextBlock);
                instructionSequence.AddRange(objectSearchInstructions);
            }
        }
        else
        {
            OnSessionEnd?.Invoke(sessionEndDialogPrefab);
        }
        OnBlockEnd?.Invoke(instructionSequence);
    }

    private void ShowNextObjectSearchInstructions()
    {
        var (objectSearchInstructions, isSessionComplete) = objectSearchHandler.GetNextObjectSearchInstructions(experimentBlocks); //isSessionComplete specifies if the session should complete after the last object search task/ when there is no more blocks after the current object search block.
        
        if (isSessionComplete)
        {
            OnSessionEnd?.Invoke(sessionEndDialogPrefab);
            // Add any remaining instructions first, then trigger session end
            if (objectSearchInstructions.Count > 0)
            {
                OnTrialEnd?.Invoke(objectSearchInstructions);
            }
            
            Debug.Log("No more blocks available. Session ended.");
            return;
        }
        
        OnTrialEnd?.Invoke(objectSearchInstructions);
    }
    
    private void SetupGuidedExplorationFinishPoint()
    {
        var currentBlock = experimentBlocks[Session.instance.CurrentBlock.number - 1] as GuidedExplorationBlock;
        currentBlock?.EnableFinishPoint(this);
    }

    private void EndGuidedExploration()
    {
        var currentBlock = experimentBlocks[Session.instance.CurrentBlock.number - 1] as GuidedExplorationBlock;
        currentBlock?.DisableNavigationGuides();
        currentBlock?.DisableFinishPoint();
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

    private void ForceSkipTrial()
    {
        if (GetCurrentBlockType() == "ObjectSearch")
        {
            ShowNextObjectSearchInstructions();
        }
        else
        {
            ShowNextInstructions();
        }
    }
    #endregion

    // #region Context Menu Methods
    // [ContextMenu("Add Generic Block")]
    // private void AddGenericBlock()
    // {
    //     experimentBlocks.Add(new LocomotionExperimentBlock());
    // }
    //
    // [ContextMenu("Add Object Search Block")]
    // private void AddObjectSearchBlock()
    // {
    //     experimentBlocks.Add(new ObjectSearchBlock());
    // }
    //
    // [ContextMenu("Add Timed Exploration Block")]
    // private void AddTimedExplorationBlock()
    // {
    //     experimentBlocks.Add(new TimedExplorationBlock());
    // }
    // #endregion
}

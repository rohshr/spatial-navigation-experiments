using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UXF;

public class SessionGenerator : MonoBehaviour
{
    // The type of environment for the block
    public enum EnvironmentType { Curved, Angled, OpenSpace, Maze }
    
    [System.Serializable]
    public class TrialTask
    {
        public string taskName;
        public GameObject taskInstructionsDialogPrefab;
        public GameObject taskCompleteMessageDialogPrefab;
    }

    [System.Serializable]
    public class ObjectSearchTask : TrialTask
    {
        [Tooltip("Object to be found")]
        public GameObject objectToFind; // Reference to the object to be found
    }

    [System.Serializable]
    public class UXFBlock
    {
        [Tooltip("Unique name for UXF block")]
        public string blockName;
        
        [Header("Environment Configuration")]
        [Space(5)]
        [Tooltip("Type of environment for the block")]
        public EnvironmentType environment;
        [Tooltip("Reference to the environment spawn point")]
        public GameObject environmentSpawnPoint; // Reference to the environment spawn point
        [Tooltip("Reference to the environment finish point. Not applicable for tasks with multiple possible end points, like the object search task.")]
        public GameObject environmentFinishPoint; // Reference to the environment finish point
        
        [Header("Block Instructions Configuration")]
        [Space(5)]
        [Tooltip("Dialog prefab to show at the start of block")]
        public GameObject startMessageDialogPrefab;
        [Tooltip("Dialog prefab to show at the end of block")]
        public GameObject endMessageDialogPrefab;
        
        [Header("Trial Settings")]
        [Space(5)]
        [Tooltip("Enable this to randomize the order of trial tasks within the block. Only applicable if multiple tasks are defined.")]
        public bool randomizeTrialTasksSequence = false;
        
        public virtual int GetTrialCount() => 1;
        public virtual string GetBlockType() => "Generic";
        public virtual GameObject GetSpawnPoint() => environmentSpawnPoint;
    }
    
    [System.Serializable]
    public class ObjectSearchTrialsBlock : UXFBlock
    {
        [Header("Object Search Configuration")]
        [Tooltip("List of object search tasks to include in the block. Each task should specify the object to find and associated instructions.")]
        public List<ObjectSearchTask> objectSearchTasks = new List<ObjectSearchTask>();
        
        public override int GetTrialCount() => objectSearchTasks.Count;
        public override string GetBlockType() => "ObjectSearch";
        
        /// <summary>
        /// Get the instrcutions sequence for an object searches in an object search block.
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetObjectSearchSequence()
        {
            var objectSearchSequence = new List<GameObject>();

            objectSearchSequence.AddRange(
                objectSearchTasks
                    .Where(task => task.objectToFind != null)
                    .Select(task => task.objectToFind)
            );

            return objectSearchSequence;
        }
    }

    public class ExplorationBlock : UXFBlock
    {
        [Tooltip("Time to allow exploration in minutes")]
        public float timeForExploration = 5f; // Default to 5 minutes
        
        public override string GetBlockType() => "Exploration";
        public float GetTimeForExplorationInSeconds() => (timeForExploration * 60f);
    }
    
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
    
    // [Header("Block Configuration Old")]
    // [Tooltip("Specify block sequence and number of trials for each block")]
    // [SerializeField] private UXFBlock[] blocks;
    
    [Header("---- TRIAL BLOCKS CONFIGURATION ----")]
    [Tooltip("Mixed list of blocks that can be either Generic or ObjectSearch blocks")]
    [Space(5)]
    [SerializeReference] private List<UXFBlock> trialBlocks = new List<UXFBlock>();
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
    private int objectSearchIndex = 0;
    private GameObject objectToFind;
    
    void Start()
    {
        OnPlayStart?.Invoke(sessionWaitingDialogPrefab);
    }

    private void OnEnable()
    {
        FinishPointCheck.OnFinishPointReached += ShowNextInstructions;
        ObjectCollisionDetection.OnObjectCollided += ShowNextObjectSearchInstructions;
        TrialManager.OnExplorationBlockCompleted += ShowNextInstructions;
        InputHandler.SkipTrialEvent += ShowNextInstructions;
    }
    
    private void OnDisable()
    {
        FinishPointCheck.OnFinishPointReached -= ShowNextInstructions;
        ObjectCollisionDetection.OnObjectCollided -= ShowNextObjectSearchInstructions;
        TrialManager.OnExplorationBlockCompleted -= ShowNextInstructions;
        InputHandler.SkipTrialEvent -= ShowNextInstructions;
    }
    
    // Session Start
    public void GenerateExperiment(Session session)
    {
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
    public List<UXFBlock> GetTrialBlocks() => trialBlocks;
    
    /// <summary>
    /// Get the spawn points sequence for the environments in the blocks.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetSpawnPointsSequence()
    {
        var spawnPointsSequence = new List<GameObject>();
        foreach (var block in trialBlocks)
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
    public List<GameObject> GetObjectSearchSequence(UXFBlock block)
    {
        var objectSearchSequence = new List<GameObject>();
        
        if (block is not ObjectSearchTrialsBlock objectSearchBlock)
        {
            Debug.LogWarning($"Block {block} is not an ObjectSearchTrialsBlock.");
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
    public GameObject GetCurrentObjectToFind() => objectToFind;
    
    public void EndExperiment()
    {
        Debug.Log("Starting end session delay...");
        // Wait for 5 seconds before ending the session
        StartCoroutine(EndSessionAfterDelay(5f));
    }

    #region Private Methods
    private void ConfigureSessionSettings(Session session)
    {
        locomotionMethodFromUI = session.participantDetails["locomotion_method"].ToString().ToLower();
        preferredHandFromUI = session.participantDetails["preferred_hand"].ToString().ToLower();
        
        // Logging session details into session data
        session.settings.SetValue("is_practice", isPracticeSession);
        session.settings.SetValue("locomotion_method", locomotionMethodFromUI);
        session.settings.SetValue("preferred_hand", preferredHandFromUI);
    }
    
    /// <summary>
    /// Setup the floors and locomotion controls based on the selected locomotion method.
    /// </summary>
    private void ConfigureLocomotion()
    {
        LocomotionMethod.UpdateFloors(locomotionMethodFromUI);

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

        if (trialBlocks.Count > 0 && trialBlocks[0]?.startMessageDialogPrefab != null)
        {
            instructionSequence.Add(trialBlocks[0].startMessageDialogPrefab);
            if (trialBlocks[0]?.GetBlockType() == "ObjectSearch")
            {
                objectSearchIndex = 0;
                var objectSearchBlock = trialBlocks[0] as ObjectSearchTrialsBlock;
                objectToFind = objectSearchBlock?.objectSearchTasks[objectSearchIndex].objectToFind;
                var objectSearchInstruction = objectSearchBlock?.objectSearchTasks[objectSearchIndex]
                    .taskInstructionsDialogPrefab;
                if (objectSearchBlock != null && objectSearchBlock.objectSearchTasks.Count > 0)
                {
                    instructionSequence.Add(objectSearchInstruction);
                }
            }
        }
        else if (trialBlocks.Count == 0)
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
        foreach (var block in trialBlocks)
        {
            var newBlock = session.CreateBlock(block.GetTrialCount());
            newBlock.settings.SetValue("block_type", block.GetBlockType());
            newBlock.settings.SetValue("environment", block.environment.ToString().ToLower());
        }
    }
    
    /// <summary>
    /// Add the end message of the previous block and the start message of the next block (if they exist) to the instruction sequence.
    /// </summary>
    private void ShowNextInstructions()
    {
        var instructionsSequence = new List<GameObject>();
        var nextBlockIndex = Session.instance.CurrentBlock.number; // CurrentBlock.number is 1-based index
        var previousBlockIndex = nextBlockIndex - 1;

        if (previousBlockIndex >= 0)
        {
            var lastBlock = trialBlocks[previousBlockIndex];
            if (lastBlock?.endMessageDialogPrefab != null)
            {
                instructionsSequence.Add(lastBlock.endMessageDialogPrefab);
            }
        }
        if (nextBlockIndex < trialBlocks.Count)
        {
            var nextBlock = trialBlocks[nextBlockIndex];
            
            if (nextBlock?.startMessageDialogPrefab != null)
            {
                instructionsSequence.Add(nextBlock.startMessageDialogPrefab);
            }
            // If the next block is an ObjectSearch block, add the instructions for the first object search task.
            if (nextBlock?.GetBlockType() == "ObjectSearch")
            {
                objectSearchIndex = 0;
                var objectSearchBlock = nextBlock as ObjectSearchTrialsBlock;
                objectToFind = objectSearchBlock?.objectSearchTasks[objectSearchIndex].objectToFind;
                var objectSearchInstruction = objectSearchBlock?.objectSearchTasks[objectSearchIndex]
                    .taskInstructionsDialogPrefab;
                if (objectSearchBlock != null && objectSearchBlock.objectSearchTasks.Count > 0)
                {
                    instructionsSequence.Add(objectSearchInstruction);
                }
            }
        }
        OnBlockEnd?.Invoke(instructionsSequence);
    }
    
    // private void CheckTargetObjectFound()
    // {
    //     // if (collidedObject == currentObjectToFind)
    //     // {
    //     //     Debug.Log($"Correct object found: {collidedObject.name}");
    //         ShowNextInstructions();
    //         ShowNextObjectSearchInstructions();
    //         OnCorrectObjectFound?.Invoke();
    //     // }
    //     // else
    //     // {
    //     //     Debug.Log($"Incorrect object found: {collidedObject.name}. Expected: {currentObjectToFind?.name}");
    //     // }
    // }

    private void ShowNextObjectSearchInstructions()
    {
        // Debug.Log($"current block number: {Session.instance.CurrentBlock.number}, current object search index: {currentObjectSearchIndex}");
        if (Session.instance.CurrentBlock.number - 1 < 0 || Session.instance.CurrentBlock.number - 1 >= trialBlocks.Count)
        {
            Debug.LogWarning($"CurrentBlock.number {Session.instance.CurrentBlock.number} is out of bounds for trialBlocks (Count: {trialBlocks.Count}).");
            return;
        }
        var currentBlock = trialBlocks[Session.instance.CurrentBlock.number - 1];
        
        if (currentBlock?.GetBlockType() != "ObjectSearch")
        {
            Debug.LogWarning("Current block is not an ObjectSearch block.");
            return;
        }
        var objectSearchInstructions = new List<GameObject>();
        var previousObjectSearchIndex = objectSearchIndex;
        var previousObjectSearchTrial = (currentBlock as ObjectSearchTrialsBlock)
            ?.objectSearchTasks[previousObjectSearchIndex];
        if (previousObjectSearchIndex >=0 && previousObjectSearchTrial?.taskCompleteMessageDialogPrefab != null)
        {
            objectSearchInstructions.Add(previousObjectSearchTrial.taskCompleteMessageDialogPrefab);
        }
        
        var nextObjectSearchIndex = ++objectSearchIndex;
        if (nextObjectSearchIndex < currentBlock.GetTrialCount())
        {
            var nextObjectSearchTrial = (currentBlock as ObjectSearchTrialsBlock)
                ?.objectSearchTasks[nextObjectSearchIndex];
            if (nextObjectSearchTrial?.taskInstructionsDialogPrefab != null)
            {
                objectSearchInstructions.Add(nextObjectSearchTrial.taskInstructionsDialogPrefab);
                objectToFind = nextObjectSearchTrial.objectToFind;
                
            }
        }
        else
        {
            Debug.LogWarning("No more object search tasks in the current block.");
            // Check if there is a next block and add its start message if it exists
            var nextBlockIndex = Session.instance.CurrentBlock.number; // CurrentBlock.number is 1-based index
            if (nextBlockIndex < trialBlocks.Count)
            {
                var nextBlock = trialBlocks[nextBlockIndex];
                if (nextBlock?.startMessageDialogPrefab != null)
                {
                    objectSearchInstructions.Add(nextBlock.startMessageDialogPrefab);
                }
            }
            else
            {
                OnSessionEnd?.Invoke(sessionEndDialogPrefab);
                Debug.Log("No more blocks available. Session ended.");
                return;
            }
        }
        OnTrialEnd?.Invoke(objectSearchInstructions);
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

    #region Context Menu Methods
    [ContextMenu("Add Generic Block")]
    private void AddGenericBlock()
    {
        trialBlocks.Add(new UXFBlock());
    }

    [ContextMenu("Add Object Search Block")]
    private void AddObjectSearchBlock()
    {
        trialBlocks.Add(new ObjectSearchTrialsBlock());
    }
    #endregion
}

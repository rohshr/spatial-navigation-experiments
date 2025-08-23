using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UXF;
using NUnit.Framework.Constraints;
using UnityEditor.EditorTools;

public class SessionGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TrialTask
    {
        public string taskName;
        public GameObject taskInstructionsDialogPrefab;
        public GameObject taskCompleteMessageDialogPrefab;
        // TODO: Maybe add reference to the object for object search task
    }

    [System.Serializable]
    public class ObjectSearchTask : TrialTask
    {
        public GameObject objectToFind; // Reference to the object to be found
    }

    [System.Serializable]
    public class UXFBlock
    {
        [Header("Block Configuration")]
        [Tooltip("Unique name for UXF block")]
        public string blockName;
        
        public enum EnvironmentType { Curved, Angled, Open_Space, Maze }
        [Header("Environment Configuration")]
        [Tooltip("Type of environment for the block")]
        public EnvironmentType environment; // Dropdown in the Unity Editor
        [Tooltip("Reference to the environment spawn point")]
        public GameObject environmentSpawnPoint; // Reference to the environment spawn point
        [Tooltip("Reference to the environment finish point. Not applicable for tasks with multiple possible end points, like the object search task.")]
        public GameObject environmentFinishPoint; // Reference to the environment finish point
        
        
        [Tooltip("Dialog prefab to show at the start of block")]
        public GameObject startMessageDialogPrefab;
        [Tooltip("Dialog prefab to show at the end of block")]
        public GameObject endMessageDialogPrefab;
        
        // TODO: Add a way to specify the type of tasks and show task specific input parameters
        public enum TaskType { Generic, ObjectSearch }
        [Header("Trial Tasks Configuration")]
        public TaskType taskType = TaskType.Generic; // Dropdown in the Unity Editor
        // [Tooltip("List of trial tasks to include in the block. If no tasks are specified, the block will be treated as one trial.")]
        // public List<TrialTask> trialTasks; // List of trial tasks to include in the block
        [SerializeField]
        public List<TrialTask> trialTasks = new List<TrialTask>();
        [SerializeField]
        public List<ObjectSearchTask> objectSearchTasks = new List<ObjectSearchTask>();

        [Tooltip("Enable this to randomize the order of trial tasks within the block. Only applicable if multiple tasks are defined.")]
        public bool randomizeTrialTasksSequence = false;
    }
    
    [Header("Locomotion Method Configuration")]
    [Tooltip("Specify the dialog prefab with instructions for continuous locomotion method.")]
    public GameObject continuousLocomotionInstructionDialogPrefab;
    [Tooltip("Specify the dialog prefab with instructions for teleport locomotion method.")]
    public GameObject teleportLocomotionInstructionDialogPrefab;
    [Tooltip("Specify the dialog prefab with instructions for node-based locomotion method.")]
    public GameObject nodeLocomotionInstructionDialogPrefab;

    [Header("Session Settings")]
    [Tooltip("Enable this to run the experiment in non-VR mode. Useful for testing without VR headset.")]
    public bool nonVRMode = false;
    [Tooltip("Check to label this session as practice. Practice sessions may have different settings.")]
    [SerializeField] private bool isPracticeSession = false;
    
    [Tooltip("Dialog prefab to show while waiting for the session to start")]
    [SerializeField] private GameObject sessionWaitingDialogPrefab;
    [Tooltip("Dialog prefab to show when the session starts")]
    [SerializeField] private GameObject sessionStartDialogPrefab;
    [Tooltip("Dialog prefab to show when the session ends")]
    [SerializeField] private GameObject sessionEndDialogPrefab;
    [Tooltip("Specify block sequence and number of trials for each block")]
    [SerializeField] private UXFBlock[] blocks;
    
    // Events
    public static event Action<GameObject> OnPlayStart; // Pass the dialog prefab to show before the start of the session
    public static event Action<List<GameObject>> OnSessionGenerate; // Pass the list of dialog prefabs to show at the start of the session
    public static event Action<List<GameObject>> OnBlockEnd; // Pass the list of dialog prefabs to show at the end of each block
    
    void Start()
    {
        OnPlayStart?.Invoke(sessionWaitingDialogPrefab);
    }

    private void OnEnable()
    {
        FinishPointCheck.OnFinishPointReached += ShowNextInstructions;
    }
    
    private void OnDisable()
    {
        FinishPointCheck.OnFinishPointReached -= ShowNextInstructions;
    }
    
    // Session Start
    public void GenerateExperiment(Session session)
    {
        String locomotionMethodFromUI = session.participantDetails["locomotion_method"].ToString().ToLower();
        String preferredHandFromUI = session.participantDetails["preferred_hand"].ToString().ToLower();
        
        // Logging session details into session data
        session.settings.SetValue("is_practice", isPracticeSession);
        session.settings.SetValue("locomotion_method", locomotionMethodFromUI);
        session.settings.SetValue("preferred_hand", preferredHandFromUI);
        
        List<GameObject> sessionStartInstructionSequence = new List<GameObject> { sessionStartDialogPrefab };

        LocomotionMethod.UpdateFloors(locomotionMethodFromUI);

        if (!nonVRMode)
        {
            InputHandler.UpdateLocomotionControls(locomotionMethodFromUI);
        }
        else
        {
            Debug.Log("Dev mode is enabled. Skipping InputHandler.UpdateLocomotionControls()");
        }
        
        switch (locomotionMethodFromUI.ToLower())
        {
            case "continuous":
                sessionStartInstructionSequence.Add(continuousLocomotionInstructionDialogPrefab);
                break;
            case "teleport":
                sessionStartInstructionSequence.Add(teleportLocomotionInstructionDialogPrefab);
                break;
            case "node":
                sessionStartInstructionSequence.Add(nodeLocomotionInstructionDialogPrefab);
                break;
        }
        
        if (blocks.Length > 0)
        {
                if (blocks[0].startMessageDialogPrefab != null)
                {
                    sessionStartInstructionSequence.Add(blocks[0].startMessageDialogPrefab);
                }
        }
        
        // Invoke event to send the instruction sequence to VRDialogFlowManager
        OnSessionGenerate?.Invoke(sessionStartInstructionSequence);

        // if (locomotionMethodFromUI == "continuous")
        // {
        //     session.settings.SetValue("locomotion_method_instruction", "continuous_locomotion_instruction");
        // }
        // else
        // {
        //     session.settings.SetValue("locomotion_method_instruction", locomotionMethodFromUI == "teleport" ? "teleport_locomotion_instruction" : "node_locomotion_instruction");
        // }

        foreach (UXFBlock block in blocks)
        {
            // Create a block for each entry in the blocks array
            int trialCount = block.trialTasks.Count == 0 && block.objectSearchTasks.Count == 0 ? 1 : Math.Max(block.trialTasks.Count, block.objectSearchTasks.Count); // Specify at least one trial if no tasks are defined
            Block newBlock = session.CreateBlock(trialCount);
            newBlock.settings.SetValue("environment", block.environment.ToString().ToLower());
        }
    }

    public void EndExperiment()
    {
        // Wait for 5 seconds before ending the session
        StartCoroutine(EndSessionAfterDelay(5f));
    }

    private IEnumerator EndSessionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Session ended.");
        Session.instance.End();
    }

    private void ShowNextInstructions()
    {
        List<GameObject> instructionsSequence = new List<GameObject>();
        int currentBlockIndex = Session.instance.CurrentBlock.number;
        int previousBlockIndex = currentBlockIndex - 1;
        
        // Add the end message dialog of the last block if it exists and the start message dialog of the next block if it exists
        if (blocks.Length > 0)
        {
            UXFBlock lastBlock = blocks[previousBlockIndex];
            UXFBlock currentBlock = blocks[currentBlockIndex];
            if (lastBlock.endMessageDialogPrefab != null)
            {
                instructionsSequence.Add(lastBlock.endMessageDialogPrefab);
            }
            instructionsSequence.Add(currentBlock.startMessageDialogPrefab);
        }
        
        
        OnBlockEnd?.Invoke(instructionsSequence);
    }
    
    // Getters
    public List<GameObject> GetSpawnPointsSequence()
    {
        List<GameObject> spawnPointsSequence = new List<GameObject>();
        foreach (UXFBlock block in blocks)
        {
            spawnPointsSequence.Add(block.environmentSpawnPoint);
        }
        
        return spawnPointsSequence;
    }
    
    public List<GameObject> GetObjectSearchSequence()
    {
        List<GameObject> objectSearchSequence = new List<GameObject>();
        foreach (UXFBlock block in blocks)
        {
            if (block.taskType == UXFBlock.TaskType.ObjectSearch)
            {
                foreach (ObjectSearchTask task in block.objectSearchTasks)
                {
                    objectSearchSequence.Add(task.objectToFind);
                }
            }
        }

        return objectSearchSequence;
    }
}

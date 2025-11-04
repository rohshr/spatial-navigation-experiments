using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UXF;

namespace PointingTask
{
    [System.Serializable]
    public class ObjectPointingTask
    {
        public GameObject referenceObject;
        public GameObject targetObject;
        public Transform spawnLocation;
    }

    public class PointingEstimationSessionGenerator : MonoBehaviour
    {
        #region Inspector Fields

        [FormerlySerializedAs("XROrigin")] [Header("---- SESSION SETTINGS ----")] [Space(5)] [SerializeField]
        private GameObject xrOrigin;
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
        public static event Action<List<GameObject>> OnShowNextInstruction;
        public static event Action OnPointingEstimationSessionStart;
    
    
        // Private variables
        private PointingEstimationTask pointingTask;
    
        void Start()
        {
            pointingTask = GetComponent<PointingEstimationTask>();
            if (pointingTask == null)
            {
                pointingTask = gameObject.AddComponent<PointingEstimationTask>();
            }
            OnPlayStart?.Invoke(sessionWaitingDialogPrefab);
        }

        private void OnEnable()
        {
            VRDialogFlowManager.OnDialogFlowComplete += StartNextTrial;
            // Subscribe to pointing task completion
            PointingEstimationTask.OnAllTasksComplete += HandleAllTasksComplete;
            PointingEstimationTask.OnPointingComplete += ShowNextPointingInstruction;
            // InputHandler.SkipTrialEvent += ShowNextInstructions;
        }
    
        private void OnDisable()
        {
            VRDialogFlowManager.OnDialogFlowComplete -= StartNextTrial;
            PointingEstimationTask.OnAllTasksComplete -= HandleAllTasksComplete;
            PointingEstimationTask.OnPointingComplete -= ShowNextPointingInstruction;
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
            SetupPointingTasks();
        }
    
        // Getters

        private void SetupPointingTasks()
        {
            // Convert ObjectPointingTask list to PointingEstimationTask.PointingTaskData list
            List<PointingEstimationTask.PointingTaskData> tasks = new List<PointingEstimationTask.PointingTaskData>();

            foreach (var task in objectPointingTasks)
            {
                tasks.Add(new PointingEstimationTask.PointingTaskData
                {
                    referenceObject = task.referenceObject,
                    targetObject = task.targetObject,
                    spawnLocation = task.spawnLocation
                });
            }
            pointingTask.InitializeTasks(tasks);
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
            InputHandler.UpdateLocomotionControls("None");
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
        
        private void ShowNextPointingInstruction()
        {
            if (currentTaskIndex >= objectPointingTasks.Count)
            {
                Debug.Log("No more pointing tasks remaining.");
                OnSessionEnd?.Invoke(sessionEndDialogPrefab);
                return;
            }
            var instructionSequence = new List<GameObject> { GetCurrentTaskInstruction() };
            OnShowNextInstruction?.Invoke(instructionSequence);
        }
    
        private void HandleAllTasksComplete()
        {
            Debug.Log("All pointing estimation tasks completed");
            OnSessionEnd?.Invoke(sessionEndDialogPrefab);
        }
    
        public void OnPointingSubmitted()
        {
            pointingTask.SubmitPointing();
        }
    
        private void StartNextTrial()
        {
            // xrOrigin.transform.SetPositionAndRotation(
            //     spawnPoint.transform.position,
            //     spawnPoint.transform.rotation
            // );
            OnPointingEstimationSessionStart?.Invoke(); // To disable locomotion
            pointingTask.StartNextTask();
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
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UXF;

namespace PointingTask
{
    public class PointingEstimationTask : MonoBehaviour
    {
        [Header("XR Components")]
        [SerializeField] private GameObject xrOrigin;
        [SerializeField] private GameObject rightHandController;
        private XRRayInteractor rightHandControllerRayInteractor;

        private Queue<PointingTaskData> taskQueue = new Queue<PointingTaskData>();
        private PointingTaskData currentTask;
        private bool isTaskActive = false;

        // Events
        public static event Action<float, float> OnPointingComplete;
        public static event Action OnAllTasksComplete;

        // Task data structure
        [System.Serializable]
        public class PointingTaskData
        {
            public GameObject referenceObject;
            public GameObject targetObject;
            public Transform spawnLocation;
        }

        void Start()
        {
            rightHandControllerRayInteractor = rightHandController.GetComponent<XRRayInteractor>();
        }

        private void OnEnable()
        {
            PointingEstimationSessionGenerator.OnPointingEstimationSessionStart += ActivatePointer;
        }

        private void OnDisable()
        {
            PointingEstimationSessionGenerator.OnPointingEstimationSessionStart -= ActivatePointer;
        }

        private void ActivatePointer()
        {
            if (rightHandControllerRayInteractor != null)
            {
                rightHandControllerRayInteractor.enabled = true;
                Debug.Log("XR Ray Interactor activated");
            }
        }

        public void InitializeTasks(List<PointingTaskData> tasks)
        {
            taskQueue.Clear();
            foreach (var task in tasks)
            {
                taskQueue.Enqueue(task);
            }

            Debug.Log($"Initialized {tasks.Count} pointing tasks");
            StartNextTask();
        }

        public void StartNextTask()
        {
            if (taskQueue.Count == 0)
            {
                Debug.Log("All pointing tasks completed");
                OnAllTasksComplete?.Invoke();
                return;
            }

            currentTask = taskQueue.Dequeue();
            isTaskActive = true;

            // Position player at spawn location facing reference object
            PositionPlayer(currentTask.spawnLocation, currentTask.referenceObject);

            // Show only the reference object
            SetObjectVisibility(currentTask.referenceObject, true);
            SetObjectVisibility(currentTask.targetObject, false);
            
            Debug.Log($"Pointing task started. Reference: {currentTask.referenceObject.name}, Target: {currentTask.targetObject.name}. Tasks remaining: {taskQueue.Count}");
        }

        private void PositionPlayer(Transform spawnLocation, GameObject referenceObject)
        {
            if (xrOrigin == null || spawnLocation == null)
            {
                Debug.LogWarning("XR Origin or spawn location not set");
                return;
            }

            // Set position
            xrOrigin.transform.position = spawnLocation.position;

            // Calculate direction to reference object on XZ plane
            Vector3 spawnPos = new Vector3(spawnLocation.position.x, 0f, spawnLocation.position.z);
            Vector3 refPos = new Vector3(referenceObject.transform.position.x, 0f, referenceObject.transform.position.z);
            Vector3 directionToReference = (refPos - spawnPos).normalized;

            // Calculate rotation to face reference object
            float targetYaw = Mathf.Atan2(directionToReference.x, directionToReference.z) * Mathf.Rad2Deg;
            xrOrigin.transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);

            Debug.Log($"Player positioned at {spawnLocation.position}, facing {referenceObject.name} (rotation: {targetYaw}°)");
        }

        public void SubmitPointing()
        {
            if (!isTaskActive)
            {
                Debug.LogWarning("No active pointing task to submit");
                return;
            }

            // Get the pointing direction from the right hand controller
            Vector3 pointingDirection = rightHandControllerRayInteractor.transform.forward;

            // Calculate angles
            float participantAngle = CalculateAngleFromCenter(pointingDirection, currentTask.spawnLocation.position);
            float correctAngle = CalculateCorrectAngle();
            float angularError = Mathf.Abs(Mathf.DeltaAngle(participantAngle, correctAngle));

            // Log to UXF trial
            LogPointingData(participantAngle, correctAngle, angularError);

            // Invoke event
            OnPointingComplete?.Invoke(participantAngle, correctAngle);

            isTaskActive = false;
            Debug.Log($"Pointing submitted. Participant: {participantAngle:F2}°, Correct: {correctAngle:F2}°, Error: {angularError:F2}°");

            // Hide the reference object before moving to next task
            SetObjectVisibility(currentTask.referenceObject, false);
            
            Session.instance.BeginNextTrial();
            
            // Move to next task
            StartNextTask();
        }

        /// <summary>
        /// Calculate the angle on the XZ plane from the spawn position to the pointing direction.
        /// </summary>
        private float CalculateAngleFromCenter(Vector3 direction, Vector3 centerPosition)
        {
            // Project direction onto XZ plane
            Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;

            // Calculate angle from forward direction (0 degrees = positive Z axis)
            float angle = Mathf.Atan2(flatDirection.x, flatDirection.z) * Mathf.Rad2Deg;

            // Normalize to 0-360 range
            if (angle < 0) angle += 360f;

            return angle;
        }

        /// <summary>
        /// Calculate the correct angle from spawn location to target object on XZ plane.
        /// </summary>
        private float CalculateCorrectAngle()
        {
            // Get spawn position projected onto XZ plane
            Vector3 spawnPos = new Vector3(currentTask.spawnLocation.position.x, 0f, currentTask.spawnLocation.position.z);
        
            // Get target position projected onto XZ plane
            Vector3 targetPos = new Vector3(currentTask.targetObject.transform.position.x, 0f, currentTask.targetObject.transform.position.z);

            // Calculate direction from spawn to target
            Vector3 directionToTarget = (targetPos - spawnPos).normalized;

            // Calculate angle
            float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

            // Normalize to 0-360 range
            if (angle < 0) angle += 360f;

            return angle;
        }

        /// <summary>
        /// Log pointing data to UXF trial.
        /// </summary>
        private void LogPointingData(float participantAngle, float correctAngle, float angularError)
        {
            if (Session.instance == null || Session.instance.CurrentTrial == null)
            {
                Debug.LogWarning("UXF Session or Trial not available for logging");
                return;
            }

            Trial currentTrial = Session.instance.CurrentTrial;

            currentTrial.result["reference_object"] = currentTask.referenceObject.name;
            currentTrial.result["target_object"] = currentTask.targetObject.name;
            currentTrial.result["participant_angle"] = participantAngle;
            currentTrial.result["correct_angle"] = correctAngle;
            currentTrial.result["angular_error"] = angularError;
            currentTrial.result["spawn_position"] = currentTask.spawnLocation.position;
            currentTrial.result["reference_position"] = currentTask.referenceObject.transform.position;
            currentTrial.result["target_position"] = currentTask.targetObject.transform.position;

            Debug.Log($"Logged pointing data to UXF trial {currentTrial.number}");
        }

        private void SetObjectVisibility(GameObject obj, bool visible)
        {
            obj.SetActive(visible);
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = visible;
            }
        }

        public int GetRemainingTaskCount()
        {
            return taskQueue.Count + (isTaskActive ? 1 : 0);
        }

        public bool IsTaskActive()
        {
            return isTaskActive;
        }
    }
}
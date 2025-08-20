using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InstructionsController instructionsController;
    [SerializeField] private GameObject XROrigin;
    [SerializeField] private GameObject UIViewpoint;

    [Header("Settings")]
    [SerializeField] private List<GameObject> SpawnPointsSequence;
    [SerializeField] private List<GameObject> ObjectSearchSequence;

    private int currentSpawnPointIndex = 0;
    private int currentObjectSearchIndex = 0;
    private GameObject pendingSpawnPoint;
    private GameObject pendingObjectSearch;
    private bool objectSearchTrialsActive = false; // Flag to check if object search trials are active

    private void OnEnable()
    {
        InstructionsController.OnInstructionsCompleted += HandleInstructionsCompleted;
        VRDialogFlowManager.OnDialogFlowComplete += HandleInstructionsCompleted; // Subscribe to the event when dialog flow is completed
        VRDialogFlowManager.OnExperimentStart += HandleInstructionsCompleted; // Subscribe to the event when the experiment starts
        FinishPointCheck.OnFinishPointReached += MoveToUIViewpoint; // Subscribe to the event when the finish point is reached
        ExperimenterControlScript.OnTrialSkipped += MoveToUIViewpoint; // Subscribe to the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided += MoveToUIViewpoint; // Subscribe to the event when the object collision is detected
    }

    private void OnDisable()
    {
        InstructionsController.OnInstructionsCompleted -= HandleInstructionsCompleted;
        VRDialogFlowManager.OnDialogFlowComplete -= HandleInstructionsCompleted; // Unsubscribe from the event when dialog flow is completed
        VRDialogFlowManager.OnExperimentStart -= HandleInstructionsCompleted; // Unsubscribe from the event when the experiment starts
        FinishPointCheck.OnFinishPointReached -= MoveToUIViewpoint; // Unsubscribe from the event when the finish point is reached
        ExperimenterControlScript.OnTrialSkipped -= MoveToUIViewpoint; // Unsubscribe from the event when the session is ended
        ObjectCollisionDetection.OnObjectCollided -= MoveToUIViewpoint; // Unsubscribe from the event when the object collision is detected
    }

    public void SetNextSpawnPoint()
    {
        if (SpawnPointsSequence == null || SpawnPointsSequence.Count <= currentSpawnPointIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more spawn points available.");
            return;
        }

        if (!objectSearchTrialsActive)
        {
            pendingSpawnPoint = SpawnPointsSequence[currentSpawnPointIndex];
            instructionsController.SetEnvironmentInstruction(pendingSpawnPoint.name);
        }
        else if (ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to find.");
            objectSearchTrialsActive = false; // Reset the flag when no more object search trials are available
            currentSpawnPointIndex++;
            if (SpawnPointsSequence == null || SpawnPointsSequence.Count <= currentSpawnPointIndex)
            {
                Debug.LogWarning($"[{nameof(TrialManager)}] No more spawn points available.");
                return;
            }
            pendingSpawnPoint = SpawnPointsSequence[currentSpawnPointIndex];
            instructionsController.SetEnvironmentInstruction(pendingSpawnPoint.name);
            return;
        }

        if (pendingSpawnPoint.name == "OpenFloorSpawnPoint")
        {
            pendingObjectSearch = ObjectSearchSequence[currentObjectSearchIndex];
            instructionsController.SetObjectSearchInstruction(pendingObjectSearch.name);
            objectSearchTrialsActive = true; // Set the flag to true when object search trials are active
            return;
        }
    }

    public void SetNextObjectSearch()
    {
        if(ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to search.");
            return;
        }

        pendingObjectSearch = ObjectSearchSequence[currentObjectSearchIndex];
        instructionsController.SetObjectSearchInstruction(pendingObjectSearch.name);
    }

    private void HandleInstructionsCompleted()
    {
        if (pendingSpawnPoint == null) return;
        if (pendingSpawnPoint.name != "OpenFloorSpawnPoint")
        {
            MoveToSpawnPoint(pendingSpawnPoint);
            currentSpawnPointIndex++;
            pendingSpawnPoint = null;
        }
        else
        {
            if (ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex)
            {
                Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to search.");
                MoveToSpawnPoint(pendingSpawnPoint);
                currentSpawnPointIndex++;
                pendingSpawnPoint = null;
                objectSearchTrialsActive = false; // Reset the flag when no more object search trials are available
                return;
            }
            MoveToSpawnPoint(pendingSpawnPoint);
            currentObjectSearchIndex++;
            pendingObjectSearch = null;
        }
    }

    private void MoveToSpawnPoint(GameObject spawnPoint)
    {
        XROrigin.transform.SetPositionAndRotation(
            spawnPoint.transform.position,
            spawnPoint.transform.rotation
        );
        Debug.Log($"Moved to spawn point: {spawnPoint.name}");
    }

    public void MoveToUIViewpoint()
    {
        if (UIViewpoint != null)
        {
            XROrigin.transform.SetPositionAndRotation(
                UIViewpoint.transform.position,
                UIViewpoint.transform.rotation
            );
            Debug.Log($"Moved to UIViewpoint: {UIViewpoint.name}");
        }
        else
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] UIViewpoint is not assigned.");
        }
    }
}

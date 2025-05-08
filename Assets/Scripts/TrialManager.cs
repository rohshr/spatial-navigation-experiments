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

    private void OnEnable()
    {
        InstructionsController.OnInstructionsCompleted += HandleInstructionsCompleted;
        FinishPointCheck.OnFinishPointReached += MoveToUIViewpoint; // Subscribe to the event when the finish point is reached
        ObjectCollisionDetection.OnObjectCollided += MoveToUIViewpoint; // Subscribe to the event when the object collision is detected
    }

    private void OnDisable()
    {
        InstructionsController.OnInstructionsCompleted -= HandleInstructionsCompleted;
        FinishPointCheck.OnFinishPointReached -= MoveToUIViewpoint; // Unsubscribe from the event when the finish point is reached
        ObjectCollisionDetection.OnObjectCollided -= MoveToUIViewpoint; // Unsubscribe from the event when the object collision is detected
    }

    public void SetNextSpawnPoint()
    {
        if (SpawnPointsSequence == null || SpawnPointsSequence.Count <= currentSpawnPointIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more spawn points available.");
            return;
        }

        pendingSpawnPoint = SpawnPointsSequence[currentSpawnPointIndex];
        instructionsController.SetEnvironmentInstruction(pendingSpawnPoint.name);

        if (ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex)
        {
            Debug.LogWarning($"[{nameof(TrialManager)}] No more objects to find.");
            return;
        }

        if (pendingSpawnPoint.name == "OpenFloorSpawnPoint")
        {
            pendingObjectSearch = ObjectSearchSequence[currentObjectSearchIndex];
            instructionsController.SetObjectSearchInstruction(pendingObjectSearch.name);
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
        if (pendingSpawnPoint != null)
        {
            if (ObjectSearchSequence == null || ObjectSearchSequence.Count <= currentObjectSearchIndex || pendingSpawnPoint.name != "OpenFloorSpawnPoint")
            {
                MoveToSpawnPoint(pendingSpawnPoint);
                currentSpawnPointIndex++;
                pendingSpawnPoint = null;
            }
            else
            {
                SetNextObjectSearch();
                currentObjectSearchIndex++;
                pendingObjectSearch = null;
                MoveToSpawnPoint(pendingSpawnPoint);
            }
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

using System;
using UnityEngine;
using UXF;

public class FinishPointCheck : MonoBehaviour
{
    private SessionGenerator sessionGenerator;
    private PlayerPositionTracker playerPositionTracker;
    public static event Action OnFinishPointReached;
    public static event Action OnPlayerFinishedGuidedExploration;
    
    private void Awake()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
        playerPositionTracker = FindFirstObjectByType<PlayerPositionTracker>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Session.instance.InTrial) return;
        
        Debug.Log("Player reached finish point");
        if (sessionGenerator.GetCurrentBlockType() == "GuidedExploration")
        {
            Debug.Log("Disabling navigation guides and finish point.");
            OnPlayerFinishedGuidedExploration?.Invoke();
        }
        
        float distanceTravelled = playerPositionTracker.GetDistanceTravelled();
        int tileChanges  = playerPositionTracker.GetTileChanges();
        
        Session.instance.CurrentTrial.result["distance_travelled"] = distanceTravelled;
        Session.instance.CurrentTrial.result["tile_changes"] = tileChanges;
        
        Session.instance.CurrentTrial.End();
        OnFinishPointReached?.Invoke();
    }
}
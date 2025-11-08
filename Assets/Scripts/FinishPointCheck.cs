using System;
using UnityEngine;
using UXF;

public class FinishPointCheck : MonoBehaviour
{
    private SessionGenerator sessionGenerator;
    public static event Action OnFinishPointReached;
    public static event Action OnPlayerFinishedGuidedExploration;
    
    private void Awake()
    {
        sessionGenerator = FindFirstObjectByType<SessionGenerator>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Session.instance.InTrial) return;
        
        Debug.Log("Player reached finish point");
        TimeSpan finalTime = GameStopwatch.StopStopwatch();
        Debug.Log($"Total time: {finalTime.TotalSeconds} seconds");
        Session.instance.CurrentTrial.result["total_exploration_time"] = finalTime.TotalSeconds;
        
        if (sessionGenerator.GetCurrentBlockType() == "GuidedExploration")
        {
            Debug.Log("Disabling navigation guides and finish point.");
            OnPlayerFinishedGuidedExploration?.Invoke();
        }
        // Log the finish point visit
        if (FindFirstObjectByType<FloorTile>() != null)
        {
            FloorTile.tileVisitQueue.Enqueue(gameObject);
        }
        OnFinishPointReached?.Invoke();
        Session.instance.CurrentTrial.End();
    }
}
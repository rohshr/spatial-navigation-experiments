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
        if (sessionGenerator.GetCurrentBlockType() == "GuidedExploration")
        {
            Debug.Log("Disabling navigation guides and finish point.");
            OnPlayerFinishedGuidedExploration?.Invoke();
        }
        Session.instance.CurrentTrial.End();
        OnFinishPointReached?.Invoke();
    }
}
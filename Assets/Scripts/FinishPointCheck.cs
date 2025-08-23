using System;
using UnityEngine;
using UXF;

public class FinishPointCheck : MonoBehaviour
{
    public static event Action OnFinishPointReached;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Session.instance.InTrial) return;
        
        Debug.Log("Player reached finish point");
        Session.instance.CurrentTrial.End();
        OnFinishPointReached?.Invoke();
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UXF;

public class ExperimenterControlScript : MonoBehaviour
{
    // Script that allows the experimenter to manage and interrupt the experiment during sessions
    
    [SerializeField] private InputActionReference pauseTrial;
    [SerializeField] private InputActionReference forceEndCurrentTrial;
    [SerializeField] private InputActionReference forceEndSession; // Action to end the current trial

    private bool isPaused = false; // Flag to check if the trial is paused
    public static event Action OnTrialSkipped;
    public static event Action OnSessionEnded;

    private void Update()
    {
        if (forceEndCurrentTrial.action.triggered && Session.instance.InTrial)
        {
            Debug.Log("Trial skipped by experimenter at " + DateTime.Now);
            Session.instance.CurrentTrial.End();
            OnTrialSkipped?.Invoke(); // Trigger the event to notify that the trial was skipped
        }

        if (forceEndSession.action.triggered && Session.instance.hasInitialised)
        {
            Debug.Log("Session ended by experimenter at " + System.DateTime.Now);
            Session.instance.CurrentTrial.End();
            Session.instance.End();
        }

        if (pauseTrial.action.triggered && !isPaused)
        {
            Debug.Log("Trial paused by experimenter at " + System.DateTime.Now);            
            isPaused = true;
            Debug.Break();
        }
        else if (pauseTrial.action.triggered && isPaused)
        {
            Debug.Log("Trial resumed by experimenter at " + System.DateTime.Now);
            isPaused = false;
        }
    }

}

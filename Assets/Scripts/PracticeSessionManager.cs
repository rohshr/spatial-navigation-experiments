using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UXF;
using UXF.UI;

public class PracticeSessionManager : MonoBehaviour
{
    [SerializeField] private UIController uiController;
    [SerializeField] private GameObject practiceSessionsEndDialog;
    
    private int currentPracticeSession = 1;
    private Coroutine uiStartRoutine;
    private PopupController popupController;
    
    public static event Action<Session> OnPracticeSessionComplete; // Event to signal the completion of a practice session.
    public static event Action<GameObject> OnPracticeEnd; // Pass the dialog prefab to show at the end of the practice sessions.

    private static string[] GetLocomotionTechniqueSequence()
    {
        String[] locomotionTechniques = new String[3];
        locomotionTechniques[0] = Session.instance.participantDetails["practice_locomotion_1"].ToString().ToLower();
        locomotionTechniques[1] = Session.instance.participantDetails["practice_locomotion_2"].ToString().ToLower();
        locomotionTechniques[2] = Session.instance.participantDetails["practice_locomotion_3"].ToString().ToLower();
        
        Debug.Log("Practice locomotion technique sequence: " + String.Join(", ", locomotionTechniques));
        
        return locomotionTechniques;
    }
    
    public void TryStartNextPracticeSession()
    {
        if (uiStartRoutine != null)
        {
            StopCoroutine(uiStartRoutine);
        }
        uiStartRoutine = StartCoroutine(TryStartNextPracticeSessionSequence());
    }

    IEnumerator TryStartNextPracticeSessionSequence()
    {
        if (currentPracticeSession > 3) yield break;
    
        string[] locomotionTechniques = GetLocomotionTechniqueSequence();
        string currentTechnique = locomotionTechniques[currentPracticeSession - 1];
    
       
        bool error = false;
        
        // UXF settings
        string experimentName = uiController.settingsElement
            .GetContents()
            .ToString()
            .Replace(".json", "");
        
        // DATA PATH
        string localFilePath = "";
        if (uiController.RequiresFilePathElement)
        {
            if (!uiController.localFilePathElement.gameObject.activeSelf)
            {
                Utilities.UXFDebugLogError("Cannot start session - need Local Data Directory element, but it is not active.");
                yield break;
            }

            localFilePath = (string)uiController.localFilePathElement.GetContents();
            if (localFilePath.Trim() == string.Empty)
            {
                uiController.localFilePathElement.DisplayFault();
                Utilities.UXFDebugLogError("Local data directory is empty");
                error = true;
            }
            else if (!Directory.Exists(localFilePath))
            {
                uiController.localFilePathElement.DisplayFault();
                Utilities.UXFDebugLogErrorFormat("Cannot start session - local data directory {0} does not exist.", localFilePath);
                error = true;
            }

            foreach (var dh in uiController.ActiveLocalFileDataHandlers)
            {
                dh.StoragePath = localFilePath;
            }
        }
        
        string ppid = uiController.ppidElement
            .GetContents()
            .ToString()
            .Trim();
        
        // PARTICIPANT DETAILS
        Dictionary<string, object> newParticipantDetails;
        var validityList = uiController.SidebarStateIsValid(out newParticipantDetails);
        bool sidebarValid = true;
        foreach (var v in validityList)
        {
            sidebarValid = sidebarValid && v.valid;
            if (!v.valid && v.entry.element != null) v.entry.element.DisplayFault();
        }
        if (!sidebarValid) error = true;

        // TERMS AND CONDITIONS
        bool acceptedTsAndCs = (bool)uiController.tsAndCsToggle.GetContents();
        if (!acceptedTsAndCs)
        {
            uiController.tsAndCsToggle.DisplayFault();
            error = true;
        }
    
        Settings sessionSettings = new Settings();
        string settingsPath = Path.Combine(Application.streamingAssetsPath, uiController.settingsElement.GetContents().ToString());
        string settingsText = null;
        try
        {
            settingsText = File.ReadAllText(settingsPath);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogException(e);
            uiController.settingsElement.DisplayFault();
            error = true;
            yield break;
        }
        Dictionary<string, object> deserializedJson = (Dictionary<string, object>)MiniJSON.Json.Deserialize(settingsText);
        if (deserializedJson == null)
        {
            Utilities.UXFDebugLogErrorFormat("Cannot deserialize json file: {0}.", settingsPath);
            uiController.settingsElement.DisplayFault();
            error = true;
        }
        else
        {
            sessionSettings = new Settings(deserializedJson);
        }
        
        sessionSettings.SetValue("locomotion_method", currentTechnique);
        sessionSettings.SetValue("practice_session", currentPracticeSession);
        
        uiStartRoutine = null;
        if (error) yield break;
        
        bool exists = Session.instance.CheckSessionExists(
            localFilePath,
            experimentName,
            ppid,
            currentPracticeSession
        );

        if (exists)
        {
            Popup newPopup = new Popup()
            {
                message = string.Format(
                    "{0} - {1} - Session #{2} already exists! Press OK to start the session anyway, data may be overwritten.",
                    experimentName,
                    ppid,
                    currentPracticeSession
                ),
                messageType = MessageType.Warning,
                onOK = () => {
                    gameObject.SetActive(false);
                    // BEGIN!
                    Session.instance.Begin(
                        experimentName,
                        ppid,
                        currentPracticeSession,
                        newParticipantDetails,
                        sessionSettings
                    );
                }
            };
            popupController.DisplayPopup(newPopup);
        }
        else
        {
            uiController.gameObject.SetActive(false);

            // BEGIN!
            Session.instance.Begin(
                experimentName,
                ppid,
                sessionNumber: currentPracticeSession,
                participantDetails: newParticipantDetails,
                settings: sessionSettings
            );
        }
    }

    private void CheckPracticeSessionComplete()
    {
        if (currentPracticeSession >= 3)
        {
            Debug.Log("Practice sessions complete");
            StartCoroutine(EndPracticeAfterDelay(5f));
        }
        else
        {
            currentPracticeSession++;
            Debug.Log($"Proceeding to practice session {currentPracticeSession}");
            OnPracticeSessionComplete?.Invoke(Session.instance);
            TryStartNextPracticeSession();
        }
    }
    
    private IEnumerator EndPracticeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnPracticeEnd?.Invoke(practiceSessionsEndDialog);
    }
}

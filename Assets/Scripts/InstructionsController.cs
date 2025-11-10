using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UXF;
using System;

[System.Serializable]
public class Instruction
{
    public string scenario;
    public string description;
}

public class InstructionsController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public GameObject InstructionsCanvas;

    [Header("Settings")]
    [SerializeField] private EnvironmentInstructions environmentInstructions;
    [SerializeField] private ObjectSearchInstructions objectSearchInstructions;
    [SerializeField] private TextAsset instructionsJson;

    // Dictionary to hold the instructions loaded from the JSON file
    private Dictionary<string, string> instructionsDictionary;
    public static List<string> currentInstructions = new();

    // Event to notify when instructions are completed
    public static event Action OnInstructionsCompleted;

    // Helper class to parse the JSON array
    [System.Serializable]
    private class InstructionArray
    {
        public Instruction[] instructions;
    }

    void Start()
    {
        instructionsDictionary = LoadInstructions();
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (InstructionsCanvas == null)
            Debug.LogError($"[{nameof(InstructionsController)}] InstructionsCanvas is not assigned!");
        if (environmentInstructions == null)
            Debug.LogError($"[{nameof(InstructionsController)}] EnvironmentInstructions ScriptableObject is not assigned!");
    }

    public void SetInitialInstructions()
    {
        // initial intro text
        List<string> initialInstructionKeys = Session.instance.settings.GetStringList("initial_instructions_set");

        // adding the method instruction to the list
        initialInstructionKeys.Add(Session.instance.settings.GetString("locomotion_method_instruction"));

        foreach (string instructionKey in initialInstructionKeys)
        {
            if (instructionsDictionary != null && instructionsDictionary.ContainsKey(instructionKey))
            {
                currentInstructions.Add(instructionsDictionary[instructionKey]);
            }
            else
            {
                Debug.LogWarning($"Instruction '{instructionKey}' not found in the instruction dictionary.");
            }
        }

    }

    public void ShowEndMessage()
    {
        if (InstructionsCanvas == null) return;

        UpdateInstructionCanvasText(instructionsDictionary["practice_end_message"]);
        InstructionsCanvas.SetActive(true);
    }

    public void SetEnvironmentInstruction(string spawnPointId)
    {
        if (environmentInstructions != null)
        {
            var instruction = System.Array.Find(environmentInstructions.instructions, 
                x => x.spawnPointId == spawnPointId);

            if (instruction != null)
            {
                currentInstructions.Add(instruction.instructionText);
            }
            else
            {
                Debug.LogWarning($"No instructions found for spawn point: {spawnPointId}");
            }
        }
        else
        {
            Debug.LogError("SpawnPointInstructions ScriptableObject is not assigned!");
        }
    }

    public void SetObjectSearchInstruction(string objectID)
    {
        if (objectSearchInstructions != null)
        {
            var instruction = System.Array.Find(objectSearchInstructions.instructions, 
                x => x.objectId == objectID);

            if (instruction != null)
            {
                Debug.Log($"Instruction count from setobjectsearchinstructions: {currentInstructions.Count}");
                currentInstructions.Add(instruction.instructionText);
            }
            else
            {
                Debug.LogWarning($"No instructions found for spawn point: {objectID}");
            }
        }
        else
        {
            Debug.LogError("SpawnPointInstructions ScriptableObject is not assigned!");
        }
    }

    // Method to set the InstructionsCanvas active or inactive
    public void ShowInstructions()
    {
        if (!InstructionsCanvas || currentInstructions.Count == 0) return;

        UpdateInstructionCanvasText(currentInstructions[0]);
        Debug.Log($"Instruction Count: {currentInstructions.Count}");
        InstructionsCanvas.SetActive(true);
    }
    public void HideInstructions()
    {
        if (!InstructionsCanvas) return;

        currentInstructions.RemoveAt(0);
        UpdateInstructionCanvasText("");

        InstructionsCanvas.SetActive(false);
    }

    // Method to handle the proceed button press
    private void OnProceed()
    {
        if (!InstructionsCanvas || !InstructionsCanvas.activeSelf) return;

        if (currentInstructions.Count > 1)
        {
            // Show next instruction
            currentInstructions.RemoveAt(0);
            UpdateInstructionCanvasText(currentInstructions[0]);
        }
        else
        {
            // No more instructions, complete the sequence
            HideInstructions();
            OnInstructionsCompleted?.Invoke();
        }
    }

    public void EnableControls()
    {
        if (Session.instance.hasInitialised)
        {
            // InputHandler.ProceedEvent += OnProceed;
            // FinishPointCheck.OnFinishPointReached += ShowInstructions;
            // ExperimenterControlScript.OnTrialSkipped += ShowInstructions;
            // ObjectCollisionDetection.OnObjectFound += ShowInstructions;
        }
    }

    private void OnDisable()
    {
        // InputHandler.ProceedEvent -= OnProceed;
        // FinishPointCheck.OnFinishPointReached -= ShowInstructions;
        // ExperimenterControlScript.OnTrialSkipped -= ShowInstructions;
        // ObjectCollisionDetection.OnObjectFound -= ShowInstructions;
    }

    // Method to load instructions from the JSON file
    private Dictionary<string, string> LoadInstructions()
    {
        if (instructionsJson != null)
        {
            Debug.Log("Parsing JSON file...");
            InstructionArray instructionsArray = JsonUtility.FromJson<InstructionArray>("{\"instructions\":" + instructionsJson.text + "}");

            Dictionary<string, string> instructionsDict = new();
            foreach (Instruction instruction in instructionsArray.instructions)
            {
                if (!string.IsNullOrEmpty(instruction.scenario))
                {
                    if (!string.IsNullOrEmpty(instruction.description))
                    {
                        instructionsDict[instruction.scenario] = instruction.description;
                        Debug.Log($"Loaded scenario: {instruction.scenario}");
                    }
                    else
                    {
                        Debug.LogWarning($"Scenario '{instruction.scenario}' has an empty 'description' field and will be skipped.");
                    }
                }
                else
                {
                    Debug.LogWarning("An instruction entry has a null or empty 'scenario' field and will be skipped.");
                }
            }
            return instructionsDict;
        }
        else
        {
            Debug.LogError("Instructions JSON file is not assigned.");
            return new Dictionary<string, string>();
        }
    }

    // Method to set the text in the InstructionsCanvas based on a scenario
    public void UpdateInstructionCanvasText(string text)
    {
        TextMeshProUGUI canvasText = InstructionsCanvas.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (canvasText != null)
        {
            canvasText.text = text;
        }
        else
        {
            Debug.LogError($"[{nameof(InstructionsController)}] Text component not found in InstructionsCanvas");
        }
    }
}
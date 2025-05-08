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
    // public static List<string> objectInstructions = new() { "find_cube", "find_sphere", "find_statue", "find_star" };

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

    public void SetEnvironmentInstruction(string spawnPointId)
    {
        if (environmentInstructions != null)
        {
            var instruction = System.Array.Find(environmentInstructions.instructions, 
                x => x.spawnPointId == spawnPointId);

            if (instruction != null)
            {
                currentInstructions.Add(instruction.instructionText);
                
                // if (spawnPointId == "OpenFloorSpawnPoint")
                // {
                //     SetObjectSearchInstruction(instruction.instructionText);
                // }
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
        InstructionsCanvas.SetActive(true);
    }
    public void HideInstructions()
    {
        if (!InstructionsCanvas) return;

        currentInstructions = new();
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
            Debug.Log("Instructions completed, triggering OnInstructionsCompleted event");
            OnInstructionsCompleted?.Invoke();
        }
    }

    public void EnableControls()
    {
        if (Session.instance.hasInitialised)
        {
            InputHandler.ProceedEvent += OnProceed;
            FinishPointCheck.OnFinishPointReached += ShowInstructions;
            ObjectCollisionDetection.OnObjectCollided += ShowInstructions;
        }
    }

    private void OnDisable()
    {
        InputHandler.ProceedEvent -= OnProceed;
        FinishPointCheck.OnFinishPointReached -= ShowInstructions;
        ObjectCollisionDetection.OnObjectCollided -= ShowInstructions;
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

    // Method to get the array of scenarios to show in sequence
    // public void GetInstructionsArray(string textScenarios)
    // {
    //     scenarios.Clear(); // Clear the previous scenarios
    //     List<string> textScenariosList = new List<string>(textScenarios.Split(','));
    //     foreach (string scenario in textScenariosList)
    //     {
    //         if (instructionsDictionary.ContainsKey(scenario))
    //         {
    //             scenarios.Add(scenario);
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"Scenario '{scenario}' not found in the instructions dictionary.");
    //         }
    //     }
    //     UpdateInstruction(instructionsDictionary[scenarios[0]]); // Set the first scenario text
    //     scenarios.RemoveAt(0); // Remove the first scenario from the list
    // }

    // public void SetEnvironmentInstructions()
    // {
    //     if (Session.instance.CurrentBlock != null)
    //     {
    //         instructions = new List<string>(); // Clear the instructions list
    //         // string environment = Session.instance.CurrentBlock.settings.GetString("environment");
    //         instructions = Session.instance.CurrentBlock.settings.GetStringList("next_instruction_set");
    //         // Debug.Log($"Environment instructions set for '{environment}': " + string.Join(", ", instructions));
    //         if (instructions.Contains("open_space_briefing"))
    //         {
    //             instructions.Add(objectInstructions[0]);
    //             objectInstructions.RemoveAt(0); // Remove the first object instruction from the list
    //         }
    //         InstructionsCanvas.SetActive(true);
    //     }
    //     else
    //     {
    //         Debug.LogError("Current block is null. Cannot set environment instructions.");
    //     }
    // }

    // public static void UpdateInstructionSet(List<string> instructionSet)
    // {
    //     instructions = instructionSet;
    // }

    // // Method that starts each block
    // public void UpdateBlockInstruction()
    // {
    //     Session.instance.CurrentBlock.settings.GetString("environment");
    //     // UpdateInstructionSet("value from the block's settings")
    // }
}
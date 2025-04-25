using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UXF;

[System.Serializable]
public class Instruction
{
    public string scenario;
    public string description;
}

public class InstructionsController : MonoBehaviour
{
    public GameObject InstructionsCanvas; // Reference to the Instructions canvas

    // Path to the instructions.json file
    public TextAsset instructionsJson;

    // Dictionary to hold the instructions loaded from the JSON file
    private Dictionary<string, string> instructionsDictionary;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load instructions from the JSON file
        instructionsDictionary = LoadInstructions();

        Debug.Log("Instructions loaded: " + instructionsDictionary.Count + " entries found.");
        Debug.Log(instructionsDictionary);

        // Example: Set the initial text
        // Update the text in the canvas to a specific text in the JSON file
        updateInstruction("onboarding");

        // Subscribe to InputHandler events
        InputHandler.ProceedEvent += OnProceed;
        InputHandler.BackEvent += OnBack;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Session.instance.InTrial)
        {

        }
    }

    // Method to load instructions from the JSON file
    private Dictionary<string, string> LoadInstructions()
    {
        if (instructionsJson != null)
        {
            Debug.Log("Parsing JSON file...");
            InstructionArray instructionsArray = JsonUtility.FromJson<InstructionArray>("{\"instructions\":" + instructionsJson.text + "}");

            Dictionary<string, string> instructionsDict = new Dictionary<string, string>();
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

    // Method to set the InstructionsCanvas active or inactive
    public void SetInstructionsCanvasActive()
    {
        if (InstructionsCanvas != null)
        {
            InstructionsCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("InstructionsCanvas is not assigned.");
        }
    }
    public void SetInstructionsCanvasInactive()
    {
        if (InstructionsCanvas != null)
        {
            InstructionsCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("InstructionsCanvas is not assigned.");
        }
    }

    // Method to set the text in the InstructionsCanvas based on a scenario
    public void updateInstruction(string scenario)
    {
        if (instructionsDictionary != null && instructionsDictionary.ContainsKey(scenario))
        {
            string text = instructionsDictionary[scenario];
            Text canvasText = InstructionsCanvas.GetComponentInChildren<Text>();
            if (canvasText != null)
            {
                canvasText.text = text;
            }
            else
            {
                Debug.LogError("Text component not found in InstructionsCanvas.");
            }
        }
        else
        {
            Debug.LogError($"Scenario '{scenario}' not found in instructions.");
        }
    }
    // Method to handle the proceed button press
    private void OnProceed()
    {
        Debug.Log("Proceed button pressed.");
        // Hide the InstructionsCanvas and start the trial
        // if the current text is onboarding, display the practice briefing text
        if (InstructionsCanvas.activeSelf)
        {
            // if the instructions canvas text is the onboarding text, show the practice briefing text
            if (InstructionsCanvas.GetComponentInChildren<Text>().text == instructionsDictionary["onboarding"])
            {
                updateInstruction("practice_briefing");
            }
            else
            {
                // Hide the InstructionsCanvas and start the trial
                SetInstructionsCanvasInactive();
                Session.instance.BeginNextTrial();
            }
        }
        else
        {
            Debug.LogWarning("InstructionsCanvas is not active. Cannot proceed.");
        }
    }

    // Method to handle the back button press
    private void OnBack()
    {
        Debug.Log("Back button pressed.");
        // Show the previous instruction or perform another action
        // Example: updateInstruction("previous_scenario");
    }

    // Helper class to parse the JSON array
    [System.Serializable]
    private class InstructionArray
    {
        public Instruction[] instructions;
    }
}

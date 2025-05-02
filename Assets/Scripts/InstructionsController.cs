using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UXF;

[System.Serializable]
public class Instruction
{
    public string scenario;
    public string description;
}

public class InstructionsController : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject InstructionsCanvas; // Reference to the Instructions canvas

    // Path to the instructions.json file
    public TextAsset instructionsJson;

    // Dictionary to hold the instructions loaded from the JSON file
    private Dictionary<string, string> instructionsDictionary;

    public static List<string> instructions = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load instructions from the JSON file
        instructionsDictionary = LoadInstructions();


        // Example: Set the initial text
        // Update the text in the canvas to a specific text in the JSON file
        // updateInstruction("onboarding");

    }

    // Update is called once per frame
    void Update()
    {
        // if (Session.instance.hasInitialised)
        // {
            
        // }
        // else
        // {

        // }
    }

    public void InstantiateControls()
    {
        if (Session.instance.hasInitialised)
        {
            // Subscribe to InputHandler events
            InputHandler.ProceedEvent += OnProceed;
            InputHandler.BackEvent += OnBack;
        } 
        else
        {
            Debug.LogError("Session instance has not been initialized. Cannot subscribe to events.");
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
            if (instructions.Count == 0)
            {
                Debug.LogWarning("No instructions available to display.");
                return;
            }
            UpdateInstructionCanvasText(instructionsDictionary[instructions[0]]); // Set the first scenario text
            instructions.RemoveAt(0); // Remove the first scenario from the list
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
    public void UpdateInstructionCanvasText(string text)
    {
        TextMeshProUGUI canvasText = InstructionsCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (canvasText != null)
        {
            canvasText.text = text;
        }
        else
        {
            Debug.LogError("Text component not found in InstructionsCanvas.");
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

    //
    public static void UpdateInstructionSet(List<string> instructionSet)
    {
        // instructions.Clear(); // Clear the previous scenarios
        // foreach (string instruction in instructionSet)
        // {
        //     instructions.Add(instruction);
        // }
        instructions = instructionSet;
    }

    // Method that starts each block
    public void UpdateBlockInstruction()
    {
        Session.instance.CurrentBlock.settings.GetString("environment");
        // UpdateInstructionSet("value from the block's settings")
    }


    // Method to handle the proceed button press
    private void OnProceed()
    {
        // Check if the InstructionsCanvas is active before proceeding
        if (InstructionsCanvas.activeSelf)
        {
            Debug.Log("Proceed button pressed.");
            // // if the instructions canvas text is the onboarding text, show the practice briefing text
            // if (InstructionsCanvas.GetComponentInChildren<TextMeshProUGUI>().text == instructionsDictionary["onboarding"])
            // {
            //     updateInstruction("practice_briefing");
            // }
            if (instructions.Count > 0)
            {
                // Update the text in the canvas to the next scenario in the list
                UpdateInstructionCanvasText(instructionsDictionary[instructions[0]]);
                instructions.RemoveAt(0); // Remove the first scenario from the list
            }
            else
            {
                // Hide the InstructionsCanvas and start the trial
                SetInstructionsCanvasInactive();
                if (mainCamera != null)
                {
                    mainCamera.cullingMask = -1; // -1 sets the culling mask to everything
                }
                else
                {
                    Debug.LogWarning("Main Camera is not assigned.");
                }
                // Move to the starting spawnpoint
            }
        }
        // else
        // {
        //     Debug.LogWarning("InstructionsCanvas is not active. Cannot proceed.");
        // }
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

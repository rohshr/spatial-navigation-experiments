using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LandmarkPlacementTest
{
    public class LandmarkPlacement : MonoBehaviour
    {
        public static LandmarkPlacement Instance;
    
        [Header("Game Objects - Drag These")]
        public DraggableObject[] objects;
        public TargetArea[] targets;
    
        [Header("UI Names - Set These Instead of Dragging")]
        public string instructionTextName = "InstructionText";
        public string continueButtonName = "ContinueButton";
        public string instructionPanelName = "InstructionPanel";
    
        // These will be found automatically
        private GameObject instructionPanel;
        private TextMeshProUGUI instructionTextTMP;
        private Text instructionTextLegacy;
        private Button continueButton;
    
        private int objectsPlaced = 0;
        private List<string> placements = new List<string>();
    
        void Awake()
        {
            Instance = this;
        }
    
        void Start()
        {
            // Find UI elements by name instead of dragging them
            FindUIElements();
        
            SetInstructionText("Drag objects to their correct positions");
        
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
                continueButton.onClick.AddListener(NextTask);
            }
        }
    
        void FindUIElements()
        {
            // Find instruction panel
            if (!string.IsNullOrEmpty(instructionPanelName))
            {
                instructionPanel = GameObject.Find(instructionPanelName);
            }
        
            // Find instruction text (try TextMeshPro first, then legacy)
            if (!string.IsNullOrEmpty(instructionTextName))
            {
                GameObject textObj = GameObject.Find(instructionTextName);
                if (textObj != null)
                {
                    instructionTextTMP = textObj.GetComponent<TextMeshProUGUI>();
                    if (instructionTextTMP == null)
                    {
                        instructionTextLegacy = textObj.GetComponent<Text>();
                    }
                }
            }
        
            // Find continue button
            if (!string.IsNullOrEmpty(continueButtonName))
            {
                GameObject buttonObj = GameObject.Find(continueButtonName);
                if (buttonObj != null)
                {
                    continueButton = buttonObj.GetComponent<Button>();
                }
            }
        }
    
        void SetInstructionText(string text)
        {
            if (instructionTextTMP != null)
            {
                instructionTextTMP.text = text;
            }
            else if (instructionTextLegacy != null)
            {
                instructionTextLegacy.text = text;
            }
            else
            {
                Debug.LogWarning("No instruction text component found!");
            }
        }
    
        public void ObjectPlaced(string objectName, string targetName)
        {
            objectsPlaced++;
            placements.Add($"{objectName} placed on {targetName}");
        
            Debug.Log($"Placed {objectName} on {targetName}");
        
            // Check if all objects are placed
            if (objectsPlaced >= objects.Length)
            {
                TaskComplete();
            }
        }
    
        void TaskComplete()
        {
            SetInstructionText("Task Complete! Click Continue.");
        
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
            }
        
            // Save results
            foreach (string placement in placements)
            {
                Debug.Log(placement);
            }
        }
    
        void NextTask()
        {
            // Load next scene or reset current one
            UnityEngine.SceneManagement.SceneManager.LoadScene("NextTask");
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LandmarkPlacementTest
{
    public class LandmarkPlacement : MonoBehaviour
    {
        public static LandmarkPlacement Instance;
    
        [Header("Game Objects")]
        public DraggableObject[] objects;
        public TargetArea[] targets;
    
        [Header("UI References")]
        public TextMeshProUGUI instructionText;
        public Button submitButton;
    
        [Header("Task Settings")]
        [TextArea(2, 4)]
        public string initialInstructions = "Drag objects to their correct positions on the map";
        public int totalObjectsRequired = 12;
    
        // Tracking
        private Dictionary<TargetArea, DraggableObject> targetPlacements = new Dictionary<TargetArea, DraggableObject>();
        private bool hasSubmitted = false;
        private string originalInstructionText;
    
        void Awake()
        {
            Instance = this;
        }
    
        void Start()
        {
            InitializeTask();
            ValidateSetup();
        }
    
        void InitializeTask()
        {
            // Store original instruction text
            if (instructionText != null)
            {
                originalInstructionText = instructionText.text;
                // Set initial instructions (use inspector value or default)
                if (!string.IsNullOrEmpty(initialInstructions))
                {
                    instructionText.text = initialInstructions;
                }
            }
        
            // Hide submit button initially
            if (submitButton != null)
            {
                submitButton.gameObject.SetActive(false);
                submitButton.onClick.AddListener(OnSubmitClicked);
            }
        
            // Initialize tracking
            foreach (TargetArea target in targets)
            {
                if (target != null)
                {
                    targetPlacements[target] = null;
                }
            }
        
            hasSubmitted = false;
        
            Debug.Log($"LandmarkPlacement initialized - expecting {totalObjectsRequired} objects to be placed");
        }
    
        void ValidateSetup()
        {
            // Check UI references
            if (instructionText == null)
            {
                Debug.LogWarning("LandmarkPlacement: Instruction Text not assigned!");
            }
            if (submitButton == null)
            {
                Debug.LogWarning("LandmarkPlacement: Submit Button not assigned!");
            }
        
            // Check that all targets have correct objects assigned
            foreach (TargetArea target in targets)
            {
                if (target != null && target.correctObject == null)
                {
                    Debug.LogWarning($"Target '{target.name}' has no correct object assigned!");
                }
            }
        
            if (targets.Length != totalObjectsRequired)
            {
                Debug.LogWarning($"Expected {totalObjectsRequired} targets, but found {targets.Length}");
            }
        }
    
        // Called by TargetArea when an object is placed
        public void OnObjectPlaced(DraggableObject placedObject, TargetArea targetArea)
        {
            // Don't allow changes after submission
            if (hasSubmitted) return;
        
            // Handle object swapping/replacement
            HandleObjectPlacement(placedObject, targetArea);
        
            // Check if all slots are filled
            CheckAllSlotsFilled();
        
            Debug.Log($"Object placed: {placedObject.name} on {targetArea.name}. Total placed: {GetTotalPlacedObjects()}");
        }
    
        void HandleObjectPlacement(DraggableObject newObject, TargetArea newTarget)
        {
            // Find if the new object was previously in another target
            TargetArea previousTarget = null;
            foreach (var kvp in targetPlacements)
            {
                if (kvp.Value == newObject)
                {
                    previousTarget = kvp.Key;
                    break;
                }
            }
        
            // Get the object currently in the new target (if any)
            DraggableObject displacedObject = targetPlacements[newTarget];
        
            // Clear the previous target first (important for accurate counting)
            if (previousTarget != null)
            {
                targetPlacements[previousTarget] = null;
                previousTarget.currentObject = null;
                previousTarget.isOccupied = false;
            }
        
            // Handle displaced object
            if (displacedObject != null && displacedObject != newObject)
            {
                if (previousTarget != null)
                {
                    // SWAP: Move displaced object to the previous target
                    targetPlacements[previousTarget] = displacedObject;
                    displacedObject.transform.position = previousTarget.transform.position;
                    previousTarget.currentObject = displacedObject;
                    previousTarget.isOccupied = true;
                    displacedObject.SetPlacementState(previousTarget.targetName, true);
                
                    Debug.Log($"Swapping: {displacedObject.name} moved to {previousTarget.name}");
                }
                else
                {
                    // REPLACE: Move displaced object to original position
                    displacedObject.transform.position = displacedObject.originalPosition;
                    displacedObject.ResetPlacement();
                
                    Debug.Log($"Replacing: {displacedObject.name} moved back to original position");
                }
            }
        
            // Place the new object in the new target
            targetPlacements[newTarget] = newObject;
            newTarget.currentObject = newObject;
            newTarget.isOccupied = true;
            newObject.SetPlacementState(newTarget.targetName, true);
        
            // Position the object at the target center
            newObject.transform.position = newTarget.transform.position;
        
            Debug.Log($"Placement complete: {newObject.name} now in {newTarget.name}");
        }
    
        void CheckAllSlotsFilled()
        {
            int placedCount = GetTotalPlacedObjects();
        
            if (placedCount >= totalObjectsRequired && submitButton != null)
            {
                submitButton.gameObject.SetActive(true);
                Debug.Log($"All objects placed ({placedCount}/{totalObjectsRequired}) - Submit button shown");
            }
            else if (submitButton != null)
            {
                submitButton.gameObject.SetActive(false);
                if (placedCount < totalObjectsRequired)
                {
                    Debug.Log($"Submit button hidden - Only {placedCount}/{totalObjectsRequired} objects placed");
                }
            }
        }
    
        int GetTotalPlacedObjects()
        {
            return targetPlacements.Values.Count(obj => obj != null);
        }
    
        void OnSubmitClicked()
        {
            if (hasSubmitted) return;
        
            hasSubmitted = true;
        
            // Calculate results
            int correctPlacements = 0;
        
            foreach (var kvp in targetPlacements)
            {
                TargetArea target = kvp.Key;
                DraggableObject placedObject = kvp.Value;
            
                if (target != null && placedObject != null)
                {
                    bool isCorrect = (placedObject == target.correctObject);
                    target.isCorrectlyPlaced = isCorrect;
                
                    if (isCorrect)
                    {
                        correctPlacements++;
                    }
                
                    // Apply visual feedback
                    target.ProvideFeedback(isCorrect);
                }
            }
        
            // Update instruction text with results
            if (instructionText != null)
            {
                instructionText.text = $"Results: You placed {correctPlacements} out of {totalObjectsRequired} objects correctly.";
            }
        
            // Hide submit button
            if (submitButton != null)
            {
                submitButton.gameObject.SetActive(false);
            }
        
            // Save results
            SaveTaskResults(correctPlacements);
        
            Debug.Log($"Task submitted: {correctPlacements}/{totalObjectsRequired} correct");
        }
    
        void SaveTaskResults(int correctCount)
        {
            var results = new TaskResults
            {
                totalTargets = totalObjectsRequired,
                correctPlacements = correctCount,
                accuracy = (float)correctCount / totalObjectsRequired * 100f,
                completionTime = Time.time,
                placements = new Dictionary<string, string>()
            };
        
            // Record all placements
            foreach (var kvp in targetPlacements)
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    results.placements[kvp.Key.name] = kvp.Value.name;
                }
            }
        
            string json = JsonUtility.ToJson(results);
            Debug.Log("Task Results: " + json);
        
            // Here you could save to file, send to server, etc.
        }
    
        // Called when an object is removed from a target (for cleanup)
        public void OnObjectRemoved(TargetArea targetArea)
        {
            if (hasSubmitted) return;
        
            Debug.Log($"DEBUG: OnObjectRemoved called for {targetArea.name}");
        
            if (targetPlacements.ContainsKey(targetArea))
            {
                DraggableObject removedObject = targetPlacements[targetArea];
                targetPlacements[targetArea] = null;
            
                if (removedObject != null)
                {
                    removedObject.ResetPlacement();
                    Debug.Log($"DEBUG: Removed {removedObject.name} from {targetArea.name}");
                }
            }
        
            targetArea.currentObject = null;
            targetArea.isOccupied = false;
            targetArea.ResetAppearance();
        
            // Check if we still have all slots filled
            CheckAllSlotsFilled();
        }
    
        // Utility methods
        public bool HasSubmitted()
        {
            return hasSubmitted;
        }
    
        public void ResetTask()
        {
            // Reset submission state
            hasSubmitted = false;
        
            // Clear all placements
            foreach (var kvp in targetPlacements.ToList())
            {
                targetPlacements[kvp.Key] = null;
            }
        
            // Reset all targets
            foreach (TargetArea target in targets)
            {
                if (target != null)
                {
                    target.ResetAppearance();
                    target.OnObjectRemoved();
                }
            }
        
            // Reset all objects to original positions
            foreach (DraggableObject obj in objects)
            {
                if (obj != null)
                {
                    obj.transform.position = obj.originalPosition;
                    obj.ResetPlacement();
                }
            }
        
            // Reset UI
            if (instructionText != null)
            {
                instructionText.text = !string.IsNullOrEmpty(initialInstructions) ? initialInstructions : originalInstructionText;
            }
        
            if (submitButton != null)
            {
                submitButton.gameObject.SetActive(false);
            }
        
            Debug.Log("Task reset");
        }
    
        public TaskResults GetCurrentResults()
        {
            int correctCount = 0;
        
            if (hasSubmitted)
            {
                foreach (var kvp in targetPlacements)
                {
                    if (kvp.Key != null && kvp.Value != null && kvp.Key.isCorrectlyPlaced)
                    {
                        correctCount++;
                    }
                }
            }
        
            return new TaskResults
            {
                totalTargets = totalObjectsRequired,
                correctPlacements = correctCount,
                accuracy = totalObjectsRequired > 0 ? (float)correctCount / totalObjectsRequired * 100f : 0f,
                completionTime = Time.time,
                placements = targetPlacements.ToDictionary(
                    kvp => kvp.Key?.name ?? "null",
                    kvp => kvp.Value?.name ?? "null"
                )
            };
        }
    }

    [System.Serializable]
    public class TaskResults
    {
        public int totalTargets;
        public int correctPlacements;
        public float accuracy;
        public float completionTime;
        public Dictionary<string, string> placements;
    }
}
using UnityEngine;

namespace LandmarkPlacementTest
{
    public class TargetArea : MonoBehaviour
    {
        [Header("Target Identity")]
        public string targetName; // For identification
    
        [Header("Correct Object")]
        public DraggableObject correctObject; // Drag the correct object here in inspector
    
        [Header("Visual Feedback")]
        public Color normalColor = Color.white;
        public Color hoverColor = Color.yellow;
        public Color correctColor = Color.green;
        public Color incorrectColor = Color.red;
        [Range(0f, 1f)]
        public float feedbackAlpha = 0.6f;
    
        [Header("Feedback Options")]
        public bool showImmediateFeedback = true;
        public bool colorTarget = true;          // Color this target area
        public bool colorPlacedObject = true;    // Color the placed object
    
        [Header("State")]
        public bool isOccupied = false;
        public DraggableObject currentObject;
        public bool isCorrectlyPlaced = false;
    
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
    
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            gameObject.tag = "Target"; // Ensure it's tagged
        
            // Auto-set target name if not set
            if (string.IsNullOrEmpty(targetName))
            {
                targetName = gameObject.name;
            }
        
            // Store original color
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                normalColor = originalColor;
            }
        
            // Add collider if none exists
            if (GetComponent<Collider2D>() == null)
            {
                CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
            }
        
            // Validation
            if (correctObject == null)
            {
                Debug.LogWarning($"TargetArea '{name}': No correct object assigned! Please drag the correct DraggableObject to the 'Correct Object' field.");
            }
        }
    
        // Called when an object is placed on this target
        public void OnObjectPlaced(DraggableObject placedObject)
        {
            // Update state
            currentObject = placedObject;
            isOccupied = true;
        
            // Don't check correctness here anymore - LandmarkPlacement handles timing
            // Just provide basic hover feedback if enabled
            if (showImmediateFeedback && !LandmarkPlacement.Instance.HasSubmitted())
            {
                // Only show neutral/hover color before submission
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(normalColor, hoverColor, 0.3f);
                }
            }
        
            Debug.Log($"Target '{name}': {placedObject.name} placed");
        }
    
        // Called after submission to show final results
        public void ProvideFeedback(bool isCorrect)
        {
            Color feedbackColor = isCorrect ? correctColor : incorrectColor;
        
            // Color the target area
            if (colorTarget && spriteRenderer != null)
            {
                Color targetColor = feedbackColor;
                targetColor.a = feedbackAlpha;
                spriteRenderer.color = targetColor;
            }
        
            // Color the placed object
            if (colorPlacedObject && currentObject != null)
            {
                SpriteRenderer objectRenderer = currentObject.GetComponent<SpriteRenderer>();
                if (objectRenderer != null)
                {
                    Color objectColor = feedbackColor;
                    objectColor.a = feedbackAlpha * 0.5f; // More subtle for objects
                    objectRenderer.color = objectColor;
                }
            }
        
            isCorrectlyPlaced = isCorrect;
        }
    
        // Called when object is removed/dragged away
        public void OnObjectRemoved()
        {
            currentObject = null;
            isOccupied = false;
            isCorrectlyPlaced = false;
        
            // Reset visual feedback
            ResetAppearance();
        
            // Notify game manager of removal
            if (LandmarkPlacement.Instance != null)
            {
                LandmarkPlacement.Instance.OnObjectRemoved(this);
            }
        }
    
        // Visual feedback for hovering
        void OnTriggerEnter2D(Collider2D other)
        {
            DraggableObject draggable = other.GetComponent<DraggableObject>();
            if (draggable != null && !isOccupied && spriteRenderer != null)
            {
                // Show hover color, but hint at correctness
                Color hoverFeedback = hoverColor;
                if (draggable == correctObject)
                {
                    hoverFeedback = Color.Lerp(hoverColor, correctColor, 0.3f); // Slight green tint
                }
                spriteRenderer.color = hoverFeedback;
            }
        }
    
        void OnTriggerExit2D(Collider2D other)
        {
            DraggableObject draggable = other.GetComponent<DraggableObject>();
            if (draggable != null && !isOccupied && spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }
    
        // Public methods for external control
        public bool IsCorrectObject(DraggableObject obj)
        {
            return obj == correctObject;
        }
    
        public string GetCorrectObjectName()
        {
            return correctObject != null ? correctObject.name : "None";
        }
    
        public void ResetAppearance()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }
    
        public void SetFeedbackColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    
        // Method to force check current placement (useful for "Check All" functionality)
        public bool CheckCurrentPlacement()
        {
            if (currentObject != null)
            {
                bool wasCorrect = isCorrectlyPlaced;
                isCorrectlyPlaced = (currentObject == correctObject);
            
                // Update feedback if state changed
                if (wasCorrect != isCorrectlyPlaced)
                {
                    ProvideFeedback(isCorrectlyPlaced);
                }
            
                return isCorrectlyPlaced;
            }
            return false;
        }
    }
}
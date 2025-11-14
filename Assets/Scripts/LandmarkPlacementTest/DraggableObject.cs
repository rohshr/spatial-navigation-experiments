using UnityEngine;

namespace LandmarkPlacementTest
{
    public class DraggableObject : MonoBehaviour
    {
        private bool isDragging = false;
        private Vector3 dragOffset;
        private Camera cam;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
    
        [Header("Object Identity")]
        public string objectName; // For identification - MUST match PlacementChecker setup
    
        [Header("Sorting")]
        public int normalSortingOrder = 1;
        public int draggingSortingOrder = 10;
    
        [Header("Placement Settings")]
        public float snapDistance = 1f; // How close to snap to target
        public bool snapToCenter = true;
    
        [Header("Audio Feedback")]
        public bool enablePlacementSound = true;
        public AudioClip placementSound; // Click/snap sound when placed
        public AudioClip pickupSound; // Optional sound when picked up
        [Range(0f, 1f)]
        public float soundVolume = 0.5f;
    
        // Tracking
        private string currentTargetName = "";
        private bool isPlaced = false;
        public Vector3 originalPosition; // Store starting position for resets/swaps
    
        // Public properties for external access
        public string CurrentTargetName => currentTargetName;
        public bool IsPlaced => isPlaced;
    
        void Start()
        {
            cam = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();
        
            // Store original position
            originalPosition = transform.position;
        
            // Set up audio source
            SetupAudioSource();
        
            // Set initial sorting order
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = normalSortingOrder;
            }
        
            // Auto-set object name if not set
            if (string.IsNullOrEmpty(objectName))
            {
                objectName = gameObject.name;
            }
        }
    
        void SetupAudioSource()
        {
            // Get existing AudioSource or create one
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && enablePlacementSound)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.volume = soundVolume;
            }
        }
    
        void OnMouseDown()
        {
            // Don't allow dragging after submission
            if (LandmarkPlacement.Instance != null && LandmarkPlacement.Instance.HasSubmitted())
            {
                return;
            }
        
            isDragging = true;
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            dragOffset = transform.position - mousePos;
        
            // Bring to front when dragging
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = draggingSortingOrder;
            }
        
            // Play pickup sound
            PlayPickupSound();
        
            // If this object was in a target, notify that it's being removed
            if (isPlaced && LandmarkPlacement.Instance != null)
            {
                // Find which target this object was in and clear it
                foreach (TargetArea target in FindObjectsOfType<TargetArea>())
                {
                    if (target.currentObject == this)
                    {
                        LandmarkPlacement.Instance.OnObjectRemoved(target);
                        break;
                    }
                }
            }
        
            // Reset placement status when starting to drag
            SetPlacementState("", false);
        }
    
        void OnMouseDrag()
        {
            if (isDragging)
            {
                Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                transform.position = mousePos + dragOffset;
            }
        }
    
        void OnMouseUp()
        {
            isDragging = false;
        
            // Return to normal sorting order
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = normalSortingOrder;
            }
        
            // Check if we're over a target
            CheckForTarget();
        }
    
        void CheckForTarget()
        {
            // Find the closest target within snap distance
            TargetArea closestTarget = FindClosestTarget();
        
            if (closestTarget != null)
            {
                string targetName = closestTarget.targetName;
            
                // Don't snap here - let LandmarkPlacement handle positioning
                SetPlacementState(targetName, true);
            
                // Play placement sound
                PlayPlacementSound();
            
                // Notify LandmarkPlacement to handle the placement logic
                if (LandmarkPlacement.Instance != null)
                {
                    LandmarkPlacement.Instance.OnObjectPlaced(this, closestTarget);
                }
            
                Debug.Log($"{objectName} placed on {targetName}");
            }
            else
            {
                // Not close to any target
                SetPlacementState("", false);
                Debug.Log($"{objectName} not placed on any target");
            }
        }
    
        TargetArea FindClosestTarget()
        {
            TargetArea[] allTargets = FindObjectsOfType<TargetArea>();
            TargetArea closestTarget = null;
            float closestDistance = float.MaxValue;
        
            foreach (TargetArea target in allTargets)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
            
                if (distance < snapDistance && distance < closestDistance)
                {
                    closestTarget = target;
                    closestDistance = distance;
                }
            }
        
            return closestTarget;
        }
    
        // Alternative method using Physics2D (if you prefer collision-based detection)
        void CheckForTargetWithPhysics()
        {
            Collider2D hit = Physics2D.OverlapPoint(transform.position);
        
            if (hit != null && hit.CompareTag("Target"))
            {
                TargetArea targetArea = hit.GetComponent<TargetArea>();
                if (targetArea != null)
                {
                    string targetName = targetArea.targetName;
                
                    if (snapToCenter)
                    {
                        transform.position = hit.transform.position;
                    }
                
                    currentTargetName = targetName;
                    isPlaced = true;
                
                    // Play placement sound
                    PlayPlacementSound();
                
                    // Notify the target area
                    targetArea.OnObjectPlaced(this);
                
                    Debug.Log($"{objectName} placed on {targetName}");
                }
            }
        }
    
        // Audio Methods
        void PlayPlacementSound()
        {
            // Option 1: Use local sound
            if (enablePlacementSound && audioSource != null && placementSound != null)
            {
                audioSource.PlayOneShot(placementSound, soundVolume);
            }
            // Option 2: Use centralized sound manager
            else if (enablePlacementSound && PlacementAudioManager.Instance != null)
            {
                PlacementAudioManager.Instance.PlayPlacementSound();
            }
        }
    
        void PlayPickupSound()
        {
            // Option 1: Use local sound
            if (enablePlacementSound && audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound, soundVolume * 0.7f);
            }
            // Option 2: Use centralized sound manager
            else if (enablePlacementSound && PlacementAudioManager.Instance != null)
            {
                PlacementAudioManager.Instance.PlayPickupSound();
            }
        }
    
        // Utility methods
        public bool IsPlacedInTarget()
        {
            return isPlaced;
        }
    
        public string GetCurrentTarget()
        {
            return currentTargetName;
        }
    
        public void ResetPlacement()
        {
            isPlaced = false;
            currentTargetName = "";
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = normalSortingOrder;
            }
        }
    
        // Method to set placement state (used by LandmarkPlacement)
        public void SetPlacementState(string targetName, bool placed)
        {
            currentTargetName = targetName;
            isPlaced = placed;
        }
    }
}
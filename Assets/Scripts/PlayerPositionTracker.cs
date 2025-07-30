using UnityEngine;
using System.Collections.Generic;

public class PlayerPositionTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private string floorTileTag = "FloorTile"; // Tag for floor tiles
    [SerializeField] private LayerMask floorLayerMask = -1;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Current and previous tile tracking
    private GameObject currentTile;
    private GameObject previousTile;
    private GameObject beforePreviousTile;
    private Vector3 currentPosition;
    private Vector3 previousPosition;

    // Timing
    private float lastUpdateTime;
    
    // Events
    public System.Action<GameObject, GameObject, GameObject> OnTileChanged; // currentTile, previousTile
    
    // CharacterController reference
    private CharacterController characterController;
    private List<GameObject> overlappingTiles = new List<GameObject>();

    void Start()
    {
        // Find XR Origin if not assigned
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found! Please assign it in the inspector.");
            return;   
        }
        
        // Get or add CharacterController
        characterController = xrOrigin.GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = xrOrigin.AddComponent<CharacterController>();
            
            // Configure CharacterController for XR
            characterController.radius = 0.3f;
            characterController.height = 1.8f;
            characterController.center = new Vector3(0, 0.9f, 0);
            characterController.slopeLimit = 45f;
            characterController.stepOffset = 0.3f;
        }
        
        // Add trigger handler to the XR Origin
        FloorTriggerHandler triggerHandler = xrOrigin.GetComponent<FloorTriggerHandler>();
        if (triggerHandler == null)
        {
            triggerHandler = xrOrigin.AddComponent<FloorTriggerHandler>();
        }
        triggerHandler.Initialize(this, floorTileTag, floorLayerMask);
        
        // Initial position tracking
        UpdatePlayerPosition();
    }

    void Update()
    {
        // Update position tracking
        UpdatePlayerPosition();
        
        // Check for tile changes
        CheckForTileChange();
    }
    
    private void UpdatePlayerPosition()
    {
        if (xrOrigin == null) return;
        
        // Store previous position
        previousPosition = currentPosition;
        currentPosition = xrOrigin.transform.position;
    }
    
    private void CheckForTileChange()
    {
        GameObject newCurrentTile = GetClosestTile();

        if (newCurrentTile is not null && newCurrentTile != currentTile)
        {
            // Update tile tracking
            if (previousTile is not null)
            {
                beforePreviousTile = previousTile;
            }
            previousTile = currentTile;
            currentTile = newCurrentTile;

            // Trigger event
            OnTileChanged?.Invoke(currentTile, previousTile, beforePreviousTile);

            if (showDebugInfo)
            {
                Debug.Log($"Player moved to tile: {(currentTile?.name ?? "None")}" +
                         (previousTile != null ? $" (from: {previousTile.name})" : ""));
            }

            // Check for turns if needed
            if (beforePreviousTile != null && currentTile != null)
            {
                Vector3 beforePos = beforePreviousTile.transform.position;
                Vector3 currentPos = currentTile.transform.position;
                
                // Check if player changed direction (not moving in straight line)
                if (beforePos.x != currentPos.x && beforePos.z != currentPos.z)
                {
                    Debug.Log("Player made a turn.");
                }
            }
        }
    }

    private void CheckForTurns()
    {
        
    }
    
    private GameObject GetClosestTile()
    {
        if (overlappingTiles.Count == 0) return null;

        GameObject closestTile = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject tile in overlappingTiles)
        {
            if (tile == null) continue;

            float distance = Vector3.Distance(currentPosition, tile.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }
    
    // Called by FloorTriggerHandler
    public void OnTileEnter(GameObject tile)
    {
        if (!overlappingTiles.Contains(tile))
        {
            overlappingTiles.Add(tile);
        }
    }
    
    public void OnTileExit(GameObject tile)
    {
        overlappingTiles.Remove(tile);
    }
    
    // Public methods to get current state
    public GameObject GetCurrentTile() => currentTile;
    public GameObject GetPreviousTile() => previousTile;
    public Vector3 GetCurrentPosition() => currentPosition;
    public Vector3 GetPreviousPosition() => previousPosition;
    public string GetCurrentTileName() => currentTile != null ? currentTile.name : "None";
    public string GetPreviousTileName() => previousTile != null ? previousTile.name : "None";
    public bool IsOnTile(GameObject tile) => currentTile == tile;
    public bool WasOnTile(GameObject tile) => previousTile == tile;
    
    void OnDrawGizmos()
    {
        if (xrOrigin == null || characterController == null) return;
        
        // Draw CharacterController bounds
        Gizmos.color = Color.yellow;
        Vector3 center = xrOrigin.transform.position + characterController.center;
        float radius = characterController.radius;
        float height = characterController.height;
        
        // Draw capsule wireframe
        Gizmos.DrawWireCube(center, new Vector3(radius * 2, height, radius * 2));
        
        // Draw current position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(xrOrigin.transform.position, 0.2f);
        
        // Draw connection to current tile
        if (currentTile != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(xrOrigin.transform.position, currentTile.transform.position);
        }
    }
}

// Helper class to handle trigger events
public class FloorTriggerHandler : MonoBehaviour
{
    private PlayerPositionTracker tracker;
    private string floorTileTag;
    private LayerMask floorLayerMask;
    
    public void Initialize(PlayerPositionTracker positionTracker, string tileTag, LayerMask layerMask)
    {
        tracker = positionTracker;
        floorTileTag = tileTag;
        floorLayerMask = layerMask;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (IsFloorTile(other.gameObject))
        {
            tracker.OnTileEnter(other.gameObject);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (IsFloorTile(other.gameObject))
        {
            tracker.OnTileExit(other.gameObject);
        }
    }
    
    private bool IsFloorTile(GameObject obj)
    {
        // Check by tag
        if (!string.IsNullOrEmpty(floorTileTag) && !obj.CompareTag(floorTileTag))
            return false;
        
        // Check by layer
        if (floorLayerMask != -1 && (floorLayerMask & (1 << obj.layer)) == 0)
            return false;
        
        return true;
    }
}
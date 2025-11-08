using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UXF;

public class PlayerPositionTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private TeleportationProvider teleportationProvider;

    // [SerializeField] private GameObject locomotionGameObject;
    [SerializeField] private string floorTileTag = "FloorTile"; // Tag for floor tiles

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;


    private Vector3 currentPosition;
    private Vector3 previousPosition;
    
    // Distance and tile change tracking
    private float distanceTravelled = 0f;
    private int tileChanges = 0;

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
        
        // Physics Ovelap Sphere for teleportation detection
        
        
        // // Initial position tracking
        // UpdatePlayerPosition();
    }

    void OnEnable()
    {
        ObjectCollisionDetection.OnObjectCollided += ResetTracking;
        FinishPointCheck.OnFinishPointReached += ResetTracking;
        TrialManager.OnExplorationBlockCompleted += ResetTracking;
        InputHandler.ProceedTrialEvent += ResetTracking;
        teleportationProvider.locomotionEnded += TeleportationLog;
    }
    
    void OnDisable()
    {
        ObjectCollisionDetection.OnObjectCollided -= ResetTracking;
        FinishPointCheck.OnFinishPointReached -= ResetTracking;
        TrialManager.OnExplorationBlockCompleted -= ResetTracking;
        InputHandler.ProceedTrialEvent -= ResetTracking;
        teleportationProvider.locomotionEnded -= TeleportationLog;
    }

    void Update()
    {
        // Update position tracking
        if (Session.instance.InTrial)
        {
            UpdatePlayerPosition();
        }
    }
    
    private void UpdatePlayerPosition()
    {
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found! Please assign it in the inspector.");
            return;  
        }
        
        // Store previous position
        previousPosition = currentPosition;
        currentPosition = xrOrigin.transform.position;
            
        // Update distance travelled in meters
        if (currentPosition != previousPosition && previousPosition != Vector3.zero && currentPosition != Vector3.zero)
        {
            distanceTravelled += Vector3.Distance(previousPosition, currentPosition);
        }

    }

    private void TeleportationLog(LocomotionProvider provider)
    {
        Debug.Log("Player teleported.");
    }
    
    // void CheckTriggersAfterTeleport(GameObject player, float checkRadius)
    // {
    //     // Allocate a buffer for results (adjust size as needed)
    //     Collider[] hitColliders = new Collider[10];
    //     int numHits = Physics.OverlapSphereNonAlloc(player.transform.position, checkRadius, hitColliders);
    //
    //     for (int i = 0; i < numHits; i++)
    //     {
    //         var _collider = hitColliders[i];
    //         if (_collider != null && _collider.isTrigger)
    //         {
    //             var tile = _collider.GetComponent<FloorTile>();
    //             if (tile != null)
    //             {
    //                 // Call your own method to handle trigger logic
    //                 tile.HandlePlayerTileEnter(_collider);
    //             }
    //         }
    //     }
    // }

    private void SaveTrackingData()
    {
        // Log final results before resetting
        Session.instance.CurrentTrial.result["distance_travelled"] = distanceTravelled;
        tileChanges = FloorTile.GetTotalVisitsCount();
        Session.instance.CurrentTrial.result["tile_changes"] = tileChanges - 1; // -1 to ignore the first tile and get the number of changes instead of visits
        Session.instance.CurrentTrial.result["tile_travel_sequence"] = FloorTile.GetVisitHistoryString();
    }
    
    private void ResetTracking()
    {
        SaveTrackingData();
        FloorTile.ClearVisitHistory();
        
        currentPosition = Vector3.zero;
        previousPosition = Vector3.zero;
        distanceTravelled = 0f;
        tileChanges = 0;
    }

    // For debugging
    public void LogDistanceTravelled()
    {
        Session.instance.CurrentTrial.result["distance_travelled"] = distanceTravelled;
        Session.instance.CurrentTrial.result["tile_changes"] = tileChanges;
    }

    private void CheckForTurns()
    {
        
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
    public Vector3 GetCurrentPosition() => currentPosition;
    public Vector3 GetPreviousPosition() => previousPosition;
    public float GetDistanceTravelled() => distanceTravelled;
    public int GetTileChanges() => tileChanges;
    
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
    }
}
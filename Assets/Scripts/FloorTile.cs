using System.Collections.Generic;
using UnityEngine;
using UXF;

public class FloorTile : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    public static GameObject StartPosition;
    public static GameObject FinishPosition;
    
    // Static queue shared by all tiles - tracks the order of tiles visited
    public static Queue<GameObject> tileVisitQueue = new Queue<GameObject>();
    
    // Static set to track unique tiles visited (prevents duplicates)
    public static HashSet<GameObject> visitedTilesHash = new HashSet<GameObject>();
    
    // Events for when any tile is visited
    public static event System.Action<GameObject, int> OnTileVisited; // (tile, totalUniqueCount)
    public static event System.Action<GameObject> OnNewTileDiscovered; // Only fires for first-time visits
    
    private bool hasBeenVisited = false;
    private static int lastVisitFrame = -1;
    private static GameObject lastVisitedTile = null;
    
    void OnTriggerEnter(Collider other)
    {
        HandlePlayerTileEnter(other);
    }

    // Utility methods you can call from anywhere

    public void HandlePlayerTileEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && Session.instance.InTrial)
        {
            // Only process one tile per frame
            if (Time.frameCount == lastVisitFrame && lastVisitedTile != null && lastVisitedTile != gameObject)
            {
                Debug.LogWarning($"Ignoring overlapping tile {gameObject.name} - already processed {lastVisitedTile.name} this frame");
                return;
            }

            lastVisitFrame = Time.frameCount;
            lastVisitedTile = gameObject;
            // Add to visit queue (tracks order and allows duplicates)
            tileVisitQueue.Enqueue(gameObject);
            
            // Check if this is a new tile
            bool isNewTile = visitedTilesHash.Add(gameObject);
            
            if (isNewTile)
            {
                hasBeenVisited = true;
                Debug.Log($"New tile discovered: {gameObject.name}");
                OnNewTileDiscovered?.Invoke(gameObject);
            }
            
            Debug.Log($"Visited tile: {gameObject.name} (Total unique tiles: {visitedTilesHash.Count})");
            OnTileVisited?.Invoke(gameObject, visitedTilesHash.Count);
        }
    }
    
    public static int GetTotalVisitsCount()
    {
        return tileVisitQueue.Count;
    }
    
    public static int GetUniqueVisitedCount()
    {
        return visitedTilesHash.Count;
    }
    
    public static void ClearVisitHistory()
    {
        tileVisitQueue.Clear();
        visitedTilesHash.Clear();
    }
    
    public static List<GameObject> GetVisitHistory()
    {
        return new List<GameObject>(tileVisitQueue);
    }
    
    // function to get a name sequence of the visit history
    public static string GetVisitHistoryString()
    {
        List<string> names = new List<string>();
        foreach (var tile in tileVisitQueue)
        {
            names.Add(tile.name);
        }
        return string.Join(" -> ", names);
    }
    
    public static List<GameObject> GetUniqueVisitedTiles()
    {
        return new List<GameObject>(visitedTilesHash);
    }
    
    public bool HasBeenVisited()
    {
        return hasBeenVisited;
    }
}
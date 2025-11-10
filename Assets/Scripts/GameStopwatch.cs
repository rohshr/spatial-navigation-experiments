using System;
using System.Diagnostics;
using UnityEngine;

public class GameStopwatch : MonoBehaviour
{
    private static GameStopwatch _instance;
    private Stopwatch stopwatch = new Stopwatch();

    // Events
    public static event Action<TimeSpan> OnStopwatchStopped;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start the stopwatch
    public static void StartStopwatch()
    {
        if (_instance == null)
        {
            UnityEngine.Debug.LogError("GameStopwatch instance not found!");
            return;
        }
        
        _instance.stopwatch.Restart();
        UnityEngine.Debug.Log("Stopwatch started");
    }

    // Stop the stopwatch and return elapsed time
    public static TimeSpan StopStopwatch()
    {
        if (_instance == null)
        {
            UnityEngine.Debug.LogError("GameStopwatch instance not found!");
            return TimeSpan.Zero;
        }

        _instance.stopwatch.Stop();
        TimeSpan elapsed = _instance.stopwatch.Elapsed;
        
        UnityEngine.Debug.Log($"Stopwatch stopped. Elapsed: {elapsed.TotalSeconds:F2} seconds");
        OnStopwatchStopped?.Invoke(elapsed);
        
        return elapsed;
    }
    
    // Pause the stopwatch and log elapsed time
    public static TimeSpan PauseStopwatch()
    {
        if (_instance == null)
        {
            UnityEngine.Debug.LogError("GameStopwatch instance not found!");
            return TimeSpan.Zero;
        }
        _instance.stopwatch.Stop();
        TimeSpan elapsed = _instance.stopwatch.Elapsed;
        UnityEngine.Debug.Log($"Stopwatch paused. Elapsed: {elapsed.TotalSeconds:F2} seconds");
        return elapsed;
    }
    
    // Resume the stopwatch from paused state
    public static void ResumeStopwatch()
    {
        if (_instance == null)
        {
            UnityEngine.Debug.LogError("GameStopwatch instance not found!");
            return;
        }
        _instance.stopwatch.Start();
        UnityEngine.Debug.Log("Stopwatch resumed");
    }

    // Get elapsed time without stopping
    public static TimeSpan GetElapsedTime()
    {
        return _instance != null ? _instance.stopwatch.Elapsed : TimeSpan.Zero;
    }

    // Get elapsed time in seconds
    public static float GetElapsedSeconds()
    {
        return _instance != null ? (float)_instance.stopwatch.Elapsed.TotalSeconds : 0f;
    }

    // Check if stopwatch is running
    public static bool IsRunning()
    {
        return _instance != null && _instance.stopwatch.IsRunning;
    }

    // Reset stopwatch
    public static void ResetStopwatch()
    {
        if (_instance != null)
        {
            _instance.stopwatch.Reset();
        }
    }
}

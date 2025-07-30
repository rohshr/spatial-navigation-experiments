using System;
using System.Collections;
using UnityEngine;
using UXF;

public class TimeControl : MonoBehaviour
{

    public void FreeExplorationBeginCountdown()
    {
        StartCoroutine(Countdown());
        Debug.Log("TS Start time:" + DateTime.Now);
    }
    public void FreeExplorationStopCountdown()
    {
        Debug.Log("End time:" + DateTime.Now);
        // StopCoroutine(Countdown());
        StopAllCoroutines();
    }

    IEnumerator Countdown()
    {
        // float currentTime = 0f;
        // float startingTime = 10f;

        // while (currentTime <= startingTime)
        // {
        //     currentTime += 1 * Time.deltaTime;
        //     Debug.Log(currentTime);
        //     yield return null;
        // }

        float explorationTime = Session.instance.settings.GetFloat("exploration_time");
        yield return new WaitForSeconds(explorationTime);

        // log all the landmarks found and the order in which they were found

        Session.instance.EndCurrentTrial();
    }
    
    void OnTriggerExit(Collider other) {
        // check if the tag applied to the game object is "SpawnPoint" or "FinishPoint"
        if (this.CompareTag("SpawnPoint") && other.CompareTag("Player"))
        {
            Debug.Log("Player has exited the spawn point.");
            // Call the method to start the countdown
            FreeExplorationBeginCountdown();
        } 
    }

    void OnTriggerEnter(Collider other)
    {
        if (this.CompareTag("FinishPoint") && other.CompareTag("Player"))
        {
            Debug.Log("Player has reached the finish point.");
            // Call the method to stop the countdown
            FreeExplorationStopCountdown();
        }
    }
}

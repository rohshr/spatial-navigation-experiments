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

        float freeExplorationTime = Session.instance.settings.GetFloat("free_exploration_time");
        yield return new WaitForSeconds(freeExplorationTime);

        // log all the landmarks found and the order in which they were found

        Session.instance.EndCurrentTrial();
    }
}

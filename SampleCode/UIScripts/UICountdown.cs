using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICountdown : MonoBehaviour
{

    public float targetTime = 10.0f;


    private void Update()
    {

        targetTime -= Time.deltaTime;
        GameObject.Find("Countdown").GetComponent<Text>().text = targetTime.ToString("N0");

        if (targetTime <= 0.0f)
        {
            TimerEnded();
        }

    }

    void TimerEnded()
    {
        //do your stuff here.
    }

}
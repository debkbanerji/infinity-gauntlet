using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RiggedHandScript : MonoBehaviour {

    Animator riggedHandAnimator;
    int updateCount;
    private string currString;
    private bool requestRunning = false;

    // Use this for initialization
    void Start () {
        //_ShowAndroidToastMessage("Hello from hand script");
        riggedHandAnimator = GetComponent<Animator>();
        updateCount = 0;
        currString = "Placed Hand";
        riggedHandAnimator.SetInteger("Target Finger", 0);
    }

    // Update is called once per frame
    void Update () {
        if (!requestRunning)
        {
            StartCoroutine(responseCoroutine());
        }
        updateCount = (updateCount + 1) % 60;
        if (updateCount == 1)
        {
            _ShowAndroidToastMessage(currString);
        }
    }

    IEnumerator responseCoroutine()
    {
        requestRunning = true;
        UnityWebRequest www = UnityWebRequest.Get("https://infinity-gauntlet.herokuapp.com/curr-finger");
        //WWW w = new WWW("www.google.com");
        //yield return new WaitUntil(() => w.bytesDownloaded > 0);
        //currString = w.ToString().Substring(0, 20);

        //currString = "yielded";

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            currString = www.error;
        }

        else
        {
            try
            {
                currString = www.downloadHandler.text;
                string currFinger = www.downloadHandler.text; ;
                // currString = "Do Transition";
                // currString = riggedHandAnimator.GetAnimatorTransitionInfo(0).ToString();
                int targetFinger = 0;
                if (currFinger == "index")
                {
                    targetFinger = 1;
                } else if (currFinger == "middle")
                {
                    targetFinger = 2;
                } else if (currFinger == "ring")
                {
                    targetFinger = 3;
                } else if (currFinger == "pinky")
                {
                    targetFinger = 4;
                } else
                {
                    targetFinger = 0;
                }
                riggedHandAnimator.SetInteger("Target Finger", targetFinger);

            }
            catch (Exception e)
            {
                currString = e.ToString();
            }
        }

        requestRunning = false;
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                toastObject.Call("show");
            }));
        }
    }
}

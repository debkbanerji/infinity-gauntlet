using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RiggedHandScript : MonoBehaviour {

    Animator riggedHandAnimator;
    int updateCount;
    private string currString;

    // Use this for initialization
    void Start () {
        //_ShowAndroidToastMessage("Hello from hand script");
        riggedHandAnimator = GetComponent<Animator>();
        updateCount = 0;
        currString = "Init";
        StartCoroutine(responseCoroutine());
        riggedHandAnimator.SetInteger("Target Finger", 0);
    }

    // Update is called once per frame
    void Update () {
        updateCount = (updateCount + 1) % 250;
        if (updateCount == 249)
        {
            riggedHandAnimator.SetInteger("Target Finger", 1);
            _ShowAndroidToastMessage(currString);
        }
    }

    IEnumerator responseCoroutine()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://www.debkbanerji.com");
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
                // currString = www.downloadHandler.text;
                currString = "Do Transition";
                // riggedHandAnimator.SetInteger("Target Finger", 1);
                currString = riggedHandAnimator.GetAnimatorTransitionInfo(0).ToString();
            }
            catch (Exception e)
            {
                currString = e.ToString();
            }
        }
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

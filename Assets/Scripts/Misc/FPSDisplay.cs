using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FPSDisplay : MonoBehaviour
{
    public Text fpsText;
    private Queue<float> fpsList;

    void Start()
    {
        fpsList = new Queue<float>();
        StartCoroutine(DrawFPS());
    }
        
    IEnumerator DrawFPS()
    {
        while (true)
        {
            float fps = 1.0f / Time.unscaledDeltaTime;
            fpsList.Enqueue(fps);
            if (fpsList.Count > 20)
                fpsList.Dequeue();

            float averageFps = 0.0f;
            foreach (float f in fpsList)
                averageFps += f;
            averageFps /= fpsList.Count;

            string text = string.Format(averageFps.ToString("0.") + " fps");
            fpsText.text = "FPS: " + text;
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
}
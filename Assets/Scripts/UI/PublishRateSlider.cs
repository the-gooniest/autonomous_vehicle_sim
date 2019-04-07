using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublishRateSlider : MonoBehaviour {

    public Slider slider;
    public Text text;

    void Awake () {
        slider.minValue = 1.0f;
        slider.maxValue = 60.0f;
        slider.onValueChanged.AddListener(SetPublishRate);
        int rate = PlayerPrefs.GetInt("SimulationPublishRate", 2);
        slider.value = rate;
        text.text = "Publish Rate: " + rate.ToString() + "Hz";
    }

    private void SetPublishRate(float param)
    {
        int publishRate = Mathf.RoundToInt(param);
        PlayerPrefs.SetInt("SimulationPublishRate", publishRate);
        text.text = "Publish Rate: " + publishRate.ToString() + "Hz";
        RosSim.Singleton.PublishFrequency = publishRate;
    }
}

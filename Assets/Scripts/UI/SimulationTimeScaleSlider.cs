using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationTimeScaleSlider : MonoBehaviour {

    public Slider slider;
    public Text text;

	void Start () {
        slider.minValue = 0.0f;
        slider.maxValue = 1.5f;
        slider.onValueChanged.AddListener(SetSimulationTimeScale);
	}

    private void SetSimulationTimeScale(float param)
    {
        float timeScale = Mathf.Round(param * 10.0f) * 0.1f;
        Time.timeScale = timeScale;
        text.text = "Simulation Time Scale: " + timeScale.ToString();
    }

}

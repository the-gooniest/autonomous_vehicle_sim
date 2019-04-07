using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

public class DriveModeOptions : MonoBehaviour {

    public Dropdown pilotModeDropDown;
    public Button changeLaneButton;
    private CarRemoteControl _remoteControl;
    public Text speedText;
    public Slider speedSlider;

	void Awake()
    {
        pilotModeDropDown.onValueChanged.AddListener(PilotModeChangedHandler);
        speedSlider.onValueChanged.AddListener(ChangeMaxSpeedHandler);
        changeLaneButton.onClick.AddListener(ChangeLaneHandler);
        _remoteControl = FindObjectOfType<CarRemoteControl>();
	}

    void Start()
    {
        int pilotModeOption = PlayerPrefs.GetInt("PilotModeOption", 0);
        pilotModeDropDown.value = pilotModeOption;
        int maxSpeed = PlayerPrefs.GetInt("MaxSpeed", 25);
        speedSlider.value = maxSpeed;
    }
	
    void PilotModeChangedHandler(int option)
    {
        PlayerPrefs.SetInt("PilotModeOption", option);
        string pilotModeString = pilotModeDropDown.options[option].text;
        if (pilotModeString.Equals("Manual Control"))
        {
            _remoteControl.PilotMode = CarRemoteControl.CarPilotMode.Manual;
        }
        else if (pilotModeString.Equals("Sim Steering and Throttle"))
        {
            _remoteControl.PilotMode = CarRemoteControl.CarPilotMode.Simulated;
        }
        else if (pilotModeString.Equals("Sim Steering with ROS Throttle"))
        {
            _remoteControl.PilotMode = CarRemoteControl.CarPilotMode.SimSteeringRosThrottle;
        }
        else if (pilotModeString.Equals("ROS Steering with Sim Throttle"))
        {
            _remoteControl.PilotMode = CarRemoteControl.CarPilotMode.SimThrottleRosSteering;
        }
        else if (pilotModeString.Equals("ROS Steering and Throttle"))
        {
            _remoteControl.PilotMode = CarRemoteControl.CarPilotMode.ROS;
        }
        else
            Debug.LogError("Invalid Drive option string: " + pilotModeString);  
    }

    void ChangeMaxSpeedHandler(float sliderValue)
    {
        int speed = Mathf.RoundToInt(sliderValue);
        PlayerPrefs.SetInt("MaxSpeed", speed);
        speedText.text = "Max Vehicle Speed: " + speed;
        _remoteControl.SetMaxSpeed(speed);
    }

    void ChangeLaneHandler()
    {
        _remoteControl.ChangeLane();
    }
}

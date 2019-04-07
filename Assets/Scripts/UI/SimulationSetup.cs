using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimulationSetup : MonoBehaviour {

    /// <summary>
    /// Private inspector variables
    /// </summary>
    [SerializeField]
    private InputField _ipAddressField = null, _portField = null;
    [SerializeField]
    private Button _chooseSimulationButton = null;
    [SerializeField]
    private GameObject _connectionPanel = null;
    [SerializeField]
    private GameObject _chooseSimulationScrollView = null;
    [SerializeField]
    private GameObject _sceneButtonPrefab = null, _sceneButtonsPanel = null;
    public GameObject SceneButtonPrefab { get { return _sceneButtonPrefab; }}
    public GameObject SceneButtonsPanel { get { return _sceneButtonsPanel; }}

    /// <summary>
    /// The singleton of this class
    /// </summary>
    private static SimulationSetup _singleton;
    public static SimulationSetup Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = FindObjectOfType<SimulationSetup>();
            return _singleton;
        }
    }

    /// <summary>
    /// PlayerPref Keys
    /// </summary>
    public static readonly string RosBridgeIpAddressKey = "RosBridgeIpAddress";
    public static readonly string RosBridgePortKey = "RosBridgePort";

    /// <summary>
    /// Constants
    /// </summary>
    public static readonly string SceneName = "SimulationSetup";
    public static readonly string DefaultIpAddress = "127.0.0.1";
    public static readonly int DefaultPort = 9090;

	void Start()
	{
        if (_ipAddressField == null)
            Debug.LogError("IpAddress InputField is null", this);
        if (_portField == null)
            Debug.LogError("PortField InputField is null", this);

        var initialIpAddress = PlayerPrefs.GetString(RosBridgeIpAddressKey);
        if (initialIpAddress != null && !initialIpAddress.Equals(""))
            _ipAddressField.text = initialIpAddress;
        else
            _ipAddressField.text = DefaultIpAddress;

        var initialPort = PlayerPrefs.GetInt(RosBridgePortKey, -1);
        if (initialPort >= 0)
            _portField.text = initialPort.ToString();
        else
            _portField.text = DefaultPort.ToString();
        
        _chooseSimulationButton.onClick.AddListener(ChooseSimulation);
	}

	void ChooseSimulation()
	{
        PlayerPrefs.SetString(RosBridgeIpAddressKey, _ipAddressField.text);
        PlayerPrefs.SetInt(RosBridgePortKey, Int32.Parse(_portField.text));

        _chooseSimulationButton.gameObject.SetActive(false);
        _connectionPanel.SetActive(false);
        _chooseSimulationScrollView.SetActive(true);
	}

    public static void LoadSimluationSetupScene()
    {
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
    }
}

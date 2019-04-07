using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Vehicles.Car;

public class EscapeMenu : MonoBehaviour {

    // Escape Menu Objects
    public GameObject MainPanel;
    public Button _exitButton;
    public Button _restartButton;

    private void Start()
    {
        var menus = FindObjectsOfType<EscapeMenu>();
        if (menus.Length > 1)
        {
            //Debug.LogError("There are " + menus.Length + " EscapeMenus already in the scene!");

            // destory excessive menus
            for (int i = menus.Length - 1; i >= 1; i--)
            {
                GameObject.Destroy(menus[i]);
            }
        }
        MainPanel.SetActive(false);

        // setup button listeners
        _exitButton.onClick.AddListener(ExitToSimulationSetup);
        _restartButton.onClick.AddListener(RestartSimulation);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            MainPanel.SetActive(!MainPanel.activeSelf);
    }

    private void ExitToSimulationSetup()
    {
        SimulationSetup.LoadSimluationSetupScene();
    }

    private void RestartSimulation()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour {

    public Button _sceneButton;
    public Text _text;

    public void Start()
    {
        _sceneButton.onClick.AddListener(LoadSimulationModule);
    }

    public void LoadSimulationModule()
    {
        SceneManager.LoadScene(_text.text, LoadSceneMode.Single);
    }

    public string SceneName
    {
        set { _text.text = value; }
    }
}

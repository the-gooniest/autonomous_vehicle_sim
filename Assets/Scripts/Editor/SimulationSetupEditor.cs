using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

[CustomEditor(typeof(SimulationSetup))]
public class SimulationSetupEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        SimulationSetup simulationSetup = (SimulationSetup)target;
        DrawDefaultInspector();
        if (!GUILayout.Button("Refresh Scene Buttons"))
            return;

        if (simulationSetup.SceneButtonsPanel == null || simulationSetup.SceneButtonPrefab == null)
            return;

        var sceneNames = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                string sceneName = Path.GetFileName(scene.path);
                sceneName = sceneName.Substring(0, sceneName.Length - 6);
                if (!sceneName.Equals("SimulationSetup"))
                {
                    Debug.Log(sceneName);
                    sceneNames.Add(sceneName);
                }
            }
        }
        foreach (Transform child in simulationSetup.SceneButtonsPanel.transform)
            DestroyImmediate(child.gameObject);
        foreach (var sceneName in sceneNames)
        {
            var newSceneButton = Instantiate(simulationSetup.SceneButtonPrefab, simulationSetup.SceneButtonsPanel.transform);
            newSceneButton.GetComponent<SceneButton>().SceneName = sceneName;
        }
    }
}
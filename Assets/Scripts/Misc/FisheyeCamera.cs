using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FisheyeCamera : MonoBehaviour {

    public Camera _orthographicCamera;
    Camera[] _cameras;

    public RenderTexture TargetTexture
    {
        get { return _orthographicCamera.targetTexture; }
    }

    void Start()
    {
        _cameras = GetComponentsInChildren<Camera>();
    }

    public void Render()
    {
        foreach (var camera in _cameras)
            camera.Render();
        _orthographicCamera.Render();
    }
}

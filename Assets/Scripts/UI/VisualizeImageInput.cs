using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizeImageInput : MonoBehaviour {

    private Texture2D _texture;

    /// <summary>
    /// Inspector variables
    /// </summary>
    [SerializeField]
    private Toggle _toggle;

    private RawImage _imageInputPanel;

    /// <summary>
    /// Singleton
    /// </summary>
    public static VisualizeImageInput _singleton;
    public static VisualizeImageInput Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = FindObjectOfType<VisualizeImageInput>();
            return _singleton;
        }
    }

    void Awake()
    {
        if (Singleton == null)
            _singleton = this;

        _imageInputPanel = GetComponent<RawImage>();
        _texture = new Texture2D(1280, 720);
        _imageInputPanel.texture = _texture;

        _toggle.onValueChanged.AddListener(EnableVisualization);
    }

    void Start()
    {
        int enabledPref = PlayerPrefs.GetInt("ImageInputEnabled", 0);
        bool wasEnabled = (enabledPref == 1);
        _toggle.isOn = wasEnabled;
        gameObject.SetActive(wasEnabled);
    }

    private void EnableVisualization(bool isOn)
    {
        PlayerPrefs.SetInt("ImageInputEnabled", isOn ? 1 : 0);
        gameObject.SetActive(isOn);
    }

    public void SetTextureWithImageData(byte[] bytes)
    {
        _texture.LoadImage(bytes);
        _texture.Apply();
    }

    public bool ToggleValue
    {
        get { return _toggle.isOn; }
        set { _toggle.isOn = value; }
    }
}

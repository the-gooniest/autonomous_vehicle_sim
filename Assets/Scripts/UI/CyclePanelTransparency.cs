using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CyclePanelTransparency : MonoBehaviour {

    private Image _panelImage;
    public Image PanelImage
    {
        get
        {
            if (_panelImage == null)
                _panelImage = GetComponent<Image>();
            return _panelImage;
        }
    }
       
    private int pingpong = -1;

	void Start () {
        StartCoroutine(CycleTransparency());
	}
	
    IEnumerator CycleTransparency()
    {
        float original_alpha = PanelImage.color.a;
        while (true)
        {
            Color color = PanelImage.color;
            float newAlpha = Mathf.Clamp(color.a + pingpong * 0.25f * Time.deltaTime, 0, original_alpha);
            PanelImage.color = new Color(color.r, color.g, color.b, newAlpha);
            if (newAlpha == 0.0f || newAlpha == original_alpha)
            {
                pingpong *= -1;
                yield return new WaitForSeconds(1.00f);
            }
            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class updateTexture : MonoBehaviour {
	public Texture newTexture; // input from inspector

	// initialization
	void Start () {
		if (newTexture != null) {
			Renderer[] childrenRenderer = transform.GetComponentsInChildren<Renderer> ();

			for (int i = 0; i < childrenRenderer.Length; i++) {
				Renderer currentRenderer = childrenRenderer [i];

				currentRenderer.material.mainTexture = newTexture;
			}
		} else {
			// no texture to update
		}
	}
}
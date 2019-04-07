using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using UnityEditor;

[CustomEditor(typeof(FisheyeCamera))]
public class FisheyeCameraEditor : Editor {

    public override void OnInspectorGUI()
    {
        var fisheye = (FisheyeCamera)target;
        if (GUILayout.Button("CaptureImage"))
            CaptureImage(fisheye, "checkerboard");
        DrawDefaultInspector();
    }

    void CaptureImage(FisheyeCamera camera, string prepend)
    {
        //needed to force camera update 
        camera.Render();
        RenderTexture targetTexture = camera.TargetTexture;
        RenderTexture.active = targetTexture;
        Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        texture2D.Apply();
        byte[] image = texture2D.EncodeToJPG();
        UnityEngine.Object.DestroyImmediate(texture2D);
        string directory = Application.dataPath + "/Textures/Checkerboard";
        string path = Path.Combine(directory, prepend + "_" + System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".jpg");
        File.WriteAllBytes(path, image);
        image = null;
    }
}

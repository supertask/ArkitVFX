using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using RenderUtil;

public class CamerImageController : MonoBehaviour
{
    [SerializeField] public ARCameraManager arCameraManager;
    [SerializeField] public Material humanOcclusionMaterial;
    //[SerializeField] public Camera camera;
    public RenderTexture capturedTex;
    
    void OnEnable()
    {
        //this.capturedTex = new RenderTexture(Screen.width, Screen.height, 0);
        //this.capturedTex = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0);
        //this.capturedTex = RenderTextureUtil.CreateRenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        //this.capturedTex = RenderTextureUtil.CreateRenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);

        // ARCameraManager がカメラフレームを更新したら関数を実行
        arCameraManager.frameReceived += OnCameraFrameReceived;
        
        this.humanOcclusionMaterial.EnableKeyword("ARKIT_BACKGROUND_URP");
    }
    
    void OnDisable()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
    }
    
    void Update()
    {
        if (arCameraManager.cameraMaterial != null)
        {
            // RenderTextureに deep copy
            Graphics.Blit(null, this.capturedTex, arCameraManager.cameraMaterial);
            this.humanOcclusionMaterial.SetTexture("BackgroundTex", this.capturedTex);
        }
    }
    
    void OnGUI ()
    {
    }
}
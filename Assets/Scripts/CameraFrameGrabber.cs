using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using System;
using Unity.Collections;

public class CameraFrameGrabber : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public Action<Texture2D> OnFrameReceived;

    Texture2D cameraTexture;

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrame;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrame;
    }

    void OnCameraFrame(ARCameraFrameEventArgs args)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
            return;

        using (cpuImage)
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.None
            };

            int dataSize = cpuImage.GetConvertedDataSize(conversionParams);

            // Allocate NativeArray for the conversion
            var buffer = new NativeArray<byte>(dataSize, Allocator.Temp);

            // Convert CPU image → buffer
            cpuImage.Convert(conversionParams, buffer);

            // Create / Resize texture if needed
            if (cameraTexture == null ||
                cameraTexture.width != cpuImage.width ||
                cameraTexture.height != cpuImage.height)
            {
                cameraTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);
            }

            // Copy buffer → texture
            cameraTexture.LoadRawTextureData(buffer);
            cameraTexture.Apply();

            buffer.Dispose();

            // Notify subscribers
            OnFrameReceived?.Invoke(cameraTexture);
        }
    }
}

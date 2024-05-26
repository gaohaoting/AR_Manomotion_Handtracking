using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputManagerStereoBase : InputManagerBase
{
    [SerializeField] List<CameraInfo> cameraInfo;

    // Textures for left and right images
    Texture2D[] textures = new Texture2D[2];

    public static Action<CameraInfo[]> OnCameraInfoUpdated;

    protected const TextureFormat Format = TextureFormat.RGBA32;

    protected virtual void InitializeTextures(Vector2Int resolution)
    {
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new Texture2D(resolution.x, resolution.y, Format, false);
        }

        currentFrame = new ManoMotionFrame();
        currentFrame.texture = textures[0];
        currentFrame.textureSecond = textures[1];

        OnFrameInitializedPointers?.Invoke(textures[0], textures[1], splittingFactor);
    }

    protected void UpdateTextures(Color32[] left, Color32[] right)
    {
        for (int i = 0; i < textures.Length; i++)
        {
            Color32[] pixels = i == 0 ? left : right;
            textures[i].SetPixels32(pixels);
            textures[i].Apply();
        }
    }

    protected void UpdateTextures(RenderTexture left, RenderTexture right)
    {
        for (int i = 0; i < textures.Length; i++)
        {
            RenderTexture rTexture = i == 0 ? left : right;
            textures[i].ReadPixels(new Rect(0, 0, rTexture.width, rTexture.height), 0, 0);
            textures[i].Apply();
        }
    }
}
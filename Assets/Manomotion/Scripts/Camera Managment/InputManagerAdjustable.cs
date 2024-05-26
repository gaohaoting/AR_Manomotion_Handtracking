using System;
using UnityEngine;

namespace CameraSystem
{
    public class InputManagerAdjustable : InputManagerBase
    {
        [Tooltip("When true the background will be zoomed in to cover the entire screen, when false the entire image will be visible but there will be black borders.")]
        [SerializeField] bool shouldBackgroundCoverScreen;
        [SerializeField] AddOn addOn;

        private WebCamTexture currentPlayingCamera;
        int textureWidth, textureHeight;
        bool isFrameUpdated;

        const int LOWEST_RESOLUTION_VALUE = 480;

        private void Awake()
        {
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            float ratio = (float)LOWEST_RESOLUTION_VALUE / Mathf.Min(screenWidth, screenHeight);
            textureWidth = (int)(screenWidth * ratio);
            textureHeight = (int)(screenHeight * ratio);

            ForceApplicationPermissions();
        }

        private void Start()
        {
            OnAddonSet?.Invoke(addOn);
            InitializeManoMotionFrame();
            HandleNewCameraDeviceSelected();
        }

        void Update()
        {
            ManoUtils.Instance.ShouldBackgroundCoverScreen(shouldBackgroundCoverScreen);
            isFrameUpdated = currentPlayingCamera.didUpdateThisFrame;

            if (isFrameUpdated)
            {
                GetCameraFrameInformation();
                OnFrameInitializedPointer?.Invoke(currentFrame.texture);
            }
        }

        private void HandleNewCameraDeviceSelected()
        {
            // Stop current camera
            if (currentPlayingCamera)
            {
                currentPlayingCamera.Stop();
                currentPlayingCamera = null;
            }

            // Assign new camera
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                WebCamDevice device = WebCamTexture.devices[i];

                if (device.isFrontFacing == isFrontFacing)
                {
                    //Debug.Log($"Handling a new camera {i}, isFrontFacing: {isFrontFacing}");
                    currentPlayingCamera = new WebCamTexture(device.name, textureWidth, textureHeight);
                    break;
                }
            }

            if (!currentPlayingCamera)
            {
                currentPlayingCamera = new WebCamTexture(WebCamTexture.devices[0].name, textureWidth, textureHeight);
            }

            // Start new camera
            if (currentPlayingCamera)
            {
                currentPlayingCamera.Play();
                ResizeManoMotionFrameResolution(currentPlayingCamera.width, currentPlayingCamera.height);
            }

            OnChangeCamera?.Invoke();
        }

        /// <summary>
        /// Initializes the ManoMotion Frame and lets the subscribers of the event know of its information.
        /// </summary>
        private void InitializeManoMotionFrame()
        {
            currentFrame = new ManoMotionFrame();
            ResizeManoMotionFrameResolution(textureWidth, textureHeight);
            currentFrame.orientation = Input.deviceOrientation;

            OnFrameInitialized?.Invoke(currentFrame);
        }

        /// <summary>
        /// Gets the camera frame pixel colors.
        /// </summary>
        protected void GetCameraFrameInformation()
        {
            if (!currentPlayingCamera)
            {
                Debug.LogWarning("No device camera available");
                HandleNewCameraDeviceSelected();
                return;
            }

            Color32[] pixels = currentPlayingCamera.GetPixels32();

            if (pixels.Length < 300)
            {
                Debug.LogWarning("The frame from the camera is too small. Pixel array length:  " + pixels.Length);
                return;
            }

            if (currentFrame.texture.GetPixels32().Length != pixels.Length)
            {
                ResizeManoMotionFrameResolution(currentPlayingCamera.width, currentPlayingCamera.height);
                return;
            }

            currentFrame.texture.SetPixels32(pixels); 

            //Flip the texture if using front facing to match the image to the device.
            if (isFrontFacing)
            {
#if UNITY_ANDROID || UNITY_STANDALONE
                FlipTextureHorizontal(ref currentFrame.texture);
#elif UNITY_IOS
                FlipTextureVertical(ref currentFrame.texture);
#endif
            }

            currentFrame.orientation = Input.deviceOrientation;
            OnFrameUpdated?.Invoke(currentFrame);
        }

        /// <summary>
        /// Sets the resolution of the currentManoMotion frame that is passed to the subscribers that want to make use of the input camera feed.
        /// </summary>
        /// <param name="width">Requires a width value.</param>
        /// <param name="height">Requires a height value.</param>
        protected void ResizeManoMotionFrameResolution(int width, int height)
        {
            currentFrame.texture = new Texture2D(width, height, TextureFormat.RGBA32, true);
            currentFrame.texture.Apply();

            OnFrameResized?.Invoke(currentFrame);
            Texture2D image = currentFrame.texture;
            image.filterMode = FilterMode.Trilinear;
            image.Apply();
            OnFrameInitializedPointer?.Invoke(image, splittingFactor);
        }

        protected override void UpdateFrontFacing(bool isFrontFacing)
        {
            HandleNewCameraDeviceSelected();
        }

        public override bool IsFrameUpdated()
        {
            return isFrameUpdated;
        }

        /// <summary>
        /// Flips the texture Horizontaly
        /// </summary>
        /// <param name="original">the ref to the texture to filp</param>
        public void FlipTextureHorizontal(ref Texture2D original)
        {
            int textureWidth = original.width;
            int textureHeight = original.height;

            Color[] colorArray = original.GetPixels();

            for (int j = 0; j < textureHeight; j++)
            {
                int rowStart = 0;
                int rowEnd = textureWidth - 1;

                while (rowStart < rowEnd)
                {
                    Color hold = colorArray[(j * textureWidth) + (rowStart)];
                    colorArray[(j * textureWidth) + (rowStart)] = colorArray[(j * textureWidth) + (rowEnd)];
                    colorArray[(j * textureWidth) + (rowEnd)] = hold;
                    rowStart++;
                    rowEnd--;
                }
            }

            original.SetPixels(colorArray);
            original.Apply();
        }

        /// <summary>
        /// Flips the texture Verticaly.
        /// </summary>
        /// <param name="orignal">The ref to the texture to flip</param>
        public void FlipTextureVertical(ref Texture2D orignal)
        {
            int width = orignal.width;
            int height = orignal.height;
            Color[] pixels = orignal.GetPixels();
            Color[] pixelsFlipped = orignal.GetPixels();
            for (int i = 0; i < height; i++)
            {
                Array.Copy(pixels, i * width, pixelsFlipped, (height - i - 1) * width, width);
            }
            orignal.SetPixels(pixelsFlipped);
            orignal.Apply();
        }

        /// <summary>
        /// Start the camera when enabled.
        /// </summary>
        private void OnEnable()
        {
            if (currentPlayingCamera)
            {
                if (!currentPlayingCamera.isPlaying)
                {
                    currentPlayingCamera.Play();
                }
            }
            else
            {
                Debug.LogWarning("I dont have a backfacing Camera");
            }
        }

        /// <summary>
        /// Stops the camera when disabled.
        /// </summary>
        private void OnDisable()
        {
            if (currentPlayingCamera && !currentPlayingCamera.isPlaying)
            {
                currentPlayingCamera.Stop();
            }
        }
    }
}
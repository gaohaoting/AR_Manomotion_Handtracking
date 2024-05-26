using System;
using UnityEngine;

[Serializable]
public struct ManoMotionFrame
{
	// Main image, used for single image input
	public Texture2D texture;

	// Right image when using stereo input
	public Texture2D textureSecond;

	// The orientation of the device. 
	public DeviceOrientation orientation;
}

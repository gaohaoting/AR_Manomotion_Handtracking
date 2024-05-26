using UnityEngine;

/// <summary>
/// Used for visualization of camera image
/// </summary>
public struct VisualizationInfo
{
    /// <summary>
    /// The Texture 2D information of the input image.
    /// </summary>
    public Texture2D rgbImage;

    /// <summary>
    /// The Texture 2D information of the second input image for stereo input.
    /// </summary>
    public Texture2D rgbImageSecond;


    /// <summary>
    /// The Texture 2D information of the cutout image.
    /// </summary>
    public Texture2D occlusionRGB;

    /// <summary>
    /// The Texture 2D information of the cutout image.
    /// </summary>
    public Texture2D occlusionRGBsecond;
}
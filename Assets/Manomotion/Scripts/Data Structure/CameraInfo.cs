[System.Serializable]
public struct CameraInfo
{
    /// <summary>
    /// <para>DEFAULT: https://docs.opencv.org/master/d4/d94/tutorial_camera_calibration.html
    /// 5 distortion coefficients</para>
    /// <para>FISHEYE: https://docs.opencv.org/3.4/db/d58/group__calib3d__fisheye.html
    /// 4 distortion coefficients</para>
    /// <para>OMNIDIRECTIONAL: https://docs.opencv.org/master/dd/d12/tutorial_omnidir_calib_main.html
    /// 6 distortion coefficients</para>
    /// </summary>
    public enum CAMERA_MODEL
    {
        DEFAULT,
        FISHEYE,
        OMNIDIRECTIONAL
    };

    public CAMERA_MODEL model;

    //The size of image over normalized_bounding_box_x and normalized_bounding_box_y directions
    public int imageX, imageY;
    public float focusX, focusY, principalPointX, principalPointY;

    /// <summary>
    /// 4-6 values depending on the model. See CAMERA_MODEL
    /// </summary>
    public float[] distortionCoefficients;

    /// <summary>
    /// Should have 16 values
    /// </summary>
    public float[] extrinsicParameters;
}
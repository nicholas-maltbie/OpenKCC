namespace nickmaltbie.OpenKCC.CameraControls
{
    public interface IManagedCamera
    {
        /// <summary>
        /// Desired pitch of the camera.
        /// </summary>
        float Pitch { get; set; }

        /// <summary>
        /// Desired yaw of the camera.
        /// </summary>
        float Yaw { get; set; }

        /// <summary>
        /// Gets or sets the previous opacity for the third person character base.
        /// </summary>
        float PreviousOpacity { get; set; }
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "Camera Track", menuName = "ArcticGame/Camera Track")]
public class CameraTrack : ScriptableObject
{
    //Camera tracks are used by vehicles to override the thirdperson camera offset 
    //so larger vehicles can still be fit on screen

    //local offset from PlayerBrain to PlayerCam
    public Vector3 CamTrack;
}

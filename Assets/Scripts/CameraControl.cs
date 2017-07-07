using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform Target;
    public float Distance;
    public float VertAngle;

    void LateUpdate()
    {
        TrackTarget();
    }

    [ContextMenu("Track Target")]
    void TrackTarget()
    {
        if (Target != null)
        {
            Vector3 dirTarget2Cam = Quaternion.Euler(VertAngle, 0f, 0f) * Vector3.back;
            transform.position = Target.position + dirTarget2Cam * Distance;
            transform.forward = -dirTarget2Cam;
        }
    }
}

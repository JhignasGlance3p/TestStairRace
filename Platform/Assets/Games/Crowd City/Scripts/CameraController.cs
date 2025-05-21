using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class CameraController : MonoBehaviour
    {
        public Transform target; // The target to follow (player in most cases)
        public float smoothSpeed = 0.125f; // Smoothness factor for camera movement
        public Vector3 offset; // Offset from the target position

        void LateUpdate()
        {
            if (target != null)
            {
                Vector3 desiredPosition = target.position + offset;
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
                transform.position = smoothedPosition;

                transform.LookAt(target); // Make the camera look at the target
            }
        }
    }
}
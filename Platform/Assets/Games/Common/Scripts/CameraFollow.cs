using UnityEngine;

namespace nostra.origami.common
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 0.125f;  // Smoothing factor for camera movement
        private Vector3 offset;

        [Range(-180f, 180f)] public float orbitAngle = 0f; // Slider for orbit angle

        private void Start()
        {
            offset = transform.position - target.position;
        }

        private void LateUpdate()
        {
            // Calculate the new offset based on the orbit angle
            Quaternion rotation = Quaternion.Euler(0, orbitAngle, 0);
            Vector3 newOffset = rotation * offset;

            // Calculate the desired camera position
            Vector3 desiredPosition = target.position + newOffset;

            // Smoothly interpolate to the desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Make the camera look at the target
            transform.LookAt(target);
        }
    }
}

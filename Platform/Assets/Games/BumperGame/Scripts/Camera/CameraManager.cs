using UnityEngine;

namespace com.nampstudios.bumper.CameraUnit
{
    public class CameraManager : MonoSingletonGeneric<CameraManager>
    {
        [SerializeField] private Vector3 offset;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private float smoothSpeed;
        [Range(0, 1)]
        [SerializeField] private float zOffsetReduction;

        private Transform targetTransform;
        private bool initialized = false;
        private Transform cameraTransform;
        private Vector3 initialOffset;
        private Vector3 adjustedOffset;

        private Vector3 desiredPosition;
        private Vector3 smoothedPosition;

        void Start()
        {
            initialOffset = offset;
            adjustedOffset = offset;
        }
        void LateUpdate()
        {
            if (!initialized)
                return;

            if (targetTransform != null)
            {
                AdjustOffsetBasedOnRotation();
                desiredPosition = targetTransform.position + adjustedOffset;
                desiredPosition.y = initialOffset.y;
                smoothedPosition = Vector3.Lerp(cameraTransform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                cameraTransform.position = smoothedPosition;
            }
        }

        public void InitCamera(Transform target)
        {
            initialized = true;
            targetTransform = target;
        }
        public void setCamera(Camera Cam)
        {
            cameraTransform = Cam.transform;
            cameraTransform.rotation = Quaternion.Euler(rotation);
        }

        private void AdjustOffsetBasedOnRotation()
        {
            float playerRotation = Mathf.Abs(targetTransform.eulerAngles.y);

            if (Mathf.Abs(playerRotation - 180f) < 60f)
            {
                adjustedOffset = new Vector3(initialOffset.x, initialOffset.y, initialOffset.z * (1 - zOffsetReduction));
            }
            else
            {
                adjustedOffset = initialOffset;
            }
        }
    }
}
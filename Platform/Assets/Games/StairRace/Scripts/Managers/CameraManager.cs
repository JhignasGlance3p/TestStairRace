using UnityEngine;
using Unity.Mathematics;

namespace nostra.pkpl.stairrace
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] float3 offset;
        [SerializeField] float3 rotation;
        [SerializeField] float smoothSpeed;
        [SerializeField] float fieldOfView = 65f;
        [SerializeField] Transform cameraTransform;

        private Transform targetTransform;
        private float3 defaultOffset;

        private void Start()
        {
            defaultOffset = offset;
            cameraTransform.rotation = Quaternion.Euler(rotation);
            Camera.main.fieldOfView = fieldOfView;
        }

        private void LateUpdate()
        {
            if (targetTransform != null)
            {
                float3 desiredPosition = (float3)targetTransform.position + offset;
                float3 smoothedPosition = Vector3.Lerp(cameraTransform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                cameraTransform.position = smoothedPosition;
            }
        }

        public void SetTarget(Transform target)
        {
            targetTransform = target;
            cameraTransform.position = (float3)targetTransform.position + offset;
        }

        public void ChangeCamera(int index)
        {
            switch (index)
            {
                case 1:
                    offset = new float3(1f, 0.6f, 0.5f);
                    cameraTransform.localEulerAngles = new float3(25f, -110f, 0f);
                    Camera.main.fieldOfView = 90;
                    break;
                case 2:
                    offset = new float3(-1f, 0.6f, 0.5f);
                    cameraTransform.localEulerAngles = new float3(25f, 110f, 0f);
                    Camera.main.fieldOfView = 90;
                    break;
                default:
                    offset = defaultOffset;
                    cameraTransform.localEulerAngles = rotation;
                    Camera.main.fieldOfView = fieldOfView;
                    break;
            }
        }
    }
}
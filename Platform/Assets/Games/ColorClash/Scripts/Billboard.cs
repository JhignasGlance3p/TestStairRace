using UnityEngine;

namespace nostra.sarvottam.colorclash
{
    public class Billboard : MonoBehaviour
    {
        Camera mainCamera;

        public void SetCamera(Camera _camera)
        {
            mainCamera = _camera;
        }
        void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
}
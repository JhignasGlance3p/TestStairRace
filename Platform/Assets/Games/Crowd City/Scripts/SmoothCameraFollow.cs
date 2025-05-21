using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        public Transform playerTarget;
        [SerializeField] private Vector3 offset;

        private void LateUpdate()
        {
            transform.position = playerTarget.position - transform.rotation * offset;
        }
    }
}
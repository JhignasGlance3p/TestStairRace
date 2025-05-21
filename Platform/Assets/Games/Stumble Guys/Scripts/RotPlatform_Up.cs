using UnityEngine;

namespace nostra.origami.stumble
{
    public class RotPlatform_Up : MonoBehaviour
    {
        [SerializeField] float rotatingSpeed = 130;

        void Update()
        {
            transform.Rotate(Vector3.up, rotatingSpeed * Time.deltaTime);
        }
    }
}
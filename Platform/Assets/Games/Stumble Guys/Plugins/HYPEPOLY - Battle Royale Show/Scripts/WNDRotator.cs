using UnityEngine;

namespace nostra.origami.stumble
{
    public class WNDRotator : MonoBehaviour
    {
        [SerializeField] Vector3 rotationSpeed;

        void FixedUpdate()
        {
            transform.Rotate(rotationSpeed);
        }
    }
}
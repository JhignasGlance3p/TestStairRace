using UnityEngine;

namespace nostra.origami.stumble
{
    public class RotateCenter : MonoBehaviour
    {
        [SerializeField] float rotatingSpeed = 130;

        void Update()
        {
            transform.Rotate(Vector3.forward, rotatingSpeed * Time.deltaTime);
        }
    }
}
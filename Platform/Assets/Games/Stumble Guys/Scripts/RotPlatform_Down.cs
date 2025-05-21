using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.origami.stumble
{
    public class RotPlatform_Down : MonoBehaviour
    {
        [SerializeField] float rotatingSpeed = 130;

        void Update()
        {
            transform.Rotate(Vector3.down, rotatingSpeed * Time.deltaTime);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace nostra.origami.stumble
{
    public class LHS_Camera : MonoBehaviour
    {
        [SerializeField] GameObject player;
        public float X { get; set; }
        public float Y { get; set; }
        public float distance = 10;
        public Touch PanningTouch { get; private set; }

        void LateUpdate()
        {
            // CameraRotate(); TODO
        }
        void CameraRotate()
        {
            // if (InputManager.keyboardControls && !EventSystem.current.IsPointerOverGameObject())
            // {
            //     X += Input.GetAxis("Mouse X");
            //     Y -= Input.GetAxis("Mouse Y");
            // }

            // Y = Mathf.Clamp(Y, -10, 30);
            // transform.rotation = Quaternion.Euler(Y, X, 0);
            // Vector3 reDistance = new Vector3(0f, -4f, distance);

            // if (player != null)
            // {
            //     //Vector3 desiredPosition = player.transform.position + offset;
            //     transform.position = player.transform.position - transform.rotation * reDistance;
            // }
        }
    }
}
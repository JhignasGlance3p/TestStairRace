using System.Collections;
using UnityEngine;

namespace com.nampstudios.bumper.Zone
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private Transform leftDoor;
        [SerializeField] private Transform rightDoor;
        [SerializeField] private float rotationSpeed = 1.0f;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private Collider[] blockColliders;

        private Quaternion leftDoorClosedRotation;
        private Quaternion rightDoorClosedRotation;
        private Quaternion leftDoorOpenRotation;
        private Quaternion rightDoorOpenRotation;
        private bool isOpen = false;
        private bool init;
        private Coroutine leftdoorRoutine, rightdoorRoutine;

        private void Start()
        {
            if (!init)
                Initialize();
        }

        public void OpenDoor()
        {
            if (!init)
                Initialize();

            if (!isOpen)
            {
                foreach (var collider in blockColliders)
                    collider.enabled = false;
                if (leftdoorRoutine != null)
                    StopCoroutine(leftdoorRoutine);
                if (rightdoorRoutine != null)
                    StopCoroutine(rightdoorRoutine);
                if (this.gameObject.activeInHierarchy)
                {
                    leftdoorRoutine = StartCoroutine(RotateDoor(leftDoor, leftDoorOpenRotation, rotationSpeed));
                    rightdoorRoutine = StartCoroutine(RotateDoor(rightDoor, rightDoorOpenRotation, rotationSpeed));
                }
                isOpen = true;
            }
        }
        public void CloseDoor()
        {
            if (!init)
                Initialize();

            if (isOpen)
            {
                foreach (var collider in blockColliders)
                    collider.enabled = true;
                if (leftdoorRoutine != null)
                    StopCoroutine(leftdoorRoutine);
                if (rightdoorRoutine != null)
                    StopCoroutine(rightdoorRoutine);
                if (this.gameObject.activeInHierarchy)
                {
                    leftdoorRoutine = StartCoroutine(RotateDoor(leftDoor, leftDoorClosedRotation, rotationSpeed));
                    rightdoorRoutine = StartCoroutine(RotateDoor(rightDoor, rightDoorClosedRotation, rotationSpeed));
                }
                isOpen = false;
            }
        }

        private void Initialize()
        {
            leftDoorClosedRotation = leftDoor.localRotation;
            rightDoorClosedRotation = rightDoor.localRotation;
            leftDoorOpenRotation = Quaternion.Euler(0, -openAngle, 0);
            rightDoorOpenRotation = Quaternion.Euler(0, openAngle, 0);
            init = true;
        }
        IEnumerator RotateDoor(Transform door, Quaternion targetRotation, float speed)
        {
            while (Quaternion.Angle(door.localRotation, targetRotation) > 0.01f)
            {
                door.localRotation = Quaternion.Slerp(door.localRotation, targetRotation, Time.deltaTime * speed);
                yield return null;
            }
            door.localRotation = targetRotation;
        }
    }
}
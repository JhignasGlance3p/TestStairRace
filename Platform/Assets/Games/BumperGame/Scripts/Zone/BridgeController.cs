using System;
using UnityEngine;

namespace com.nampstudios.bumper.Zone
{
    public class BridgeController : MonoBehaviour
    {
        [SerializeField] private DoorController entryDoor;
        [SerializeField] private DoorController exitDoor;

        public event Action OnEnterZone;
        private bool enteredZone = false;
        private bool bridgeOpen = false;

        void OnTriggerEnter(Collider other)
        {
            if (enteredZone || !bridgeOpen)
                return;

            if (other.CompareTag("Player"))
            {
                enteredZone = true;
                exitDoor.OpenDoor();
                entryDoor.CloseDoor();
                OnEnterZone?.Invoke();
            }
        }

        public void OpenBridge()
        {
            entryDoor.OpenDoor();
            bridgeOpen = true;
        }
        public void CloseBridge()
        {
            exitDoor.CloseDoor();
            entryDoor.CloseDoor();
            bridgeOpen = false;
        }
        public void reset()
        {
            CloseBridge();
            enteredZone = false;
            bridgeOpen = false;
        }
    }
}
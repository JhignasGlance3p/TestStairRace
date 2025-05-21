using nostra.input;
using System;
using UnityEngine;

namespace com.nampstudios.bumper.Shared
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] FloatingJoystick joystick;
        [SerializeField] private float afkDelay = 3f;
        public event Action<Vector2> OnMovementInput;
        public event Action OnStopInput;
        private bool onStopFired = false;

        private Vector2 movementInput;
        private bool isGameRunning;
        private float timer;

        private void Start()
        {
            timer = 0f;
        }

        public bool IsPlayerIdle => timer > afkDelay;

        public void SetGameRunning(bool value)
        {
            isGameRunning = value;
        }
        public void CallPush()
        {
            OnStopInput?.Invoke();
        }
        private void Update()
        {
            if (!isGameRunning) return;
            OnMovementInput?.Invoke(joystick.Direction);
            if (movementInput != Vector2.zero)
            {
                timer = 0f;
                onStopFired = false;
            }
            else
            {
                timer += Time.deltaTime;
                if (onStopFired)
                    return;
                onStopFired = true;
                OnStopInput?.Invoke();
            }
        }
    }
}

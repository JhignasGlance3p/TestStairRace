using System;
using UnityEngine;

namespace com.nampstudios.bumper.Game
{
    public class TrackPlayer : MonoBehaviour
    {
        private Transform targetTransform;
        private bool init = false;
        private Vector3 initialPos;
        private void Start()
        {
            initialPos = transform.position;
            // Init();
        }

        private void LateUpdate()
        {
            if (!init)
            {
                //Init();
                return;
            }
            if (targetTransform != null)
                transform.position = initialPos + targetTransform.position;
        }
        public void onGameOver()
        {
            init = false;
        }
        public void Init(GameManager _gameManager)
        {
            if (init == true) return;

            targetTransform = _gameManager.Player.transform;
            init = true;
        }
    }
}
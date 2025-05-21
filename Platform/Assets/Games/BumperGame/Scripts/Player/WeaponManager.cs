using com.nampstudios.bumper.Enemy;
using UnityEngine;

namespace com.nampstudios.bumper.Player
{
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] private Pusher pusher;
        [SerializeField] private Hitter hitter;
        [SerializeField] private float weaponTravelDistance;
        [SerializeField] private float boostedWeaponTravelDistance;
        [SerializeField] private float weaponPushMultiplier;
        [SerializeField] private float forceApplyDelay;
        [SerializeField] private float contactForceMultiplier;

        GameManager m_gameManager;
        private float distance = 0f;
        private Vector3 scale;

        public void Initialise(GameManager _gameManager)
        {
            m_gameManager = _gameManager;
            distance = weaponTravelDistance;
            scale = transform.localScale;
            hitter.Init(weaponPushMultiplier);
            var input = m_gameManager.Input_Manager;
            input.OnStopInput += ThrowWeapon;
        }
        public void on_Start()
        {
        }
        public void SetBoost(bool value, float boostMultiplier = 1)
        {
            if (value)
            {
                distance = boostedWeaponTravelDistance;
                hitter.Init(boostMultiplier);
                var scale = transform.localScale;
                scale.x *= 1.25f;
                transform.localScale = scale;
            }
            else
            {
                distance = weaponTravelDistance;
                hitter.Init(weaponPushMultiplier);
                transform.localScale = scale;
            }
        }

        private void ThrowWeapon()
        {
            pusher.ThrowWeapon(distance);
        }

        private void OnCollisionEnter(Collision collision)
        {
            var controller = collision.gameObject.GetComponent<EnemyController>();
            if (controller != null && !m_gameManager.IsPlayerIdle)
            {
                controller.TakeSwing(transform.forward, contactForceMultiplier);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            var controller = collision.gameObject.GetComponent<EnemyController>();
            if (controller != null && !m_gameManager.IsPlayerIdle)
            {
                controller.TakeSwing(transform.forward);
            }
        }

        private void OnDisable()
        {
            if (m_gameManager != null)
            {
                var input = m_gameManager.Input_Manager;
                input.OnStopInput -= ThrowWeapon;
            }
        }
    }
}

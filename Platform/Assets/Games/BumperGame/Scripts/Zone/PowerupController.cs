using UnityEngine;

namespace com.nampstudios.bumper.Zone
{
    public class PowerupController : MonoBehaviour
    {
        [SerializeField] private Animator effectAnimator;
        [SerializeField] private Sprite speedBoostSprite;
        [SerializeField] private Sprite invincibilitySprite;
        [SerializeField] private Sprite powerBoostSprite;

        private PowerupType powerupType;
        private Transform cameraTransform;
        private SpriteRenderer spriteRenderer;
        private bool powerupDone;
        private GameManager m_gameManager;

        private void Awake()
        {
            cameraTransform = Camera.main.transform;
            spriteRenderer = GetComponent<SpriteRenderer>();
            powerupDone = false;
        }
        public void Initialize(GameManager _gameManager, PowerupType type)
        {
            m_gameManager = _gameManager;
            powerupType = type;
            SetupSprites();
        }


        private void Update()
        {
            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;
            transform.rotation = Camera.main.transform.rotation;
        }

        private void SetupSprites()
        {
            switch (powerupType)
            {
                case PowerupType.SpeedBoost:
                    spriteRenderer.sprite = speedBoostSprite;
                    break;
                case PowerupType.PowerBoost:
                    spriteRenderer.sprite = powerBoostSprite;
                    break;
                case PowerupType.Invincibility:
                    spriteRenderer.sprite = invincibilitySprite;
                    break;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if ((!other.CompareTag(GameConstants.PLAYER_TAG) && !other.CompareTag(GameConstants.WEAPON_TAG)) || powerupDone)
                return;
            powerupDone = true;
            spriteRenderer.enabled = false;
            effectAnimator.gameObject.SetActive(true);
            m_gameManager.Ui_Manager.ShowText($"{powerupType} Achieved");
            var controller = m_gameManager.Player;
            if (controller != null)
            {
                switch (powerupType)
                {
                    case PowerupType.SpeedBoost:
                        controller.SpeedBoost();
                        effectAnimator.Play(GameConstants.SPEEDBOOST_ANIM);
                        break;
                    case PowerupType.PowerBoost:
                        effectAnimator.Play(GameConstants.POWERBOOST_ANIM);
                        controller.PowerBoost();
                        break;
                    case PowerupType.Invincibility:
                        effectAnimator.Play(GameConstants.INVINCIBILITY_ANIM);
                        controller.Invincibility();
                        break;
                }
            }
            Destroy(gameObject, 1.2f);
        }
    }
}

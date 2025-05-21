using DG.Tweening;
using TMPro;
using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class PowerUp : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI powerUpText;

        int operation;
        int randomValue;
        bool m_canTrigger = false;

        PowerupStatus storingData;
        public PowerupStatus PowerupStatus => storingData;

        void Awake()
        {
            transform.DOLocalMoveY(0.8f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }
        void Update()
        {
            transform.Rotate(Vector3.up * 20f * Time.deltaTime);
        }
        void OnTriggerEnter(Collider other)
        {
            if (this.gameObject.activeInHierarchy == false || m_canTrigger == false)
            {
                return;
            }
            if (other.CompareTag("Character"))
            {
                CrowdHandler crowdHandler = other.GetComponentInParent<CrowdHandler>();
                if (crowdHandler == null)
                    return;

                switch (operation)
                {
                    case 0:

                        crowdHandler.UpdateCrowd(randomValue);
                        break;
                    case 1:

                        crowdHandler.RemoveCrowd(removeCount: randomValue);
                        break;
                    case 2:

                        int result = (crowdHandler.CrowdCount * randomValue) - crowdHandler.CrowdCount;
                        crowdHandler.UpdateCrowd(result);
                        break;
                    default:

                        break;
                }

                storingData.isVisible = false;
                gameObject.SetActive(false);
            }
        }

        public void SetPowerup(int _operation, int _value, bool _canTrigger = true)
        {
            randomValue = _value;
            operation = _operation;
            m_canTrigger = _canTrigger;

            if (powerUpText == null)
            {
                powerUpText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            }
            switch (operation)
            {
                case 0:

                    powerUpText.text = $"+{randomValue}";
                    break;
                case 1:

                    powerUpText.text = $"-{randomValue}";
                    break;
                case 2:

                    powerUpText.text = $"x{randomValue}";
                    break;
                default:

                    break;
            }
            gameObject.SetActive(true);

            storingData = new PowerupStatus();
            storingData.isVisible = true;
            storingData.operation = operation;
            storingData.randomValue = randomValue;
        }
        public void UpdateStatus(PowerupStatus status)
        {
            this.gameObject.SetActive(status.isVisible);
            switch (status.operation)
            {
                case 0:

                    powerUpText.text = $"+{status.randomValue}";
                    break;
                case 1:

                    powerUpText.text = $"-{status.randomValue}";
                    break;
                case 2:

                    powerUpText.text = $"x{status.randomValue}";
                    break;
                default:

                    break;
            }
        }
        public void OnGameOver()
        {
            operation = 0;
            randomValue = 0;
            m_canTrigger = false;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

namespace com.nampstudios.bumper.UI
{
    public class UiManager : MonoBehaviour
    {
        const int maxTimer = 50; //500
        [SerializeField] private int sliderMaxValue;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Image timeSliderfillImage;
        [SerializeField] private GameObject gameUiPanel;
        [SerializeField] private TextMeshProUGUI timeRemainingTxt;
        [SerializeField] private TextMeshProUGUI scoreTxt;

        [Header("CoinFly")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private RectTransform hudCoinTransform;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float flyDuration = 1f;
        [SerializeField] private float popupHeight = 0.5f;

        [Header("PowerupTxt")]
        [SerializeField] private TextMeshProUGUI powerupTxt;

        float remainingTime;
        bool isGameRunning;
        Coroutine coinsFlyRoutine;
        GameManager m_gameManager;

        void Update()
        {
            if (isGameRunning)
                UpdateSlider();
        }
        void OnDisable()
        {
            if (coinsFlyRoutine != null)
                StopCoroutine(coinsFlyRoutine);
        }

        public void Initialise(GameManager _gameManager)
        {
            m_gameManager = _gameManager;
            sliderMaxValue = maxTimer;
            scoreTxt.text = $"0";
            powerupTxt.gameObject.SetActive(false);
        }

        public void SpawnCoins(Vector3 enemyPosition, int coinCount)
        {
            coinsFlyRoutine = StartCoroutine(SpawnCoinsRoutine(enemyPosition, coinCount));
        }
        public void ShowText(string text)
        {
            powerupTxt.gameObject.SetActive(true);
            powerupTxt.text = $"{text}";
            powerupTxt.gameObject.GetComponent<Animator>().Play("Highlight");
            Invoke(nameof(DisablePowerupTxt), 2f);
        }
        public void UpdateScore(int currentScore)
        {
            if (isGameRunning)
            {
                scoreTxt.text = $"{currentScore}";
            }
        }
        public void OnGameOver(bool winner)
        {
            isGameRunning = false;
            m_gameManager.stopTrackPlayer();
            // gameOverPanel.gameObject.SetActive(true);
            // gameOverPanel.Initialize(winner, OnRestartClicked);
            // startGamePanel.SetActive(false);
            // gameUiPanel.SetActive(false);
        }
        // public void OnStartBtnClicked()
        // {
        //     sliderMaxValue = maxTimer;
        //     m_gameManager.TriggerGameStart();
        //     startGamePanel.SetActive(false);
        //     gameUiPanel.SetActive(true);
        //     SetTimer();
        //     isGameRunning = true;
        // }

        // public void OnRestartClicked()
        // {
        //     sliderMaxValue = maxTimer;
        //     m_gameManager.RestartScene();
        //     isGameRunning = true;
        //     gameOverPanel.gameObject.SetActive(false);
        //     gameUiPanel.SetActive(true);
        // }

        void SetTimer()
        {
            timerSlider.maxValue = sliderMaxValue;
            timerSlider.value = sliderMaxValue;
            timeSliderfillImage.color = Color.green;
            remainingTime = sliderMaxValue;
            timeRemainingTxt.text = $"{(int)remainingTime}";
        }
        void UpdateSlider()
        {
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                if (timerSlider.value == (int)remainingTime)
                    return;
                timerSlider.value = (int)remainingTime;
                timeRemainingTxt.text = $"{(int)remainingTime}";
                float normalizedTime = remainingTime / timerSlider.maxValue;
                if (normalizedTime > 0.5f)
                {
                    timeSliderfillImage.color = Color.Lerp(Color.yellow, Color.green, (normalizedTime - 0.5f) * 2);
                }
                else
                {
                    timeSliderfillImage.color = Color.Lerp(Color.red, Color.yellow, normalizedTime * 2);
                }
            }
            else
            {
                remainingTime = 0;
                if (remainingTime <= 0)
                {
                    m_gameManager.TriggerGameOver(true);
                }
            }
        }
        IEnumerator SpawnCoinsRoutine(Vector3 enemyPosition, int coinCount)
        {
            enemyPosition.y = popupHeight;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(enemyPosition);

            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = m_gameManager.CoinPool.GetItem();
                if (coin != null)
                {
                    yield return new WaitForSeconds(0.075f);
                    coin.SetActive(true);
                    coin.transform.position = screenPosition;
                    FlyToHUD(coin);
                }
            }
        }
        private void DisablePowerupTxt()
        {
            powerupTxt.gameObject.SetActive(false);
        }
        private void FlyToHUD(GameObject coin)
        {
            RectTransform coinRectTransform = coin.GetComponent<RectTransform>();

            coinRectTransform.DOMove(hudCoinTransform.position, flyDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    coin.SetActive(false);
                    m_gameManager.CoinPool.ReturnItem(coin);
                });
        }
    }
}
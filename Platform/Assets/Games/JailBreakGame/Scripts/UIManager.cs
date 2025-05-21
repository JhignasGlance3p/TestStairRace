using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

namespace nostra.PKPL.JailBreakGame
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;

        public TextMeshProUGUI roundText, gameStartRoundText;
        public Image panelBG;
        public List<TextMeshProUGUI> countdownTexts; // 3,2,1,Go

        public List<TextMeshProUGUI> playerScoreTexts;
        public List<Image> playerCompletedImages;
        public PopAnimator playerWon, playerLost;

        public Button playLocalButton;

        private void Start()
        {
            //playLocalButton.onClick.AddListener(OnPlayLocalClicked);
            //HideHUD();
        }
        public void OnStart()
        {
            OnPlayLocalClicked();
            HideHUD();
        }
        private void OnPlayLocalClicked()
        {
            gameManager.StartOfflineGame();
            playLocalButton.gameObject.SetActive(false);
        }

        public IEnumerator UpdateRoundText(int currentRound, int totalRounds)
        {
            roundText.text = $"Round {currentRound}/{totalRounds}";
            gameStartRoundText.text = $"Round {currentRound}";
            panelBG.gameObject.SetActive(true);
            panelBG.color = new Color(panelBG.color.r, panelBG.color.g, panelBG.color.b, 0);
            panelBG.DOFade(1, .25f);
            gameStartRoundText.transform.parent.gameObject.SetActive(true);
            gameStartRoundText.GetComponentInParent<Image>().DOFade(1, .25f);
            yield return new WaitForSeconds(2f);
            gameStartRoundText.GetComponentInParent<Image>().DOFade(0, .25f).OnComplete(() => gameStartRoundText.transform.parent.gameObject.SetActive(false));
            panelBG.DOFade(0, .25f).OnComplete(() => panelBG.gameObject.SetActive(false));
            yield return new WaitForSeconds(.35f);
        }

        public void ShowHUD()
        {
            StartCoroutine(ShowHUDCoroutine());
        }

        IEnumerator ShowHUDCoroutine()
        {
            roundText.transform.parent.gameObject.SetActive(true);
            Image image = roundText.GetComponentInParent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            image.DOFade(1, .25f);
            yield return new WaitForSeconds(.25f);
            foreach (var playerScoreText in playerScoreTexts)
            {
                playerScoreText.transform.parent.gameObject.SetActive(true);
                Image playerImage = playerScoreText.GetComponentInParent<Image>();
                playerImage.color = new Color(playerImage.color.r, playerImage.color.g, playerImage.color.b, 0);
                playerImage.DOFade(1, .25f);
            }
        }

        public void HideHUD()
        {
            StartCoroutine(HideHUDCoroutine());
        }

        IEnumerator HideHUDCoroutine()
        {
            foreach (var playerCompletedImage in playerCompletedImages)
            {
                playerCompletedImage.transform.localScale = Vector3.zero;
                playerCompletedImage.gameObject.SetActive(false);
            }
            roundText.GetComponentInParent<Image>().DOFade(0, .25f);
            yield return new WaitForSeconds(.25f);
            roundText.transform.parent.gameObject.SetActive(false);
            foreach (var playerScoreText in playerScoreTexts)
            {
                playerScoreText.GetComponentInParent<Image>().DOFade(0, .25f);
                yield return new WaitForSeconds(.25f);
                playerScoreText.transform.parent.gameObject.SetActive(false);
            }
        }

        public IEnumerator ShowCountdown()
        {
            panelBG.gameObject.SetActive(true);
            panelBG.color = new Color(panelBG.color.r, panelBG.color.g, panelBG.color.b, 0);
            panelBG.DOFade(1, .25f);
            for (int i = countdownTexts.Count - 1; i >= 0; i--)
            {
                foreach (var text in countdownTexts)
                {
                    text.gameObject.SetActive(false);
                }
                countdownTexts[i].gameObject.SetActive(true);
                countdownTexts[i].transform.localScale = Vector3.zero;
                countdownTexts[i].transform.DOScale(1, .25f);
                gameManager.m_audioManager.PlayClip(i == 0 ? gameManager.m_audioManager.countdownStart : gameManager.m_audioManager.countdown);
                if (i == 1)
                {
                    gameManager.m_audioManager.PlayGameMusic();
                }
                yield return new WaitForSeconds(1f);
                var cText = countdownTexts[i];
                cText.transform.DOScale(0, .25f).OnComplete(() => cText.gameObject.SetActive(false));
            }
            panelBG.DOFade(0, .25f).OnComplete(() => panelBG.gameObject.SetActive(false));
        }

        public void UpdatePlayerScores(Dictionary<PlayerController, int> playerScores)
        {
            int index = 0;
            foreach (var playerScore in playerScores)
            {
                if (index < playerScoreTexts.Count)
                {
                    playerScoreTexts[index].text = $"{playerScore.Value}";
                    Debug.LogError("Update Player Score :::" + index + ":::" + playerScore.Value);
                }
                index++;
            }
        }

        public void ShowResultScreen(Dictionary<PlayerController, int> playerScores)
        {
            if(gameManager.OnWatch)
            {
                return;
            }
            var highestScorePlayer = gameManager.finishOrder[0];
            gameManager.winnerID = highestScorePlayer._Id;
            playerWon.gameObject.SetActive(true);
            playerLost.gameObject.SetActive(false);
            /*if (highestScorePlayer.isAI)
            {
                playerWon.gameObject.SetActive(false);
                playerLost.gameObject.SetActive(true);
            }
            else
            {
                playerWon.gameObject.SetActive(true);
                playerLost.gameObject.SetActive(false);
            }*/
        }

        public void HideResultScreen()
        {
            playerWon.Close();
            playerLost.Close();
        }
    }
}

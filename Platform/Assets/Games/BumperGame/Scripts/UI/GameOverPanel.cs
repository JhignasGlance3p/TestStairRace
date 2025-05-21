using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace com.nampstudios.bumper.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        private const string timeUpTxt = "Time's Up!";
        private const string loseTxt = "You Lost!";

        [SerializeField] private Image bg;
        [SerializeField] private Sprite winBg;
        [SerializeField] private Sprite loseBg;
        [SerializeField] private TextMeshProUGUI titleTxt;
        [SerializeField] private Color winColor;
        [SerializeField] private Color loseColor;
        [SerializeField] private TextMeshProUGUI scoretxt;

        private System.Action restartCB;

        public void Initialize(bool isWinner, System.Action restartBtnAction)
        {
            // restartCB = restartBtnAction;
            // bg.sprite = isWinner ? winBg : loseBg;
            // titleTxt.text = isWinner ? timeUpTxt : loseTxt;
            // titleTxt.color = isWinner ? winColor : loseColor;
            // scoretxt.text = GameManager.Instance.Player_Score.ToString();
        }

        public void OnRestartBtnClicked()
        {
            // if (restartCB != null)
            //     restartCB?.Invoke();
        }
    }
}

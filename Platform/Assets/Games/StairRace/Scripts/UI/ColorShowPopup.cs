using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class ColorShowPopup : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI descriptionTxt;
        [SerializeField] Animator iconAnimator, popupAnimator;
        [SerializeField] int colorSwitchCount;
        [SerializeField] float timeBtwEachSwitch;
        [SerializeField] string switchColorTxt;
        [SerializeField] string showSelectedColorTxt;
        [SerializeField] float waitTimeBeforeStartGame;

        ColorName playerColor;
        Action<bool> onClose;
        private readonly string[] states = { GameConstants.BLUE, GameConstants.RED, GameConstants.YELLOW, GameConstants.VIOLET };

        void OnEnable()
        {
            StartCoroutine(RandomizeColor());
        }

        public void StartShow(ColorName _playerColor, Action<bool> _onClose)
        {
            playerColor = _playerColor;
            onClose = _onClose;
            this.gameObject.transform.localScale = Vector3.one;
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            descriptionTxt.text = switchColorTxt;
            this.gameObject.SetActive(true);
        }
        public void ClosePopup()
        {
            this.gameObject.SetActive(false);
            this.gameObject.transform.localScale = Vector3.one;
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            onClose?.Invoke(false);
        }

        private IEnumerator RandomizeColor()
        {
            int count = 0, index = 0;
            while (count < colorSwitchCount)
            {
                iconAnimator.Play(states[index++]);
                if (index == states.Length)
                    index = 0;
                count++;
                yield return new WaitForSeconds(timeBtwEachSwitch);
            }

            descriptionTxt.text = showSelectedColorTxt;
            switch (playerColor)
            {
                case ColorName.Blue:
                    iconAnimator.Play(GameConstants.BLUE);
                    break;
                case ColorName.Red:
                    iconAnimator.Play(GameConstants.RED);
                    break;
                case ColorName.Yellow:
                    iconAnimator.Play(GameConstants.YELLOW);
                    break;
                case ColorName.Violet:
                default:
                    iconAnimator.Play(GameConstants.VIOLET);
                    break;
            }
            yield return new WaitForSeconds(waitTimeBeforeStartGame);
            popupAnimator.Play(GameConstants.CLOSE);
        }
    }
}
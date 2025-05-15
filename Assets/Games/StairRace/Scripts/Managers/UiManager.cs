using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace nostra.pkpl.stairrace
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField] Image tileNameImage;
        [SerializeField] Animator tileIconAnimator;
        [SerializeField] GameObject tileIcon;
        [SerializeField] TileNameImage[] tileNameImages;

        public void SetPlayerColor(ColorName colorName)
        {
            tileIcon.SetActive(true);
            switch (colorName)
            {
                case ColorName.Blue:
                    tileIconAnimator.Play(GameConstants.BLUE);
                    break;
                case ColorName.Red:
                    tileIconAnimator.Play(GameConstants.RED);
                    break;
                case ColorName.Yellow:
                    tileIconAnimator.Play(GameConstants.YELLOW);
                    break;
                case ColorName.Violet:
                    tileIconAnimator.Play(GameConstants.VIOLET);
                    break;
            }

            var tileNameData = Array.Find(tileNameImages, item => item.Color == colorName);
            if (tileNameData != null)
            {
                tileNameImage.sprite = tileNameData.TileNameSprite;
            }
        }
    }

    [Serializable]
    public class TileNameImage
    {
        public ColorName Color;
        public Sprite TileNameSprite;
    }
}
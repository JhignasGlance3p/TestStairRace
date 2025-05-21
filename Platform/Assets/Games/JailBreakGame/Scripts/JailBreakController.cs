using nostra.character;
using nostra.core.games;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nostra.PKPL.JailBreakGame
{
    public class JailBreakController : GamesController
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject gameCanvas;
        [SerializeField] private int characterCount;

        public int m_count => characterCount;

        private NostraCharacter[] Characters = null;

        protected override void OnCardStateChanged(CardState _cardState)
        {
            switch (_cardState)
            {
                case CardState.LOADED:
                    gameCanvas.SetActive(false);
                    gameManager.OnLoaded();
                    break;
                case CardState.FOCUSED:
                    gameCanvas.SetActive(false);
                    gameManager.OnFocussed();
                    PlayerCharacterCustomise(Characters[0]);
                    break;
                case CardState.START:
                    gameCanvas.SetActive(true);
                    gameManager.OnStart();
                    break;
                case CardState.PAUSE:
                    gameCanvas.SetActive(false);
                    PlayerCharacterCustomise(Characters[0]);
                    gameManager.OnPause();
                    break;
                case CardState.RESTART:
                    gameCanvas.SetActive(true);
                    PlayerCharacterCustomise(Characters[0]);
                    gameManager.onRestart();
                    break;
                case CardState.REDIRECT:
                    gameCanvas.SetActive(true);
                    PlayerCharacterCustomise(Characters[0]);
                    gameManager.OnStart();
                    break;
                 case CardState.GAMEOVER_WATCH:
                        break;
            }
        }
        public NostraCharacter[] GetCharacters(int _count)
        {
            Characters = GetGameCharacters(_count);
            for (int i = 0; i < Characters.Length; i++)
            {
                if (i == 0)
                {
                    PlayerCharacterCustomise(Characters[i]);
                }
                else
                {
                    PlayerRandomCustomisation(Characters[i]);
                }
            }
            return Characters;
        }
        public string GetReplayStorePath()
        {
            return string.Empty;
        }
    }
}

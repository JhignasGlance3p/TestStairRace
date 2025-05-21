using nostra.character;
using nostra.core.games;
using System.Collections;
using UnityEngine;

namespace nostra.origami.stumble
{
    public class StumbleGuyController : GamesController
    {
        const int MAX_CHARACTERS = 32;
        [SerializeField] LHS_CountdownController m_gameManager;
        [SerializeField] GameObject m_gameCanvas;

        private NostraCharacter[] Characters = null;

        private CardState m_cardState;
        public CardState CardState => m_cardState;

        protected override void OnCardStateChanged(CardState _cardState)
        {
            m_cardState = _cardState;
            switch (_cardState)
            {
                case CardState.LOADED:
                    m_gameCanvas.SetActive(false);
                    m_gameManager.OnLoaded();
                    break;
                case CardState.FOCUSED:
                    GetCharacters(MAX_CHARACTERS);
                    m_gameCanvas.SetActive(false);
                    m_gameManager.OnFocussed();
                    break;
                case CardState.START:
                    m_gameCanvas.SetActive(true);
                    // m_gameManager.OnStart();
                    break;
                case CardState.PAUSE:
                    m_gameCanvas.SetActive(false);
                    // m_gameManager.OnPause();
                    break;
                case CardState.RESTART:
                    m_gameCanvas.SetActive(true);
                    // m_gameManager.onRestart();
                    break;
                case CardState.REDIRECT:
                    GetCharacters(MAX_CHARACTERS);
                    m_gameCanvas.SetActive(true);
                    // m_gameManager.OnStart();
                    break;
                case CardState.HIDDEN:
                    m_gameCanvas.SetActive(false);
                    break;
                case CardState.GAMEOVER_WATCH:
                    m_gameCanvas.SetActive(true);
                    break;
            }
        }
        private NostraCharacter[] GetCharacters(int _count)
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

        public NostraCharacter GetCharacter(int _index)
        {
            if (_index < 0 || _index >= Characters.Length)
            {
                return null;
            }
            return Characters[_index];
        }

        public void OnGameInstantiated()
        {
            // gameManager.Setup ( gameData );
        }
        public void OnGameToggleFocused(bool isInFocus)
        {
            // gameManager.ToggleAI ( isInFocus );
        }
        public void OnGameStart()
        {
            // GameCanvas.SetActive ( true );
            // camera2D.gameObject.SetActive ( false );
            // cameraGame.gameObject.SetActive ( true );
            // gameManager.StartGame ( );
        }
        public void OnGameTogglePause(bool isPaused)
        {
            // GameCanvas.SetActive ( !isPaused );
            // //gameManager.GamePaused = isPaused;
            // camera2D.gameObject.SetActive ( isPaused );
            // cameraGame.gameObject.SetActive ( !isPaused );
        }
        // public void Customize ( PlayerCustomizeMeta playerData )
        // {
        //     //gameManager.LoadData ( playerData );
        // }
        public void ChangeCamera(int index)
        {
        }
        public void OnToggleHide(bool shouldShow)
        {
            this.gameObject.SetActive(shouldShow);
        }
        public void OnGameDestroyed()
        {
            Destroy(this.gameObject);
        }
    }
}
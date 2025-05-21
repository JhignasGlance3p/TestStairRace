using nostra.character;
using nostra.core.games;
using nostra.core.Post;
using nostra.quickplay.core.Recorder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.sarvottam.colorclash
{
    public class ColorClashHandler : GamesController
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject gameCanvas;

        private NostraCharacter[] Characters = null;
        private CardState m_cardState;
        public CardState CardState => m_cardState;

        protected override void OnCardStateChanged(CardState _cardState)
        {
            m_cardState = _cardState;
            switch (_cardState)
            {
                case CardState.LOADED:
                    gameCanvas.SetActive(false);
                    gameManager.OnLoaded();
                    break;
                case CardState.FOCUSED:
                    gameCanvas.SetActive(false);
                    gameManager.OnPreFocus();
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        gameManager.OnFocussed();
                    }
                    PlayerCharacterCustomise(Characters[0]);
                    break;
                case CardState.START:
                    gameCanvas.SetActive(true);
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        gameManager.OnStart();
                    }
                    break;
                case CardState.PAUSE:
                    gameCanvas.SetActive(false);
                    PlayerCharacterCustomise(Characters[0]);
                    gameManager.OnPause();
                    break;
                case CardState.RESTART:
                    gameCanvas.SetActive(true);
                    PlayerCharacterCustomise(Characters[0]);
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        gameManager.onRestart();
                    }
                    break;
                case CardState.REDIRECT:
                    gameCanvas.SetActive(true);
                    PlayerCharacterCustomise(Characters[0]);
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        gameManager.OnStart();
                    }
                    break;
                case CardState.GAMEOVER_WATCH:
                    gameCanvas.SetActive(true);
                    break;
            }
        }
        public NostraCharacter[] GetCharacters(int _count)
        {
            Characters = GetGameCharacters(_count);
            for (int i = 0; i < Characters.Length; i++)
            {
                if(i == 0)
                {
                    PlayerCharacterCustomise(Characters[i]);
                } else
                {
                    PlayerRandomCustomisation(Characters[i]);
                }
            }
            return Characters;
        }
        public void RegisterTrackable(ITrackable _trackable)
        {
            base.RegisterTrackable(_trackable);
        }
        public void StartRecording()
        {
            base.StartRecording();
        }
        public void StopRecording()
        {
            base.StopRecording();
        }
        public void WriteAction()
        {
            base.WriteAction();
        }
         protected override void OnReplayStart()
        {
            base.OnReplayStart();
            gameManager.OnReplayStart();
        }
        protected override void OnReplayEnd()
        {
            base.OnReplayEnd();
            gameManager.OnReplayEnd();
        }
        protected override void OnReplayTimeUpdate(float _time)
        {
            base.OnReplayTimeUpdate(_time);
            gameManager.UpdateGameTimerOnSimulate(_time);
        }
        public override void OnChangeCameraSelected(int _cameraIndex)
        {
            base.OnChangeCameraSelected(_cameraIndex);
            gameManager.SetCameraMode(_cameraIndex);
        }
    }
}

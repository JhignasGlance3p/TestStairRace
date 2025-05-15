using nostra.character;
using nostra.core.games;
using nostra.core.Post;
using nostra.quickplay.core.Recorder;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class StairRaceController : GamesController
    {
        [SerializeField] StairRaceManager m_gameManager;
        [SerializeField] GameObject m_gameCanvas;
        [SerializeField] int m_requiredPlayers;

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
                    GetCharacters(m_requiredPlayers);
                    m_gameCanvas.SetActive(false);
                    m_gameManager.OnPrefocus();
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        m_gameManager.OnFocussed();
                    }
                    break;
                case CardState.START:
                    m_gameCanvas.SetActive(true);
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        m_gameManager.OnStart();
                    }
                    break;
                case CardState.PAUSE:
                    m_gameCanvas.SetActive(false);
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        m_gameManager.OnPause();
                    }
                    break;
                case CardState.RESTART:
                    m_gameCanvas.SetActive(true);
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        m_gameManager.onRestart();
                    }
                    break;
                case CardState.REDIRECT:
                    GetCharacters(m_requiredPlayers);
                    m_gameCanvas.SetActive(true);
                    if(post.post_type == PostConstants.POST_TYPE_GAME)
                    {
                        m_gameManager.OnStart();
                    }
                    break;
                case CardState.HIDDEN:
                    m_gameCanvas.SetActive(false);
                    break;
            }
        }
        private NostraCharacter[] GetCharacters(int _count)
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

        public NostraCharacter GetCharacter(int _index)
        {
            if(_index < 0 || _index >= Characters.Length)
            {
                return null;
            }
            return Characters[_index];
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
            m_gameManager.OnReplayStart();
        }
        protected override void OnReplayEnd()
        {
            base.OnReplayEnd();
        }
        protected override void OnReplayTimeUpdate(float _time)
        {
            base.OnReplayTimeUpdate(_time);
        }
        public override void OnChangeCameraSelected(int _cameraIndex)
        {
            base.OnChangeCameraSelected(_cameraIndex);
            m_gameManager.ChangeCamera(_cameraIndex);
        }
    }
}
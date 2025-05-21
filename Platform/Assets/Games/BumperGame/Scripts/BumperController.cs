using com.nampstudios.bumper;
using com.nampstudios.bumper.CameraUnit;
using nostra.character;
using nostra.core.games;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.nampstudios.bumper
{
    public class BumperController : GamesController
    {
        [SerializeField] GameManager m_gameManager;
        [SerializeField] GameObject m_gameCanvas;
        [SerializeField] CameraManager camManager;
        [SerializeField] int m_requiredPlayers;

        private NostraCharacter[] Characters = null;
        private CardState m_cardState;
        public CardState CardState => m_cardState;

        Dictionary<string, Queue<GameObject>> queueObj = new();
        Dictionary<string, Queue<GameObject>> pooledObj = new();

        protected override void OnCardStateChanged(CardState _cardState)
        {
            m_cardState = _cardState;
            switch (_cardState)
            {
                case CardState.LOADED:
                    m_gameCanvas.SetActive(false);
                    // m_gameManager.OnLoaded();
                    break;
                case CardState.FOCUSED:
                    GetCharacters(m_requiredPlayers);
                    m_gameCanvas.SetActive(false);
                    // m_gameManager.OnFocussed();
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
                    GetCharacters(m_requiredPlayers);
                    m_gameCanvas.SetActive(true);
                    // m_gameManager.OnStart();
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
        // public void OnRenderingCompleted()
        // {
        //     CallbackEventBase cb = new CallbackEventBase();
        //     cb.cEvent = CallbackEvent.RENDERED;
        //     if (PlatformCardState == PostCardState.RENDER)
        //     {
        //         this.gameObject.SetActive(true);
        //         m_callback?.Invoke(cb);
        //     }
        //     else if (PlatformCardState == PostCardState.FOCUSSED)
        //     {
        //         this.gameObject.SetActive(true);
        //         m_callback?.Invoke(cb);
        //         this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.FOCUSSED, this.CurrentPost);
        //     }
        //     else if (PlatformCardState == PostCardState.START)
        //     {
        //         this.gameObject.SetActive(true);
        //         m_callback?.Invoke(cb);
        //         this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.START, this.CurrentPost);
        //     }
        //     else if (PlatformCardState == PostCardState.HIDDEN)
        //     {
        //         this.gameObject.SetActive(false);
        //         this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.HIDDEN, this.CurrentPost);
        //     }
        // }
        // public void ToggleGameViewOnGameScreen<T>(T _isOn)
        // {
        //     CallbackEventObject<T> cb = new CallbackEventObject<T>();
        //     cb.cEvent = CallbackEvent.TOGGLE_GAME_VIEW;
        //     cb.data = _isOn;
        //     this.m_callback?.Invoke(cb);
        // }
        // public void GameOver<T>(T lb)
        // {
        //     cameraGame.targetTexture = this.m_2dTexture;
        //     GameCanvas.SetActive(false);
        //     CallbackEventObject<T> cb = new CallbackEventObject<T>();
        //     cb.cEvent = CallbackEvent.GAMEOVER;
        //     cb.data = lb;
        //     this.m_callback?.Invoke(cb);
        // }
        // private void HandleState()
        // {
        //     switch (PlatformCardState)
        //     {
        //         case PostCardState.RENDER:

        //             OnInitRender();
        //             break;
        //         case PostCardState.RENDER1:

        //             this.cameraGame.targetTexture = m_2dTexture;
        //             // RenderTexture.active = currentActiveRT;
        //             break;
        //         case PostCardState.FOCUSSED:

        //             OnInitAutoPlay();
        //             break;

        //         case PostCardState.START:
        //         case PostCardState.CONTINUE:
        //         case PostCardState.WATCH:

        //             OnStart();
        //             break;
        //         case PostCardState.RESTART:

        //             OnRestart();
        //             break;
        //         case PostCardState.PAUSE:

        //             OnPause();
        //             break;

        //         case PostCardState.HIDDEN:

        //             OnHidden();
        //             break;

        //         case PostCardState.NONE:
        //         default:

        //             break;
        //     }
        // }
        // private void OnGameInstantiated()
        // {
        //     if (m_initialised == true)
        //     {
        //         if (m_assetsLoaded == true)
        //         {
        //             CallbackEventBase cb = new CallbackEventBase();
        //             cb.cEvent = CallbackEvent.INSTANTIATED;
        //             this.m_callback?.Invoke(cb);
        //         }
        //         return;
        //     }

        //     m_initialised = true;
        //     GameCanvas.SetActive(false);
        //     m_currentDependencyIndex = 0;
        //     LoadDependencies();
        // }
        // private void OnInitRender()
        // {
        //     this.cameraGame.targetTexture = m_2dTexture;
        //     if (m_assetsLoaded == false)
        //     {
        //         CallbackEventBase cb = new CallbackEventBase();
        //         cb.cEvent = CallbackEvent.ASSETLOADING;
        //         this.m_callback?.Invoke(cb);
        //         return;
        //     }
        //     if (m_assetsLoaded == true && PlatformCardState != PostCardState.HIDDEN)
        //     {
        //         this.gameObject.SetActive(true);
        //         gameManager.Construct();
        //     }
        // }
        // private void OnInitAutoPlay()
        // {
        //     if (m_assetsLoaded == true && PlatformCardState != PostCardState.HIDDEN)
        //     {
        //         bgm_audio.Play();
        //     }
        // }
        // private void OnStart()
        // {
        //     if (m_assetsLoaded == false)
        //     {
        //         return;
        //     }
        //     cameraGame.targetTexture = null;
        //     GameCanvas.SetActive(true);
        // }
        // private void OnPause()
        // {
        //     if (m_assetsLoaded == false)
        //     {
        //         return;
        //     }
        //     cameraGame.targetTexture = this.m_2dTexture;
        //     GameCanvas.SetActive(false);
        // }
        // private void OnRestart()
        // {
        //     this.cameraGame.targetTexture = null;
        //     GameCanvas.SetActive(true);
        // }
        // private void OnHidden()
        // {
        //     if (m_assetsLoaded == false)
        //     {
        //         return;
        //     }
        //     GameCanvas.SetActive(false);
        //     cameraGame.targetTexture = null;
        //     this.gameObject.SetActive(false);
        //     bgm_audio.Stop();
        // }
        /*public void SetCallback(Action<CallbackEvent, object> _callback)
        {
            m_callback = _callback;
        }
        public void SetTexture(RenderTexture _2dTexture)
        {
            m_2dTexture = _2dTexture;
        }
        public void OnMessagingEvent(PlatformGameEvent _pEvent, PostCardState _gState, object _args)
        {
            NostraLogger.Logger.Log($"[BomberController] OnMessagingEvent!!! _pEvent: {_pEvent}, _gState:{_gState}", Color.grey);
            m_currentEvent = _pEvent;
            m_currentState = _gState;
            m_currentArgs = _args;
            if (m_currentArgs != null && m_currentArgs as Post != null)
            {
                m_currentPost = m_currentArgs as Post;
            }
            HandleEvent();
        }
        public void OnRenderingCompleted()
        {
            m_callback?.Invoke(CallbackEvent.RENDERED, null);
        }

        private void HandleEvent()
        {
            switch (m_currentEvent)
            {
                case PlatformGameEvent.INSTANTIATE:

                    OnGameInstantiated();
                    break;

                case PlatformGameEvent.UPDATE_STATE:

                    HandleState();
                    break;
            }
        }
        private void HandleState()
        {
            switch (m_currentState)
            {
                case PostCardState.RENDER:

                    OnInitRender();
                    break;

                case PostCardState.FOCUSSED:

                    OnInitRender();
                    OnInitAutoPlay();
                    break;

                case PostCardState.START:

                    cameraGame.targetTexture = null;
                    GameCanvas.SetActive(true);
                    break;

                case PostCardState.PAUSE:

                    break;

                case PostCardState.CONTINUE:

                    break;

                case PostCardState.RESTART:

                    break;

                case PostCardState.WATCH:

                    break;

                case PostCardState.HIDDEN:

                    OnHidden();
                    break;

                case PostCardState.NONE:
                default:

                    break;
            }
        }
        private void OnGameInstantiated()
        {
            if (m_initialised == true) return;

            m_initialised = true;
        }
        private void OnInitRender()
        {
            if (m_currentPost != null)
            {
                this.cameraGame.targetTexture = m_2dTexture;
            }
            this.gameObject.SetActive(true);
            this.m_callback?.Invoke(CallbackEvent.RENDERED, null);
        }
        private void OnInitAutoPlay()
        {

        }
        private void OnHidden()
        {
            this.gameObject.SetActive(false);
            GameCanvas.gameObject.SetActive(false);
            cameraGame.targetTexture = null;
        }

        /*public void OnToggleHide ( bool shouldShow )
        {
            this.gameObject.SetActive ( shouldShow );
        }
        public GameObject GetMyGO ()
        {
            return this.gameObject;
        }
        public void OnGameInstantiated ( GameMeta gameData, Action callback )
        {
            //AssetsController.Instance.SetGameLayerRecursive ( this.gameObject, gameData.layer );
            GameCanvas.SetActive ( false );
            camera2D.targetTexture = gameData.targetTexture;
            camManager.setCamera ( camera2D );
            gameManager.Setup ( gameData );
        }
        public void OnGameUpdated ( GameMeta gameData )
        {

        }

        public void OnGameDestroyed ()
        {
            Destroy ( this.gameObject );
        }

        public void OnGameStart ()
        {
            GameCanvas.SetActive ( true );
            camera2D.gameObject.SetActive ( false );
            camManager.setCamera ( cameraGame );
            cameraGame.gameObject.SetActive ( true );
            gameManager.StartGame ( );
        }

        public void OnGameToggleFocused ( bool isInFocus )
        {
            gameManager.ToggleAI ( isInFocus );
        }

        public void OnGameTogglePause ( bool isPaused )
        {
            GameCanvas.SetActive ( !isPaused );
            camera2D.gameObject.SetActive ( isPaused );
            camManager.setCamera ( camera2D );
            cameraGame.gameObject.SetActive ( !isPaused );
            gameManager.playPauseEnemyGO ( );
        }
        public void gameEnd ()
        {
            GameCanvas.SetActive ( true );
            camera2D.gameObject.SetActive ( false );
            camManager.setCamera ( cameraGame );
            cameraGame.gameObject.SetActive ( true );
            gameManager.StartGame ( );
        }
        public void ChangeCamera ( int index )
        {

        }
        public void OnGameOver ()
        {
            gameEnd ( );
        }
        public void WatchCompleted ()
        {

        }*/
    }
}
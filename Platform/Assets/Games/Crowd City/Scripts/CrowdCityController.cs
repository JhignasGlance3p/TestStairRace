using nostra.core.games;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class CrowdCityController : GamesController
    {
        [SerializeField] CrowdCityManager gameManager;
        [SerializeField] Canvas GameCanvas;

        bool m_initialised = false;

        // public void OnMessagingEvent(PlatformGameEvent _pEvent, PostCardState _gState, object _args)
        // {
        //     PlatformEvent = _pEvent;
        //     if (PlatformGameEvent.UPDATE_STATE == _pEvent)
        //         PlatformCardState = _gState;
        //     if (_args != null && _args as Post != null)
        //     {
        //         CurrentPost = _args as Post;
        //     }
        //     HandleEvent();
        // }
        // public void ChangeCamera(int index)
        // {
        //     cameraGame.gameObject.SetActive(false);
        //     frontCamera.gameObject.SetActive(false);
        //     backCamera.gameObject.SetActive(false);
        //     switch (index)
        //     {
        //         case 0:

        //             activeCamera = cameraGame;
        //             break;

        //         case 1:

        //             activeCamera = frontCamera;
        //             break;

        //         case 2:

        //             activeCamera = backCamera;
        //             break;

        //         default:
        //             activeCamera = cameraGame;
        //             break;
        //     }

        //     activeCamera.gameObject.SetActive(true);
        //     activeCamera.gameObject.GetComponent<Cam>().enabled = true;
        //     activeCamera.gameObject.GetComponent<SmoothCameraFollow>().enabled = true;
        // }
        // public void OnRenderingCompleted()
        // {
        //     CallbackEventBase cb = new CallbackEventBase();
        //     cb.cEvent = CallbackEvent.RENDERED;
        //     if (PlatformCardState == PostCardState.RENDER)
        //     {
        //         this.gameObject.SetActive(true);
        //         this.m_callback?.Invoke(cb);
        //     }
        //     else if (PlatformCardState == PostCardState.FOCUSSED)
        //     {
        //         this.gameObject.SetActive(true);
        //         this.m_callback?.Invoke(cb);
        //         this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.FOCUSSED, this.CurrentPost);
        //     }
        //     else if (PlatformCardState == PostCardState.START)
        //     {
        //         this.gameObject.SetActive(true);
        //         this.m_callback?.Invoke(cb);
        //         this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.START, this.CurrentPost);
        //     }
        //     else if (PlatformCardState == PostCardState.HIDDEN)
        //     {
        //         this.gameObject.SetActive(false);
        //         this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.HIDDEN, this.CurrentPost);
        //     }
        // }
        // public GameObject GetDependentAsset(string address)
        // {
        //     if (queueObj.ContainsKey(address))
        //     {
        //         if (queueObj[address].Count > 0)
        //         {
        //             GameObject go = queueObj[address].Dequeue();
        //             pooledObj[address].Enqueue(go);
        //             return go;
        //         }
        //     }

        //     return null;
        // }
        // public void GameOver<T>(T _gameOverObj)
        // {
        //     PlatformCardState = PostCardState.REPLAY;
        //     activeCamera.targetTexture = this.m_2dTexture;
        //     GameCanvas.gameObject.SetActive(false);
        //     var cb = new CallbackEventObject<T>();
        //     cb.cEvent = CallbackEvent.GAMEOVER;
        //     cb.data = _gameOverObj;
        //     this.m_callback?.Invoke(cb);
        // }
        // private void HandleEvent()
        // {
        //     switch (PlatformEvent)
        //     {
        //         case PlatformGameEvent.INSTANTIATE:
        //             SetMainCamera();
        //             OnGameInstantiated();
        //             break;

        //         case PlatformGameEvent.UPDATE_STATE:

        //             HandleState();
        //             break;
        //     }
        // }
        // private void HandleState()
        // {
        //     switch (PlatformCardState)
        //     {
        //         case PostCardState.RENDER:
        //             SetMainCamera();
        //             OnInitRender();
        //             break;
        //         case PostCardState.RENDER1:

        //             activeCamera.targetTexture = m_2dTexture;
        //             GameCanvas.gameObject.SetActive(false);
        //             // RenderTexture.active = currentActiveRT;
        //             break;
        //         case PostCardState.FOCUSSED:

        //             SetMainCamera();
        //             OnInitAutoPlay();
        //             break;

        //         case PostCardState.START:
        //         case PostCardState.CONTINUE:
        //         case PostCardState.WATCH:

        //             SetMainCamera();
        //             OnStart();
        //             break;
        //         case PostCardState.RESTART:

        //             SetMainCamera();
        //             OnRestart();
        //             break;
        //         case PostCardState.PAUSE:

        //             SetMainCamera();
        //             OnPause();
        //             break;

        //         case PostCardState.HIDDEN:

        //             SetMainCamera();
        //             OnHidden();
        //             break;

        //         case PostCardState.HIDE_FOR_OTHER:

        //             OnHideFoOther();
        //             break;

        //         case PostCardState.REPLAY:

        //             OnWatchReplay();
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
        //     GameCanvas.gameObject.SetActive(false);
        //     m_currentDependencyIndex = 0;
        //     LoadDependencies();
        // }
        // private void LoadDependencies()
        // {
        //     if (m_currentDependencyIndex < this.CurrentPost.game.addressDependencies.Count)
        //     {
        //         string address = this.CurrentPost.game.addressDependencies[m_currentDependencyIndex];
        //         if (queueObj.ContainsKey(address) == false)
        //         {
        //             queueObj.Add(address, new Queue<GameObject>());
        //         }
        //         if (pooledObj.ContainsKey(address) == false)
        //         {
        //             pooledObj.Add(address, new Queue<GameObject>());
        //         }
        //         AssetsController.Instance.LoadGameAssetByAddress(this.CurrentPost.game.addressDependencies[m_currentDependencyIndex], (m_assetInfo) =>
        //         {
        //             if (m_assetInfo != null && m_assetInfo.postGO != null)
        //                 m_assetInfo.postGO.transform.SetParent(this.transform);
        //             m_assetInfo.postGO.name = "base_" + address;
        //             queueObj[address].Enqueue(m_assetInfo.postGO);
        //             m_currentDependencyIndex++;
        //             LoadDependencies();
        //         });
        //     }
        //     else
        //     {
        //         m_assetsLoaded = true;
        //         gameManager.Initialise(this);
        //         if (PlatformCardState == PostCardState.RENDER)
        //         {
        //             this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.RENDER, this.CurrentPost);
        //         }
        //         else if (PlatformCardState == PostCardState.FOCUSSED)
        //         {
        //             this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.RENDER, this.CurrentPost);
        //             this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.FOCUSSED, this.CurrentPost);
        //         }
        //         else if (PlatformCardState == PostCardState.START)
        //         {
        //             this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.RENDER, this.CurrentPost);
        //             this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.START, this.CurrentPost);
        //         }
        //         else if (PlatformCardState == PostCardState.HIDDEN)
        //         {
        //             this.OnMessagingEvent(PlatformGameEvent.UPDATE_STATE, PostCardState.HIDDEN, this.CurrentPost);
        //         }
        //         CallbackEventBase cb = new CallbackEventBase();
        //         cb.cEvent = CallbackEvent.INSTANTIATED;
        //         this.m_callback?.Invoke(cb);
        //     }
        // }
        // private void OnInitRender()
        // {
        //     activeCamera.targetTexture = m_2dTexture;
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
        //         /* this.activeCamera.targetTexture = null;
        //         / currentActiveRT = RenderTexture.active;
        //         / RenderTexture.active = this.m_2dTexture; */
        //         gameManager.OnFocussed();
        //         bgm_audio.Play();
        //     }
        // }
        // private void OnStart()
        // {
        //     if (m_assetsLoaded == false)
        //     {
        //         return;
        //     }
        //     activeCamera.targetTexture = null;
        //     gameManager.OnStart();
        //     GameCanvas.gameObject.SetActive(true);
        // }
        // private void OnPause()
        // {
        //     if (m_assetsLoaded == false)
        //     {
        //         return;
        //     }
        //     activeCamera.targetTexture = this.m_2dTexture;
        //     gameManager.OnPause();
        //     GameCanvas.gameObject.SetActive(false);
        // }
        // private void OnRestart()
        // {
        //     string recordPath = Path.Combine(Application.persistentDataPath, $"{CurrentPost.post_id}.txt");
        //     bool isFileExists = File.Exists(recordPath);
        //     if (isFileExists)
        //     {
        //         File.Delete(recordPath);
        //     }
        //     activeCamera.targetTexture = null;
        //     gameManager.OnRestart();
        //     GameCanvas.gameObject.SetActive(true);
        // }
        // private void OnHidden(bool isForOther = false)
        // {
        //     if (m_assetsLoaded == false)
        //     {
        //         return;
        //     }
        //     gameManager.OnHidden();
        //     GameCanvas.gameObject.SetActive(false);
        //     activeCamera.targetTexture = null;
        //     this.gameObject.SetActive(false);
        //     bgm_audio.Stop();
        //     if (isForOther == false)
        //         m_callback.Invoke(new CallbackEventBase() { cEvent = CallbackEvent.HIDDEN_COMPELETED });
        // }
        // private void OnHideFoOther()
        // {
        //     OnHidden(true);
        // }
        // private void OnWatchReplay()
        // {
        //     activeCamera.targetTexture = null;
        //     gameManager.OnWatchReplay();
        //     GameCanvas.gameObject.SetActive(true);
        // }
        // private void SetMainCamera()
        // {
        //     cameraGame.gameObject.SetActive(false);
        //     frontCamera.gameObject.SetActive(false);
        //     backCamera.gameObject.SetActive(false);
        //     activeCamera = cameraGame;
        //     activeCamera.gameObject.SetActive(true);
        // }
    }
}
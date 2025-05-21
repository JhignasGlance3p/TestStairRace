using com.nampstudios.bumper;
using com.nampstudios.bumper.Enemy;
using com.nampstudios.bumper.Game;
using com.nampstudios.bumper.Player;
using com.nampstudios.bumper.Shared;
using com.nampstudios.bumper.UI;
using com.nampstudios.bumper.Zone;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.nampstudios.bumper
{
    public class GameManager : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private Transform userParentTransform;
        [SerializeField] private Vector3 playerSpawnPos;

        [Header("Managers")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private ZoneManager zoneManager;
        [SerializeField] private UiManager uiManager;
        [SerializeField] private SceneLoadHandler sceneLoader;

        [Header("ObjectPools")]
        [SerializeField] private GenericPool<EnemyController> largeEnemyPool;
        [SerializeField] private GenericPool<EnemyController> smallEnemyPool;
        [SerializeField] private GenericPool<GameObject> coinPool;

        [SerializeField] private TrackPlayer TrackPlayerScript;

        public event Action OnGameStart;
        public event Action<bool> OnGameOver;
        public event Action OnZonesReady;

        private PlayerController player;
        private int playerScore;
        private BumperController m_controller;

        public PlayerController Player => playerPrefab;
        public int Player_Score => playerScore;
        public InputManager Input_Manager => inputManager;
        public UiManager Ui_Manager => uiManager;
        public ZoneManager Zone_Manager => zoneManager;
        public SceneLoadHandler scene_Loader => sceneLoader;
        public bool IsPlayerIdle => inputManager.IsPlayerIdle;
        public GenericPool<EnemyController> LargeEnemyPool => largeEnemyPool;
        public GenericPool<EnemyController> SmallEnemyPool => smallEnemyPool;
        public GenericPool<GameObject> CoinPool => coinPool;
        public BumperController Controller => m_controller;

        public bool isGamePaused;
        public GameObject PlayerGO;
        public bool isAutoPlay = false;

        public int zone_PowerUp = -1;
        public int zone_sEnemy = -1;
        public int zone_lEnemy = -1;

        public List<hOrderIndex> savedZoneOrder = null;

        public void Initialise(BumperController _controller)
        {
            m_controller = _controller;
            zoneManager.Initialise(this);
            uiManager.Initialise(this);
        }
        public void Construct()
        {
        }
        public void OnFocussed()
        {
        }
        public void OnStart()
        {
        }
        public void OnPause()
        {
        }
        public void OnRestart()
        {
        }
        public void OnHidden()
        {
        }

        private void Reset()
        {
            playerScore = 0;
        }

        public void setPrefabGO(GameObject GO, GameObject parentGO)
        {
            PlayerGO = GO;
            StartCoroutine("InstantiatePrefab");
            zoneManager.ParentGO = parentGO;
            // zoneManager.SpawnInitialZones();
        }
        public void TriggerGameStart()
        {
            OnGameStart?.Invoke();
            inputManager.SetGameRunning(true);
        }
        public void TriggerOnZonesReady()
        {
            OnZonesReady?.Invoke();
            playerScore = 0;
            uiManager.UpdateScore(playerScore);
            //player = Instantiate(playerPrefab,  playerSpawnPos, Quaternion.identity,playerParent);
        }
        System.Collections.IEnumerator InstantiatePrefab()
        {
            player = playerPrefab;
            //player = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity, playerParent);
            yield return new WaitForEndOfFrame();
            GameObject user = Instantiate(PlayerGO, playerSpawnPos, Quaternion.identity, player.transform);
            user.transform.localScale = Vector3.one * 0.3f;
            user.transform.localPosition = new Vector3(0, 1.7f, -0.25f);
            user.SetActive(true);
            yield return new WaitForEndOfFrame();
            //camManager.setCamera(activeCamera);
            playerPrefab.gameOver = false;
            playerPrefab.setAnimator(user);
            TrackPlayerScript.Init(this);
        }
        public void stopTrackPlayer()
        {
            playerPrefab.gameOver = true;
            TrackPlayerScript.onGameOver();
        }
        public void stopCoroutine()
        {
            if (playerPrefab.coroutine != null)
            {
                StopCoroutine(playerPrefab.coroutine);
            }
        }
        public void TriggerGameOver(bool winner)
        {
            scene_Loader.gameOver();
            scene_Loader.playPauseEnemyGO();
            inputManager.SetGameRunning(false);
            uiManager.OnGameOver(winner);
            /*if ( player != null )
                Destroy ( player.gameObject );*/
        }
        public void OnEnemyDeath(int scoreForKill)
        {
            playerScore += scoreForKill;
            uiManager.UpdateScore(playerScore);
        }
        public void RestartScene()
        {
            playerPrefab.gameOver = false;
            sceneLoader.getPlayerData();
        }
        private void SetupLayerCollisions()
        {
            Physics.IgnoreLayerCollision(GameConstants.GROUND_LAYER, GameConstants.WEAPON_LAYER);
            Physics.IgnoreLayerCollision(GameConstants.WEAPON_LAYER, GameConstants.WEAPON_LAYER);
            Physics.IgnoreLayerCollision(GameConstants.WEAPON_LAYER, GameConstants.PLAYER_LAYER);
        }
    }
}
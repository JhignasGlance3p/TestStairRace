using com.nampstudios.bumper.Enemy;
using com.nampstudios.bumper.UI;
using com.nampstudios.bumper.Zone;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.nampstudios.bumper
{
    [Serializable]
    public class GameProgressData
    {
        public int frame;
        public PlayerProgress playerProgress;
        public int hasPushed;
        public PlayerZoneData zoneData;
        public List<hOrderIndex> OrderIndex;

        public GameProgressData(int _frame, PlayerProgress _player, List<hOrderIndex> _orderIndex, int _pushed, PlayerZoneData zone_data)
        {
            this.frame = _frame;
            this.playerProgress = _player;
            this.OrderIndex = _orderIndex;
            this.hasPushed = _pushed;
            this.zoneData = zone_data;
        }
    }
    [Serializable]
    public class PlayerProgress
    {
        public Vector3 input;
        public Vector3 rotationValue;

        public PlayerProgress(Vector3 _input, Vector3 _rotation)
        {
            input = _input;
            rotationValue = _rotation;
        }
    }
    [Serializable]
    public class PlayerZoneData
    {
        public int zonePowerUp;
        public PlayerZoneData(int _powerup)
        {
            zonePowerUp = _powerup;
        }
    }
    [Serializable]
    public class ZoneEnemyCount
    {
        public int sEnemy;
        public int lEnemy;
        public ZoneEnemyCount(int _senemy, int _lenemy)
        {
            sEnemy = _senemy;
            lEnemy = _lenemy;
        }
    }
    [Serializable]
    public class hOrderIndex
    {
        public List<EnemyData> enemyConfig;
        public List<ZoneEnemyCount> zoneConfig;
        public hOrderIndex(List<EnemyData> _list, List<ZoneEnemyCount> _zlist)
        {
            enemyConfig = _list;
            zoneConfig = _zlist;
        }
        /*public List<EnemyCountOrderIndex> enemyZone;
        public hOrderIndex(List<EnemyCountOrderIndex> _list)
        {
            enemyZone = _list;
        }*/
    }
    [Serializable]
    public class EnemyCountOrderIndex
    {
        public List<EnemyData> enemyConfig;
        public EnemyCountOrderIndex(List<EnemyData> _list)
        {
            enemyConfig = _list;
        }
    }
    [Serializable]
    public class EnemyData
    {
        public float speed;
        public Waypoint wayPointPos;
        public Vector3 position;
        public EnemyData(float _speed, Waypoint _wayPoint, Vector3 _position)
        {
            this.speed = _speed;
            this.wayPointPos = _wayPoint;
            this.position = _position;
        }
    }
    [Serializable]
    public class ProgressList
    {
        public List<GameProgressData> progressData = new List<GameProgressData>();
    }
    public class SceneLoadHandler : MonoSingletonGeneric<SceneLoadHandler>
    {
        [SerializeField] private GameObject bumperGame;
        [SerializeField] private GameManager manager;
        [SerializeField] private GameObject nostraCanvas;
        public UiManager uiManager;
        public GameObject[] AIPrefabGO;
        public GameObject PlayerGO;
        public int currentStartFrame;
        public ProgressList progressList;
        public PlayerZoneData _zoneData;
        public EnemyCountOrderIndex _zoneEnemy;
        public int currentIndex = 0;
        public int foundIndex = 0;
        public bool IsWatch = false;

        private void PerformPostLoadActions()
        {
            manager.setPrefabGO(AIPrefabGO[0], bumperGame);
        }
        ///////////////////////
        // public void Setup(GameMeta gameData)
        // {
        //     gameMeta = gameData;
        //     if (AssetsController.Instance != null)
        //     {
        //         AIPrefabGO = new GameObject[1];
        //         GameObject customiseUI = null;// ( GameObject ) gameData.card.managedAssets.GetResourceObject ( gameData.characterMeta.address );
        //         //AssetsController.Instance.gameGOs.TryGetValue ( gameData.characterMeta.address, out customiseUI );
        //         if (customiseUI == null)
        //         {
        //             return;
        //         }
        //         AIPrefabGO[0] = customiseUI;
        //     }

        //     //PerformPostLoadActions ( );
        // }
        public void StartGame()
        {
            Debug.LogError("Start Game >>>>> ");
            //PerformPostLoadActions();
            progressList = new ProgressList();
            _zoneData = new PlayerZoneData(-1);
            progressList.progressData = new List<GameProgressData>();
            progressList.progressData.Insert(0, new GameProgressData(0, null, null, 0, null));
            //progressList.progressData[0].OrderIndex = new hOrderIndex(new List<EnemyCountOrderIndex>(new List<EnemyData>()));
            //progressList.progressData[0].OrderIndex.enemyZone.enemyConfig.Insert(0,new ZoneEnemyConfig(0.0f,null,Vector3.zero));
            manager.stopCoroutine();
            manager.isGamePaused = true;
            manager.isAutoPlay = false;
            playPauseEnemyGO();
            currentStartFrame = Time.frameCount;
            restart();
            showNostraCanvas(true);
            // uiManager.OnStartBtnClicked(); //TODO Deepak
        }
        public void showNostraCanvas(bool status)
        {
            if (status)
            {
                if (!nostraCanvas.activeInHierarchy)
                {
                    nostraCanvas.SetActive(status);
                }
            }
            else
            {
                nostraCanvas.SetActive(status);
            }
        }
        public void playPauseEnemyGO()
        {
            for (int i = 0; i < bumperGame.transform.childCount; i++)
            {
                var controller = bumperGame.transform.GetChild(i).gameObject.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.onPause(manager.isGamePaused);
                }
            }
        }
        public void restart()
        {
            manager.Zone_Manager.CleanZone();
            manager.Player.transform.position = Vector3.zero;
            manager.Player.transform.localEulerAngles = Vector3.zero;
            manager.Player.targetIndex = 1;
            // manager.Zone_Manager.SpawnInitialZones();
            foreach (Transform child in bumperGame.transform)
            {
                if (child.CompareTag("GameEnemy") && child.gameObject.activeInHierarchy)
                {
                    EnemyController EC = child.gameObject.GetComponent<EnemyController>();
                    EC.onRestart();
                }
            }
        }
        public void gameOver()
        {
            setPlayerData();
            manager.isGamePaused = true;
            showNostraCanvas(false);
            playPauseEnemyGO();
        }

        void setPlayerData()
        {
            try
            {
                var tmpFilePath = Path.Combine(Application.persistentDataPath, "gameData.json");
                //if ( gameMeta != null && gameMeta.post != null )
                //{
                //    tmpFilePath = Path.Combine ( Application.persistentDataPath, $"{gameMeta.post.post_id}_{new Guid ( )}.json" );
                //    CreatedGames game = new CreatedGames ( );
                //    game.cardType = CardType.WATCH;
                //    game.GameLink = tmpFilePath;
                //    game.GameName = $"{gameMeta.post.post_id} Watch";

                //    string savedGames = PlayerPrefs.GetString ( "savedList", string.Empty );
                //    CreatedList createdList;
                //    if ( string.IsNullOrEmpty ( savedGames ) == false )
                //    {
                //        createdList = JsonUtility.FromJson<CreatedList> ( savedGames );
                //    } else
                //    {
                //        createdList = new CreatedList ( );
                //        createdList.createdGames = new List<CreatedGames> ( );
                //    }
                //    createdList.createdGames.Add ( game );
                //    PlayerPrefs.SetString ( "savedList", JsonUtility.ToJson ( createdList ) );
                //}

                //Directory.CreateDirectory ( Path.GetDirectoryName ( tmpFilePath ) );
                //if ( File.Exists ( tmpFilePath ) )
                //{
                //    File.Delete ( tmpFilePath );
                //}
                //File.WriteAllText ( tmpFilePath, JsonUtility.ToJson ( progressList ) );
            }
            catch
            {
                Debug.LogError("Exception while saving");
            }
        }
        public void ToggleAI(bool isInFocus)
        {
            if (isInFocus)
            {
                manager.isGamePaused = isInFocus;
                manager.isAutoPlay = true;
                playPauseEnemyGO();
                PerformPostLoadActions();
            }
            else
            {
                manager.isGamePaused = isInFocus;
            }
            /*manager.isGamePaused = !isInFocus; //SingleGamePlay
            manager.isAutoPlay = true; //SingleGamePlay
            playPauseEnemyGO(); //SingleGamePlay*/
        }

        public void getPlayerData()
        {
            var tmpFilePath = Path.Combine(Application.persistentDataPath, "gameData.json");
            string savedData = File.ReadAllText(tmpFilePath);
            WatchGame(savedData);
        }
        public void WatchGame(string progressData)
        {
            currentIndex = 0;
            foundIndex = 0;
            manager.stopCoroutine();
            manager.isGamePaused = true;
            manager.isAutoPlay = false;
            IsWatch = true;
            playPauseEnemyGO();
            restart();
            showNostraCanvas(true);
            // uiManager.OnStartBtnClicked();TODO Deepak
            if (string.IsNullOrEmpty(progressData) == true)
            {
                return;
            }

            progressList = JsonUtility.FromJson<ProgressList>(progressData);
        }
    }
}
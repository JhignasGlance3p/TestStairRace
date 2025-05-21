using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using nostra.input;
using nostra.character;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using Newtonsoft.Json;
#if JAILBREAK_FUSION
using Fusion;
using Fusion.Sockets;
#endif

namespace nostra.PKPL.JailBreakGame
{
    public class GameManager :
#if JAILBREAK_FUSION 
SimulationBehaviour, INetworkRunnerCallbacks
#else
    MonoBehaviour
#endif
    {
#if JAILBREAK_FUSION7
        [Networked]
#endif
        public int currentRound { get; set; } = 0;
        [SerializeField] JailBreakController Controller;
        [SerializeField] Joystick floatingJoystick;
        private NostraCharacter[] Characters = null;
#if JAILBREAK_FUSION
        [Networked]
#endif
        public bool roundInProgress { get; set; } = false;

        [Header("Prefabs and Game Objects")]
        [SerializeField] GameObject player1Prefab;
        [SerializeField] GameObject player2Prefab;
        [SerializeField] GameObject player3Prefab;
        [SerializeField] GameObject player4Prefab;
        [SerializeField] GameObject m_keyPrefab;

        public GameObject keyPrefab => m_keyPrefab;

#if JAILBREAK_FUSION
        public NetworkPrefabRef player1NetworkPrefab, player2NetworkPrefab, player3NetworkPrefab, player4NetworkPrefab;
        public NetworkPrefabRef keyNetworkPrefab;
#endif
        [Header("Gameplay Settings")]
        List<string> playerDoorTags = new List<string> { "QP_Exit1", "QP_Exit2", "QP_Exit3", "QP_Exit4" };
        public int totalRounds = 3;
        public float roundStartDelay = 5f; // Delay between rounds
        public float matchmakingTimeout = 10f; // Timeout for matchmaking

        public List<PlayerController> players = new List<PlayerController>();
        private Dictionary<PlayerController, int> playerScores = new Dictionary<PlayerController, int>();
        private Dictionary<PlayerController, int> firstPlaceFinishes = new Dictionary<PlayerController, int>();
        [HideInInspector]
        public List<PlayerController> finishOrder = new List<PlayerController>();

#if JAILBREAK_FUSION
        public NetworkRunner networkRunner;
        private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
#endif
        public bool isOfflineMode = true;
        private int maxPlayers = 4;

        public int winnerID = 0;
        public bool isAutoPlay = false;
        bool onWatch = false;
        public bool OnWatch => onWatch;

        public bool pauseTheGame = false;

        bool isFileLoaded = false;
        [SerializeField]AudioManager audioManager;
        [SerializeField] UIManager uiManager;
        [SerializeField] WorldManager worldManager;
        public AudioManager m_audioManager => audioManager;
        public UIManager m_uiManager => uiManager;
        public WorldManager m_worldManager => worldManager;

        public List<int> randomKeyIndex = new List<int>();
        public List<int> randomLaserIndex = new List<int>();
        int CurrentFrame = 0;
        int tempFrame = 0;

        JailBreak m_jailBreak;
        GameProgress gameProgress = new();

        StreamWriter progressWrite;
        string recordPath = string.Empty;
        string[] replayContents;
       
        Coroutine m_GameLoop;

        [Serializable]
        public class JailBreak
        {
            public GameProgress gameProgress;
        }
        [Serializable]
        public class GameProgress
        {
            public int CurrentFrame;
            public int CurrentRound;
            public List<PoliceProgress> policeData;
            public List<PlayerProgress> playerData;
            public List<int> keyData;
            public List<int> laserData;
        }
        [Serializable]
        public class PlayerProgress
        {
            public bool isAi;
            public float speed;
            public Vector3 position;
            public Vector3 rotation;
            public PlayerProgress(bool ai,float _speed,Vector3 pos,Vector3 rot)
            {
                this.isAi = ai;
                this.speed = _speed;
                this.position = pos;
                this.rotation = rot;
            }
        }
        [Serializable]
        public class PoliceProgress
        {
            public Vector3 position;
            public Vector3 rotation;
            public float speed;
            public PoliceProgress(Vector3 pos, Vector3 rot, float _speed)
            {
                this.position = pos;
                this.rotation = rot;
                this.speed = _speed;
        }
        }
        //[Serializable]
        //public class Key
        //{
        //    public int placementId;
        //    public Key(int _id)
        //    {
        //        this.placementId = _id;
        //    }
        //}
        private void Awake()
        {
            //m_gameManager = this;
            //recordPath = Application.persistentDataPath + "/JailBreak.txt";
            //Init();
        }

        private void FixedUpdate()
        {
            if(isAutoPlay || (!roundInProgress))
            {
                return;
            }

            if (onWatch == false)
            {
                m_jailBreak.gameProgress = new GameProgress();
                tempFrame = Time.frameCount - CurrentFrame;
                m_jailBreak.gameProgress.CurrentFrame = tempFrame;
                m_jailBreak.gameProgress.CurrentRound = currentRound;
                m_jailBreak.gameProgress.policeData = new List<PoliceProgress>();
                m_jailBreak.gameProgress.keyData = new List<int>();
                m_jailBreak.gameProgress.laserData = new List<int>();

                foreach (var police in m_worldManager.policeControllers)
                {
                    m_jailBreak.gameProgress.policeData.Add(new PoliceProgress(police.gameObject.transform.position, police.gameObject.transform.eulerAngles,police.setSpeed()));
                }
                m_jailBreak.gameProgress.playerData = new List<PlayerProgress>();
                int i = 0;
                foreach (var player in players)
                {
                    m_jailBreak.gameProgress.playerData.Add(new((i == 0 ? false : true), player.setSpeedToTxt(), player.gameObject.transform.position, player.gameObject.transform.localEulerAngles));
                    i++;
                }
                foreach(int index in randomKeyIndex)
                {
                    m_jailBreak.gameProgress.keyData.Add(index);
                }
                foreach (int index in randomLaserIndex)
                {
                    m_jailBreak.gameProgress.laserData.Add(index);
                }
                progressWrite.Write(JsonConvert.SerializeObject(m_jailBreak.gameProgress, Formatting.None, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
                progressWrite.WriteLine();
                if(randomKeyIndex.Count > 0)
                {
                    randomKeyIndex = new List<int>();
                }
                if (randomLaserIndex.Count > 0)
                {
                    randomLaserIndex = new List<int>();
                }
            }
            else if(isFileLoaded)
            {
                StartCoroutine(_wait());
            }
        }

        IEnumerator _wait()
        {
            yield return new WaitForSeconds(0.05f);
            WatchTheGamePlay();
        }
        void WatchTheGamePlay()
        {
            if (replayContents.Length > 0 && CurrentFrame < replayContents.Length - 1)
            {
                GameProgress m_gameProgress = JsonUtility.FromJson<GameProgress>(replayContents[CurrentFrame]);
                int player_index = 0;
                int police_index = 0;
                if (randomKeyIndex.Count > 0)
                {
                    randomKeyIndex = new List<int>();
                }
                if (randomLaserIndex.Count > 0)
                {
                    randomLaserIndex = new List<int>();
                }
                if (m_gameProgress != null)
                {
                    foreach(var player_Data in m_gameProgress.playerData)
                    {
                        players[player_index].gameObject.transform.position = player_Data.position;
                        players[player_index].gameObject.transform.localEulerAngles = player_Data.rotation;
                        players[player_index].SetSpeed(player_Data.speed);
                        player_index++;
                    }

                    foreach(var police_Data in m_gameProgress.policeData)
                    {
                        m_worldManager.policeControllers[police_index].gameObject.transform.position = police_Data.position;
                        m_worldManager.policeControllers[police_index].gameObject.transform.localEulerAngles = police_Data.rotation;
                        m_worldManager.policeControllers[police_index].SetSpeedFromTxt(police_Data.speed);
                        police_index++;
                    }
                    foreach(int key_Data in m_gameProgress.keyData)
                    {
                        randomKeyIndex.Add(key_Data);
                    }
                    foreach (int laser_Data in m_gameProgress.laserData)
                    {
                        randomLaserIndex.Add(laser_Data);
                    }
                    if(randomKeyIndex.Count > 0)
                    {
                        m_worldManager.SpawnKeysOnWatch();
                    }
                    if (randomLaserIndex.Count > 0)
                    {
                        m_worldManager.TriggerLaser();
                    }
                    CurrentFrame++;
                }
            }
            else
            {
                Debug.LogError("Watch Game Ends Here >>>>>>>>>>>>");
            }
        }

        public void Reset_OnWatch()
        {
            floatingJoystick.gameObject.SetActive(false);
            if (m_GameLoop != null)
            {
                StopCoroutine(m_GameLoop);
                //m_GameLoop = StartCoroutine(GameLoop());
            }
            if (m_worldManager.policeControllers.Count <= 0)
            {
                // Spawn police
                m_worldManager.SpawnPolice();

                // Spawn doors
                m_worldManager.SpawnDoors();
            }
            else
            {
                CleanUpRound();
            }
            foreach (PlayerController player in players)
            {
                player.gameObject.SetActive(true);
            }
            m_worldManager.resetVariables();
            onWatch = true;
            isAutoPlay = false;
            //roundInProgress = false;
            roundInProgress = true;
            currentRound = 0;
            CurrentFrame = 0;
            randomKeyIndex = new List<int>();
            randomLaserIndex = new List<int>();
            if (m_worldManager.policeControllers.Count > 0)
            {
                foreach(PoliceController policeCtrl in m_worldManager.policeControllers)
                {
                    policeCtrl.stopBehavior();
                }
            }
            if (File.Exists(recordPath))
            {
                string[] contents = File.ReadAllLines(recordPath);
                if (contents.Length > 0 && CurrentFrame < contents.Length)
                {
                    replayContents = contents;
                }
                isFileLoaded = true;
            }
        }
        //private void Init()
        //{
        //    if (onWatch)
        //    {
        //        SpawnPlayersOffline();
        //        loadGameData();
        //    }
        //    else
        //    {
        //        initGame();
        //    }
        //}

        public Vector2 GetJoystickMovement()
        {
            return floatingJoystick.Direction;
        }
        public void OnLoaded()
        {
            if (m_worldManager.policeControllers.Count <= 0)
            {
                // Spawn police
                m_worldManager.SpawnPolice();

                // Spawn doors
                m_worldManager.SpawnDoors();
            }
            else
            {
                CleanUpRound();
            }
            SpawnPlayersOffline();
        }
        public void OnFocussed()
        {
            recordPath = Controller.GetReplayStorePath();
            floatingJoystick.gameObject.SetActive(false);
            Characters = Controller.GetCharacters(Controller.m_count);
            roundInProgress = true;
            for (int i = 0; i < Characters.Length; i++)
            {
                players[i].SetCharacter(Characters[i]);
            }
            OnAutoPlay();
            m_audioManager.PlayGameOverMusic();
        }
        void OnAutoPlay()
        {
            isAutoPlay = true;
            pauseTheGame = false;
            //m_uiManager.HideHUD();
            StartOfflineGame();
        }
        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    ReStartGame();
            //}
            //else if(Input.GetKeyDown(KeyCode.W))
            //{
            //    loadGameData();
            //}
            //else if (Input.GetKeyDown(KeyCode.P))
            //{
            //    PauseGame();
            //}
            //else if (Input.GetKeyDown(KeyCode.R))
            //{
            //    ResumeGame();
            //}
            //else if (Input.GetKeyDown(KeyCode.C))
            //{
            //    ContinueGame();
            //}
        }
        void ContinueGame()
        {

        }
        public void OnPause()
        {
            pauseTheGame = true;
            if (m_worldManager.policeControllers.Count > 0)
            {
                foreach (PoliceController policeCtrl in m_worldManager.policeControllers)
                {
                    policeCtrl.PauseTheGame(pauseTheGame);
                }
            }
            foreach (PlayerController player in players)
            {
                player.PauseTheGame(pauseTheGame);
            }
            OnFocussed();
        }
        public void onRestart()
        {
            if (m_GameLoop != null)
            {
                StopCoroutine(m_GameLoop);
            }
            CleanUpRound();
            OnStart();
        }
        public void ResumeGame()
        {
            pauseTheGame = false;
            if (m_worldManager.policeControllers.Count > 0)
            {
                foreach (PoliceController policeCtrl in m_worldManager.policeControllers)
                {
                    policeCtrl.PauseTheGame(pauseTheGame);
                }
            }
            foreach (PlayerController player in players)
            {
                player.PauseTheGame(pauseTheGame);
            }
        }
        public void OnStart()
        {
            if(m_GameLoop != null)
            {
                StopCoroutine(m_GameLoop);
            }
            pauseTheGame = false;
            floatingJoystick.gameObject.SetActive(true);
            m_jailBreak = new();
            m_jailBreak.gameProgress = new();
            FileStream fileStream = new FileStream(recordPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            if (progressWrite == null)
            {
                progressWrite = new StreamWriter(recordPath, true);
                progressWrite.AutoFlush = true;
            }

            randomKeyIndex = new List<int>();
            randomLaserIndex = new List<int>();
            isAutoPlay = false;
            currentRound = 0;
            loadNextRound();
        }
        public void PlayCharacterAnimation(NostraCharacter _character, string _animation, float _speed)
        {
            Controller?.PlayCharacterAnimation(_character, _animation, _speed);
        }
        public void PlayCharacterAnimation(NostraCharacter _character, string _animation, bool _play)
        {
            Controller?.PlayCharacterAnimation(_character, _animation, _play);
        }
        void loadNextRound()
        {
            m_worldManager.resetVariables();
            roundInProgress = false;
            StartOfflineGame();
        }
        public void StartOfflineGame()
        {
            isOfflineMode = true;
            //NostraJoystick.activateJoyStick(isAutoPlay);
            
            if (m_worldManager.policeControllers.Count <= 0)
            {
                // Spawn police
                m_worldManager.SpawnPolice();

                // Spawn doors
                m_worldManager.SpawnDoors();
            }
            else
            {
                m_worldManager.setDoorIntialValues();
                CleanUpRound();
            }
            if (currentRound < totalRounds)
            {
                currentRound++;
            }
            if (m_GameLoop != null)
            {
                StopCoroutine(m_GameLoop);
            }
            SpawnPlayersOffline();
            m_GameLoop = StartCoroutine(GameLoop());
        }

        public void StartOnlineGame()
        {
            isOfflineMode = false;
#if JAILBREAK_FUSION
            InitializeNetworkRunner();
#endif
        }

#if JAILBREAK_FUSION
        private async void InitializeNetworkRunner()
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
            networkRunner.ProvideInput = true;
            networkRunner.AddCallbacks(this);

            var result = await networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = "JailBreakSession",
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = maxPlayers
            });

            if (result.Ok)
            {
                if (networkRunner.IsSharedModeMasterClient)
                {
                    StartCoroutine(MatchmakingCountdown());
                }
            }
            else
            {
                Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            }
        }
        private IEnumerator MatchmakingCountdown()
        {
            float timer = matchmakingTimeout;
            while (timer > 0 && players.Count < maxPlayers)
            {
                timer -= Time.deltaTime;
                // Update UI timer if needed
                yield return null;
            }

            // Fill remaining slots with bots
            int botsToAdd = maxPlayers - players.Count;
            for (int i = 0; i < botsToAdd; i++)
            {
                SpawnBotPlayer();
            }

            StartCoroutine(GameLoop());
        }

        private void SpawnBotPlayer()
        {
            // Only the host should spawn bots
            if (networkRunner.IsSharedModeMasterClient)
            {
                int playerIndex = players.Count;
                var spawnPoints = m_worldManager.playerSpawnPoints;
                Vector3 spawnPosition = spawnPoints[playerIndex].position;

                NetworkPrefabRef aiPrefab;
                string playerDoorTag;

                switch (playerIndex)
                {
                    case 0:
                        aiPrefab = player1NetworkPrefab;
                        playerDoorTag = playerDoorTags[0];
                        break;
                    case 1:
                        aiPrefab = player2NetworkPrefab;
                        playerDoorTag = playerDoorTags[1];
                        break;
                    case 2:
                        aiPrefab = player3NetworkPrefab;
                        playerDoorTag = playerDoorTags[2];
                        break;
                    case 3:
                        aiPrefab = player4NetworkPrefab;
                        playerDoorTag = playerDoorTags[3];
                        break;
                    default:
                        aiPrefab = player1NetworkPrefab;
                        playerDoorTag = playerDoorTags[0];
                        break;
                }

                NetworkObject botNetworkObject = networkRunner.Spawn(aiPrefab, spawnPosition, Quaternion.identity);
                PlayerController botController = botNetworkObject.GetComponent<PlayerController>();
                botController.isAI = true;
                botController.playerDoorTag = playerDoorTag;

                if (!playerScores.ContainsKey(botController))
                {
                    playerScores[botController] = 0;
                    firstPlaceFinishes[botController] = 0;
                }
            }
        }
#endif

        private void SpawnPlayersOffline()
        {
            // Spawn players locally for offline mode
            isOfflineMode = true;
            //currentRound = 0;
            SpawnPlayers();
        }

        private void SpawnPlayers()
        {
            if (players.Count > 0 && isOfflineMode)
            {
                int i = 0;
                finishOrder.Clear();
                foreach (var player in players)
                {
                    if (player != null)
                    {
                        player.gameObject.transform.position = m_worldManager.playerSpawnPoints[i].position;
                        player.gameObject.transform.rotation = Quaternion.identity;
                        player.isAI = (i == 0 ? isAutoPlay : true);
                        
                        player.gameObject.SetActive(true);
                        player.showHighLight(!isAutoPlay);
                        player.hasEscaped = false;
                        player._Id = i;
                        player.playerDoorTag = playerDoorTags[i];
                        if (currentRound == 1)
                        {
                            if (!playerScores.ContainsKey(player))
                            {
                                playerScores[player] = 0;
                                firstPlaceFinishes[player] = 0;
                            }
                            m_uiManager.playerScoreTexts[i].text = "0";
                        }
                        i++;
                    }
                }
                return;
            }
            var spawnPoints = m_worldManager.playerSpawnPoints;

            // Spawn the main player or networked players
            if (isOfflineMode)
            {
                // Spawn local player
                var playerObj = Instantiate(player1Prefab, spawnPoints[0].position, Quaternion.identity, transform);
                PlayerController playerController = playerObj.GetComponent<PlayerController>();
                playerController.gameManager = this;
                playerController.isAI = isAutoPlay;
                playerController.showHighLight(!isAutoPlay);
                playerController.playerDoorTag = playerDoorTags[0];
                playerController.hasEscaped = false;
                playerController._Id = 4;
                players.Add(playerController);

                if (!playerScores.ContainsKey(playerController))
                {
                    playerScores[playerController] = 0;
                    firstPlaceFinishes[playerController] = 0;
                }

                // Spawn AI bots
                for (int i = 1; i <= 3; i++)
                {
                    GameObject aiPrefab;
                    switch (i)
                    {
                        case 1:
                            aiPrefab = player2Prefab;
                            break;
                        case 2:
                            aiPrefab = player3Prefab;
                            break;
                        case 3:
                            aiPrefab = player4Prefab;
                            break;
                        default:
                            aiPrefab = player2Prefab;
                            break;
                    }
                    var aiObj = Instantiate(aiPrefab, spawnPoints[i].position, Quaternion.identity, transform);
                    PlayerController aiController = aiObj.GetComponent<PlayerController>();
                    aiController.gameManager = this;
                    aiController.isAI = true;
                    aiController.showHighLight(!isAutoPlay);
                    aiController.playerDoorTag = playerDoorTags[i];
                    aiController.hasEscaped = false;
                    aiController._Id = (4-i);
                    players.Add(aiController);

                    if (!playerScores.ContainsKey(aiController))
                    {
                        playerScores[aiController] = 0;
                        firstPlaceFinishes[aiController] = 0;
                    }
                }
            }
            else
            {
                // Networked players are spawned in OnPlayerJoined callback
            }
        }

        private IEnumerator GameLoop()
        {
            //for (currentRound = 1; currentRound <= totalRounds; currentRound++)
            //{
            yield return StartCoroutine(RoundStart());

                // Wait until all clients are ready
                yield return new WaitUntil(() => roundInProgress);
            if (!OnWatch)
            {
                yield return StartCoroutine(RoundPlay());
            }
            
            yield return StartCoroutine(RoundEnd());
            //}
            yield return new WaitForSeconds(0.5f);
            if (currentRound != totalRounds)
            {
                loadNextRound();
            }
            else
            {
                if (progressWrite != null)
                {
                    progressWrite.Close();
                    progressWrite = null;
                }
                Reset_OnWatch();
            }
            //SceneManager.LoadScene(0);
        }


        private IEnumerator RoundStart()
        {
            // Update UI
            if (isAutoPlay)
            {
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return m_uiManager.UpdateRoundText(currentRound, totalRounds);
                yield return m_uiManager.ShowCountdown();
                m_uiManager.ShowHUD();
            }
            if (
#if JAILBREAK_FUSION
            (networkRunner != null ? networkRunner.IsSharedModeMasterClient : true) || isOfflineMode)
#else
            true
#endif
            )
            {
                // Allow players to move after countdown
                roundInProgress = true;
                if (!OnWatch)
                {
                    if (currentRound == 1)
                    {
                        CurrentFrame = Time.frameCount+1;
                    }
                    else
                    {
                        CurrentFrame = Time.frameCount - tempFrame+1;
                    }
                }
                // Only the server spawns initial keys
                m_worldManager.SpawnInitialKeys();

                foreach (var player in players)
                {
                    player.canMove = true;
                    player.gameObject.SetActive(true);
                    player.play();
                }

                foreach (var police in m_worldManager.policeControllers)
                {
                    police.Start();
                }
            }
        }

        private IEnumerator RoundPlay()
        {
            while (!roundInProgress)
            {
                // Wait until the round starts
                yield return null;
            }

            while (roundInProgress)
            {
                // Check for round end conditions
               CheckRoundEndConditions();
                yield return null;
            }
        }

        private IEnumerator ResetRoundVar()
        {
            yield return null;
            if (players[0].hasEscaped)
            {
                roundInProgress = false;
                m_worldManager.resetVariables();
                if (m_worldManager.policeControllers.Count <= 0)
                {
                    // Spawn police
                    m_worldManager.SpawnPolice();

                    // Spawn doors
                    m_worldManager.SpawnDoors();
                }
                else
                {
                    CleanUpRound();
                }
                foreach (PlayerController player in players)
                {
                    player.gameObject.SetActive(true);
                }
                yield return new WaitForSeconds(1.0f);
                roundInProgress = true;
                yield break;
            }
            int escapedPlayers = 0;

            foreach (var player in players)
            {
                if (player.hasEscaped)
                {
                    escapedPlayers++;
                }
            }

            if (escapedPlayers >= players.Count - 1)
            {
                roundInProgress = false;
                m_worldManager.resetVariables();
                if (m_worldManager.policeControllers.Count <= 0)
                {
                    // Spawn police
                    m_worldManager.SpawnPolice();

                    // Spawn doors
                    m_worldManager.SpawnDoors();
                }
                else
                {
                    CleanUpRound();
                }
                foreach (PlayerController player in players)
                {
                    player.gameObject.SetActive(true);
                }
                yield return new WaitForSeconds(1.0f);
                roundInProgress = true;
            }
        }
        private IEnumerator RoundEnd()
        {
            //roundInProgress = false;
            // Display round results
            m_audioManager.PlayGameOverMusic();
            yield return new WaitForSeconds(.5f);
            m_uiManager.HideHUD();
            yield return new WaitForSeconds(.5f);
            m_uiManager.ShowResultScreen(playerScores); //TODO Manoj
            CleanUpRound(); 

            // Wait for player to view results
            //yield return new WaitForSeconds(3f);
            yield return new WaitForSeconds(1.5f);

            m_uiManager.HideResultScreen(); //TODO Manoj
            yield return new WaitForSeconds(1f);
            // Clean up for next round
        }

        public void PlayerEscaped(PlayerController player)
        {
            if(onWatch)
            {
                StartCoroutine(ResetRoundVar());
                return;
            }
            if (!finishOrder.Contains(player))
            {
                finishOrder.Add(player);
                AssignScore(player);
            }
            m_uiManager.playerCompletedImages[players.IndexOf(player)].gameObject.SetActive(true);
            m_uiManager.playerCompletedImages[players.IndexOf(player)].transform.localScale = Vector3.zero;
            m_uiManager.playerCompletedImages[players.IndexOf(player)].transform.DOScale(0.35481f, .25f);
        }

        private void AssignScore(PlayerController player)
        {
            Debug.LogError("Assign Player Score :::" + player.gameObject.name);
            int position = finishOrder.IndexOf(player) + 1;
            switch (position)
            {
                case 1:
                    playerScores[player] += 10;
                    firstPlaceFinishes[player]++;
                    break;
                case 2:
                    playerScores[player] += 5;
                    break;
                case 3:
                    playerScores[player] += 3;
                    break;
                case 4:
                    playerScores[player] += 1;
                    break;
            }
            // Update UI
            m_uiManager.UpdatePlayerScores(playerScores);
        }

        private void CheckRoundEndConditions()
        {
            if (
#if JAILBREAK_FUSION
            (networkRunner != null ? networkRunner.IsSharedModeMasterClient : true) || isOfflineMode)
#else
            true && !OnWatch
#endif
            )
            {
                if (players[0].hasEscaped)
                {
                    roundInProgress = false;
                    foreach (var player in players)
                    {
                        if (!player.hasEscaped)
                        {
                            player.hasEscaped = true; // Mark as escaped for scoring purposes
                            finishOrder.Add(player);
                            player.gameObject.SetActive(false);
                            AssignScore(player);
                        }
                    }
                    return;
                }
                // The round ends when three players have escaped
                int escapedPlayers = 0;

                foreach (var player in players)
                {
                    if (player.hasEscaped)
                    {
                        escapedPlayers++;
                    }
                }

                if (escapedPlayers >= players.Count - 1)
                {
                    // Assign 4th place to the remaining player
                    foreach (var player in players)
                    {
                        if (!player.hasEscaped)
                        {
                            player.hasEscaped = true; // Mark as escaped for scoring purposes
                            finishOrder.Add(player);
                            AssignScore(player);
                            break;
                        }
                    }
                    roundInProgress = false;
                }
            }
        }


        private void CleanUpRound()
        {
            // Destroy remaining keys
            m_worldManager.CleanUp();
            foreach (var player in players)
            {
                player.ResetPlayer();
            }
            finishOrder.Clear();
        }

        private void ShowFinalResults()
        {
            // Determine the winner
            int highestScore = playerScores.Values.Max();
            var topPlayers = playerScores.Where(p => p.Value == highestScore).Select(p => p.Key).ToList();

            string resultMessage = "";

            if (topPlayers.Count == 1)
            {
                resultMessage = $"{(topPlayers[0].isAI ? "Bot" : "You")} win!";
            }
            else
            {
                // Tie-breaker: most first-place finishes
                int maxFirstPlaceFinishes = topPlayers.Max(p => firstPlaceFinishes[p]);
                var finalWinners = topPlayers.Where(p => firstPlaceFinishes[p] == maxFirstPlaceFinishes).ToList();

                if (finalWinners.Count == 1)
                {
                    resultMessage = $"{(finalWinners[0].isAI ? "Bot" : "You")} win!";
                }
                else
                {
                    resultMessage = "It's a tie!";
                }
            }

            // Display final results
            //m_uiManager.ShowFinalResult(resultMessage);
        }

        public bool IsRoundInProgress()
        {
            return roundInProgress;
        }

        public IEnumerator DelayedAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

#if JAILBREAK_FUSION
        #region Fusion Callbacks

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {

            Debug.Log("Player joined: " + player);
            if (runner.LocalPlayer == player)
            {
                if (runner.IsSharedModeMasterClient)
                {
                    // Spawn police
                    m_worldManager.SpawnPolice();

                    // Spawn doors
                    m_worldManager.SpawnDoors();
                }
                Debug.Log("Server");
                // Assign spawn point based on player count
                var spawnPoints = m_worldManager.playerSpawnPoints;
                int playerIndex = runner.SessionInfo.PlayerCount - 1;
                Vector3 spawnPosition = spawnPoints[playerIndex].position;
                Quaternion spawnRotation = spawnPoints[playerIndex].rotation;
                NetworkPrefabRef prefabRef;
                string playerDoorTag;

                switch (playerIndex)
                {
                    case 0:
                        prefabRef = player1NetworkPrefab;
                        playerDoorTag = playerDoorTags[0];
                        break;
                    case 1:
                        prefabRef = player2NetworkPrefab;
                        playerDoorTag = playerDoorTags[1];
                        break;
                    case 2:
                        prefabRef = player3NetworkPrefab;
                        playerDoorTag = playerDoorTags[2];
                        break;
                    case 3:
                        prefabRef = player4NetworkPrefab;
                        playerDoorTag = playerDoorTags[3];
                        break;
                    default:
                        prefabRef = player1NetworkPrefab;
                        playerDoorTag = playerDoorTags[0];
                        break;
                }

                NetworkObject networkPlayerObject = networkRunner.Spawn(prefabRef, spawnPosition, spawnRotation, player);
                Debug.Log(playerDoorTag);
                PlayerController playerController = networkPlayerObject.GetComponent<PlayerController>();
                playerController.isAI = false;
                playerController.playerDoorTag = playerDoorTag;


                // Ensure that the player's initial position is synchronized
                playerController.transform.position = spawnPosition;

                if (!playerScores.ContainsKey(playerController))
                {
                    playerScores[playerController] = 0;
                    firstPlaceFinishes[playerController] = 0;
                }

                spawnedCharacters.Add(player, networkPlayerObject);
            }
        }


        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            Vector2 inp = NostraInput.GetAxis("joystick");
            data.direction = new Vector2(inp.y, -inp.x);
            input.Set(data);
        }

        // Implement other required callbacks with empty bodies or as needed

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            // throw new NotImplementedException();
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            // throw new NotImplementedException();
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            //throw new NotImplementedException();
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            // throw new NotImplementedException();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            // throw new NotImplementedException();
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            // throw new NotImplementedException();
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            // throw new NotImplementedException();
        }

        #endregion
#endif
    }
}

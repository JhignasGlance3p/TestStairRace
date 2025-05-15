using Newtonsoft.Json;
using nostra.character;
using nostra.core.games;
using nostra.quickplay.core.Recorder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace nostra.pkpl.stairrace
{
    public class StairRaceManager : MonoBehaviour
    {
        [SerializeField] StairRaceController m_handler;
        [Header("Platform")]
        [SerializeField] List<StairBlock> stairBlocks;
        [SerializeField] PlatformsManager platformsManager;
        [Header("Managers")]
        [SerializeField] UiManager uiManager;
        [SerializeField] CameraManager cameraManager;

        [Space]
        [Header("Player")]
        [SerializeField] PlayerController playerPrefab;
        [SerializeField] Transform playerParent;
        [SerializeField] float3 playerSpawnPos;
        [SerializeField] int enemyCount;
        [SerializeField] EnemyController enemyPrefab;
        [SerializeField] float3[] enemySpawnOffsets;
        [Space]
        [Header("Platform")]
        [SerializeField] float3 collectibleStairDimensions;
        [SerializeField] float3 stairDimensions;
        [SerializeField] Material StairtransparentMat;
        [SerializeField] Material StairGreyMat;
        [Space]
        [Header("UI")]
        [SerializeField] ColorShowPopup colorPopup;
        [SerializeField] GameObject watch_panel, game_panel;
        [SerializeField] FloatingJoystick joystick;

        int[] playerColors;
        List<IPlayer> players = new();
        List<PlatformManager> platformHandlers { get; set; } = new() { };
        int[] randomBlockIndex;
        public ColorName PlayerColor { get; private set; }
        PlayerController spawnedPlayer;
        List<GameObject> m_characterPool = new();
        int score;
        bool isGameRunning;
        bool isGameStarted = false;
        bool isWatching = false;
        string[] replayContents = null;
        string recordPath = string.Empty;
        System.Random _random = new System.Random();
        StairRaceGameProgress gameProgress;
        StreamWriter progressWrite;
        int currentWatchFrame = -1;
        Dictionary<string, IReconstructable> replayMapper = new Dictionary<string, IReconstructable>();
        List<CollectibleBlock> powerupBlocks = new List<CollectibleBlock>();

        public int Score => score;
        public PlayerController Player => spawnedPlayer;
        public bool IsGameRunning => isGameRunning;
        public float3 CollectibleStairDimensions => collectibleStairDimensions;
        public float3 StairDimensions => stairDimensions;
        public List<StairBlock> StairTypes => stairBlocks;
        public Dictionary<ColorName, StairBlock> StairTypesDict => stairBlocks.ToDictionary(x => x.Color, x => x);
        public PlatformsManager PlatformsManager => platformsManager;
        public Material TransparentMat => StairtransparentMat;
        public List<IPlayer> Players => players;
        public List<CollectibleBlock> PowerupBlocks => powerupBlocks;

        public void OnLoaded()
        {
            playerColors = new int[stairBlocks.Count];
            for (int i = 0; i < stairBlocks.Count; i++)
            {
                playerColors[i] = (int)stairBlocks[i].Color;
            }
            platformHandlers = platformsManager.OnLoaded();
            int maxBlocksOnPlatform = -1;
            foreach (PlatformManager platform in platformHandlers)
            {
                if (maxBlocksOnPlatform < platform.maxBlocksOnPlatform())
                {
                    maxBlocksOnPlatform = platform.maxBlocksOnPlatform();
                }
            }
            StateRegistry.Register<SRPlayerData>();
            StateRegistry.Register<SRCollectibleData>();
            StateRegistry.Register<SRStairData>();
            SetRandomBlockIndex(maxBlocksOnPlatform);
            SpawnPlayers();
            SpawnPowerupBlocks();
            m_handler.SetReplayMapper(replayMapper);
        }
        void SpawnPlayers()
        {
            spawnedPlayer = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity, playerParent);
            spawnedPlayer.name = "Player";
            spawnedPlayer.OnLoaded(this, cameraManager, 0);
            players.Add(spawnedPlayer);
            m_handler.RegisterTrackable(spawnedPlayer);
            replayMapper.Add(spawnedPlayer.PlayerIndex.ToString(), spawnedPlayer);
            for (int i = 1; i <= enemyCount; i++)
            {
                var enemy = Instantiate(enemyPrefab, playerSpawnPos + enemySpawnOffsets[i - 1], Quaternion.identity, playerParent);
                enemy.name = $"Enemy{i}";
                enemy.OnLoaded(this, i);
                players.Add(enemy);
                m_handler.RegisterTrackable(enemy);
                replayMapper.Add(enemy.PlayerIndex.ToString(), enemy);
            }
        }
        void SpawnPowerupBlocks()
        {
            for(int i = 0; i < spawnedPlayer.NumberOfPickupsPossible; i++)
            {
                CollectibleBlock block = stairBlocks[0].Prefab;
                CollectibleBlock instance = Instantiate(block, playerSpawnPos, Quaternion.identity, null);
                instance.gameObject.SetActive(false);
                instance.OnLoaded(this, (platformHandlers.Count + 1), i, true);
                instance.Reset(ColorName.None);
                powerupBlocks.Add(instance);
            }
        }
        void SetRandomBlockIndex(int _maxBlocksOnPlatform)
        {
            randomBlockIndex = new int[_maxBlocksOnPlatform];
            int stairIndex = 0;
            for (int i = 0; i < randomBlockIndex.Length; i++)
            {
                if (stairIndex >= stairBlocks.Count)
                {
                    stairIndex = 0;
                }
                randomBlockIndex[i] = (int)stairBlocks[stairIndex].Color;
                stairIndex++;
            }
        }
        public void OnPrefocus()
        {
            int _index = 0;
            foreach (IPlayer player in players)
            {
                player.SetCharacter(m_handler.GetCharacter(_index));
                _index++;
            }
        }
        public void OnFocussed()
        {
            OnRestart(false);
        }
        public void OnStart()
        {
            OnRestart(true);
        }
        public void OnPause()
        {
            OnFocussed();
        }
        public void onRestart()
        {
            OnStart();
        }
        public void OnReplayStart()
        {
            isWatching = true;
        }

        void Update()
        {
            if(isGameRunning)
            {
                if(players != null && players.Count > 0 && players[0] != null && players[0].IsPlayer == true)
                {
                    players[0].OnMoveEvent(joystick.Direction);
                }
            }
        }
        void LateUpdate()
        {
            if(CanSave())
            {
                m_handler.WriteAction();
            }
        }

        public void OnRestart(bool _isStart = true)
        {
            isWatching = false;
            game_panel.gameObject.SetActive(_isStart);
            joystick.enabled = false;
            gameProgress = null;
            StartGame(_isStart);
        }

        public Material GetMaterial(ColorName _color)
        {
            if (_color == ColorName.None)
            {
                return StairtransparentMat;
            }
            else if (_color == ColorName.Grey)
            {
                return StairGreyMat;
            }
            return stairBlocks.Find(x => x.Color == _color).Material;
        }
        public void TriggerGameStart(bool _isReplay = false)
        {
            joystick.enabled = true;
            uiManager.SetPlayerColor(PlayerColor);
            isGameRunning = true;
            isGameStarted = true;
            foreach (IPlayer player in players)
            {
                player.OnStart();
            }
            m_handler.StartRecording();
        }
        public void TriggerGameOver(bool value)
        {
            isGameRunning = false;
            isGameStarted = false;
            joystick.enabled = false;
            joystick.OnPointerUp(null);
            foreach (IPlayer player in players)
            {
                player.OnGameOver();
            }
            if(isWatching == false)
            {
                m_handler.StopRecording();
                StartCoroutine(ActionOnGameOver());
            }
        }
        public void PlayAnimation(NostraCharacter _character, string _animation, bool _play)
        {
            m_handler.PlayCharacterAnimation(_character, _animation, _play);
        }
        public void PlayAnimation(NostraCharacter _character, string _animation, float _speed)
        {
            m_handler.PlayCharacterAnimation(_character, _animation, _speed);
        }
        void StartGame(bool _isStart = true)
        {
            score = 0;
            for(int i = 0; i < playerColors.Length; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, playerColors.Length);
                int temp = playerColors[i];
                playerColors[i] = playerColors[randomIndex];
                playerColors[randomIndex] = temp;
            }
            SRFirstFrame firstFrame = SetFirstFrameOnStart();
            SRUpdateProgress updateProgress = SetupCurrentFrameOnStart();
            
            SetupFirstFrame(firstFrame);
            SetupCurrentFrame(updateProgress);

            if(_isStart == false)
            {
                StartGameForFocus();
            }
            else
            {
                colorPopup.StartShow(PlayerColor, TriggerGameStart);
            }
        }
        void StartGameForFocus()
        {
            foreach (IPlayer player in players)
            {
                player.OnFocussed();
            }
        }
        
        void Shuffle(int[] array)
        {
            int p = array.Length;
            for (int n = p - 1; n > 0; n--)
            {
                int r = _random.Next(1, n);
                int t = array[r];
                array[r] = array[n];
                array[n] = t;
            }
        }

        SRFirstFrame SetFirstFrameOnStart()
        {
            SRFirstFrame firstFrame = new();
            firstFrame.platformData = new();
            for (int _index = 0; _index < platformHandlers.Count; _index++)
            {
                Shuffle(randomBlockIndex);
                SRPlatformData platformData = new();
                platformData.randomColors = new int[randomBlockIndex.Length];
                for (int j = 0; j < randomBlockIndex.Length; j++)
                {
                    platformData.randomColors[j] = randomBlockIndex[j];
                }
                List<SRPowerupData> SRPowerups = new();
                for (int j = 0; j < 3; j++)
                {
                    SRPowerupData powerupData = new();
                    powerupData.operation = UnityEngine.Random.Range(0, 3);
                    if (powerupData.operation <= 1)
                    {
                        powerupData.randomValue = UnityEngine.Random.Range(1, 4);
                    }
                    else
                    {
                        powerupData.randomValue = UnityEngine.Random.Range(2, 4);
                    }
                    SRPowerups.Add(powerupData);
                }
                platformData.powerups = SRPowerups;
                firstFrame.platformData.Add(platformData);
            }
            return firstFrame;
        }
        SRUpdateProgress SetupCurrentFrameOnStart()
        {
            int _colorIndex = 0;
            int _botIndex = 0;
            SRUpdateProgress updateProgress = new();

            updateProgress.isGameOver = false;
            updateProgress.platformProgress = new();
            updateProgress.playerProgress = new();

            for (int index = 0; index < platformHandlers.Count; index++)
            {
                SRPlatformProgress platformProgress = new();
                platformProgress.platformIndex = index;
                platformProgress.bridgeProgress = new();
                for (int j = 0; j < platformHandlers[index].BridgesOnPlatfom.Count; j++)
                {
                    SRBridgeProgress bridgeProgress = new();
                    bridgeProgress.bridgeIndex = j;
                    bridgeProgress.isOccupied = false;
                    bridgeProgress.currentBlockedIndex = 0;
                    bridgeProgress.stairProgress = new();
                    for (int k = 0; k < platformHandlers[index].BridgesOnPlatfom[j].StairsOnBridge.Length; k++)
                    {
                        SRStairProgress stairProgress = new();
                        stairProgress.stairIndex = k;
                        stairProgress.color = ColorName.None;
                        bridgeProgress.stairProgress.Add(stairProgress);
                    }
                    platformProgress.bridgeProgress.Add(bridgeProgress);
                }
                updateProgress.platformProgress.Add(platformProgress);
            }
            foreach (IPlayer player in players)
            {
                SRPlayerProgress playerProgress = new();
                playerProgress.color = (ColorName)playerColors[_colorIndex];
                playerProgress.blockInHand = 0;
                playerProgress.currentPlatformIndex = 0;
                playerProgress.currentBridgeIndex = -1;
                playerProgress.currentStairIndex = -1;
                playerProgress.playerIndex = _colorIndex;
                playerProgress.curentRotation = new SerializableVector3(Vector3.zero);
                playerProgress.collectedBlocks = new();
                if (player.IsPlayer == false)
                {
                    playerProgress.currentPosition = new SerializableVector3(playerSpawnPos + enemySpawnOffsets[_botIndex]);
                    _botIndex++;
                }
                else
                {
                    PlayerColor = (ColorName)playerColors[_colorIndex];
                    playerProgress.currentPosition = new SerializableVector3(playerSpawnPos);
                }
                _colorIndex++;
                updateProgress.playerProgress.Add(playerProgress);
            }
            return updateProgress;
        }
        void SetupFirstFrame(SRFirstFrame _firstFrameData)
        {
            int _index = 0;
            foreach (SRPlatformData platformData in _firstFrameData.platformData)
            {
                platformHandlers[_index].Reset(platformData.randomColors, platformData.powerups);
                _index++;
            }
            foreach (IPlayer player in players)
            {
                player.Reset();
            }
        }
        void SetupCurrentFrame(SRUpdateProgress _currentFrame)
        {
            foreach (SRPlayerProgress playerProgress in _currentFrame.playerProgress)
            {
                players[playerProgress.playerIndex].SetProgress(playerProgress);
            }
            foreach (SRPlatformProgress platformProgress in _currentFrame.platformProgress)
            {
                if (platformProgress.platformIndex >= 0 && platformProgress.platformIndex < platformHandlers.Count)
                    platformHandlers[platformProgress.platformIndex].SetProgress(platformProgress);
            }
        }
        IEnumerator ActionOnGameOver()
        {
            yield return new WaitForSeconds(1f);
            switch(m_handler.CardState)
            {
                case CardState.FOCUSED:
                    OnRestart(false);
                    break;
                case CardState.START:
                case CardState.RESTART:
                case CardState.REDIRECT:
                    OnGameOver();
                    break;
            }
        }
        void OnGameOver()
        {
            List<PlayerData> playersList = new();
            int index = 1;
            foreach (IPlayer player in players)
            {
                PlayerData data = new PlayerData
                {
                    platformIndex = player.platformIndex,
                    totalStairsInHand = player.CollectedStairCount,
                    PlayerName = player.IsPlayer ? "You" : "Player_" + index,
                    isPlayer = player.IsPlayer
                };
                playersList.Add(data);
                index++;
            }
            playersList = playersList.OrderByDescending(p => p.platformIndex)
                                   .ThenByDescending(p => p.totalStairsInHand)
                                   .ToList();

            GameOverLeaderboard leaderboard = new GameOverLeaderboard();
            foreach (PlayerData player in playersList)
            {
                GameOverRank rank = new GameOverRank();
                rank.playerName = player.PlayerName;
                rank.playerScore = player.platformIndex;
                rank.isPlayer = player.isPlayer;
                leaderboard.lb.Add(rank);
            }
            m_handler.GameOverScreen(leaderboard);
        }
        public bool CanSave()
        {
            switch (m_handler.CardState)
            {
                case CardState.START:
                case CardState.RESTART:
                case CardState.REDIRECT:
                    if(isGameRunning == true)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
        public void RegisterBlock(CollectibleBlock _block)
        {
            replayMapper.Add((_block.PlatformIndex + "_" + _block.BlockIndex), _block);
            m_handler.RegisterTrackable(_block);
            m_handler.SetReplayMapper(replayMapper);
        }
        public void RegisterStair(Stair _stair)
        {
            replayMapper.Add((_stair.ReplayPrefix + "_" + _stair.StairIndex), _stair);
            m_handler.RegisterTrackable(_stair);
            m_handler.SetReplayMapper(replayMapper);
        }
        // int currentLightColor = -1;
        // [SerializeField] Light directionalLight;
        // public void ChangeColor(int index)
        // {
        //     if (index == currentLightColor)
        //     {
        //         currentLightColor = -1;
        //         directionalLight.color = new Color(1, 1, 1);
        //     }
        //     else if (index == 0)
        //     {
        //         currentLightColor = 0;
        //         directionalLight.color = new Color(0, 1, 0);
        //     }
        //     else if (index == 1)
        //     {
        //         currentLightColor = 1;
        //         directionalLight.color = new Color(0.1176471f, 0.61f, 0);
        //     }
        // }
        public void ChangeCamera(int index)
        {
            cameraManager.ChangeCamera(index);
        }
    }
    [Serializable]
    public class PlayerData
    {
        public int platformIndex;
        public int totalStairsInHand;
        public string PlayerName;
        public bool isPlayer;
    }
    [Serializable]
    public class SRPlayerData : IGameObjectState
    {
        public int playerIndex;
        public ColorName color;
        public SerializableVector3 currentPosition;
        public SerializableVector3 curentRotation;

        string IGameObjectState.Id => playerIndex.ToString();
        bool IGameObjectState.canCapture => true;
    }
    [Serializable]
    public class SRCollectibleData : IGameObjectState
    {
        public string Id;
        public int currentPlayerIndex = -1;
        public SerializableVector3 currentPosition;
        public SerializableVector3 curentRotation;
        public ColorName color;
        public bool canCapture;
        public bool isPowerup = false;

        string IGameObjectState.Id => Id;
        bool IGameObjectState.canCapture => canCapture;
    }
    [Serializable]
    public class SRStairData : IGameObjectState
    {
        public string Id;
        public bool isOccupied;
        public ColorName color;
        public bool canCapture;

        string IGameObjectState.Id => Id;
        bool IGameObjectState.canCapture => canCapture;
    }
}
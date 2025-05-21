using nostra.origami.common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace nostra.origami.crowdcity
{
    [Serializable]
    public class GameStatus
    {
        public int frame;
        public float gameTimeElapsed;
        public int killStatus;
        public List<Crowd> crowds;
        public List<NeutralCharacterStatus> audiences;
        public List<PowerupStatus> powerups;
    }
    [Serializable]
    public class Crowd
    {
        public string crowdTag;
        public int crowdCount;
        public string crowdGangName;
        public Color crowdColor;
        public string characterId;
        public int randomCustomiseId;
        public Vector3 initialPosition;
        public bool isPlayer;
        public Vector3 currentPosition;
        public Vector3 currentRotation;
        public int currentCount;
    }
    [Serializable]
    public class PowerupStatus
    {
        public int operation;
        public int randomValue;
        public bool isVisible;
    }
    [Serializable]
    public class NeutralCharacterStatus
    {
        public int parentIndex;
        public bool isVisible;
    }

    [Serializable]
    public class PowerupHolder
    {
        public int operation;
        public int weight;
        public int minRand;
        public int maxRand;
    }
    public enum CharacterType
    {
        NEUTRAL,
        PLAYER,
        AI
    }
    public class CrowdCityManager : MonoBehaviour
    {
        [SerializeField] PowerupHolder[] randomPowerups;
        [SerializeField] Transform powerupParent;
        [SerializeField] Transform environmentHolder;
        [SerializeField] Transform crowdPoints;
        [SerializeField] Transform opponents;
        [SerializeField] Transform aiPoints;
        [SerializeField] CrowdHandler playerHandler;
        [SerializeField] CrowdController playerController;
        [SerializeField] TextMeshProUGUI timerTxt, killsTxt;
        [SerializeField] float gameTimer = 30f;
        [SerializeField] GameObject inGamePanel;
        [SerializeField] ObjectPooler pooler;
        [SerializeField] GameObject joystick;

        int m_RandomAICount = 4;
        int poolSize = 20;

        PowerUp[] powerUps;
        List<GameObject> m_characterPool = new();
        List<GameObject> m_characterInUse = new();
        Vector2 moveDirection;
        float gameElapsedTime = 0f;
        int killCounter = 0;
        bool isGameOver = false;
        bool isGameStarted = false;
        List<CrowdHandler> CrowdHandlers { get; set; } = new() { };
        List<NeutralCharacter> neutralCharacters = new();
        CrowdCityController m_controller;
        GameStatus currentStatus;
        Crowd playerCrowd;
        int[] randomIndex = new[] { 0, 1, 2, 3 };
        System.Random _random = new System.Random();
        int currentIndex = 0;
        StreamWriter progressWrite;
        string[] replayContents;
        string recordPath;

        public CrowdCityController Controller => m_controller;
        public ObjectPooler Pooler => pooler;
        public int currentStartFrame;
        public bool GameOver => isGameOver;

        private void Update()
        {
            if (Controller == null || isGameOver == true || isGameStarted == false)
            {
                return;
            }
            // if (Controller.PlatformCardState == PostCardState.START || Controller.PlatformCardState == PostCardState.CONTINUE || Controller.PlatformCardState == PostCardState.RESTART)
            // {
                playerController.HandleInput(moveDirection);
                if (gameElapsedTime < gameTimer)
                {
                    gameElapsedTime += Time.deltaTime;

                    float remainingTime = Mathf.Clamp(gameTimer - gameElapsedTime, 0f, gameTimer);
                    TimeSpan remainingTimeSpan = TimeSpan.FromSeconds(remainingTime);

                    string etaTimeString = string.Format("{0:D2}:{1:D2}", remainingTimeSpan.Minutes, remainingTimeSpan.Seconds);
                    timerTxt.text = etaTimeString;
                }
                else
                {
                    WinGame();
                }
            // }
        }
        protected void LateUpdate()
        {
            // if ((Controller.PlatformCardState == PostCardState.START
            // || Controller.PlatformCardState == PostCardState.CONTINUE ||
            // Controller.PlatformCardState == PostCardState.RESTART) &&
            // isGameOver == false && progressWrite != null && isGameStarted == true)
            // {
            //     currentStatus = new();
            //     currentStatus.frame = currentStartFrame;
            //     currentStatus.gameTimeElapsed = gameElapsedTime;
            //     currentStatus.killStatus = killCounter;
            //     currentStatus.powerups = new();
            //     currentStatus.crowds = new();
            //     currentStatus.audiences = new();
            //     foreach (PowerUp powerup in powerUps)
            //     {
            //         currentStatus.powerups.Add(powerup.PowerupStatus);
            //     }
            //     foreach (CrowdHandler handler in CrowdHandlers)
            //     {
            //         currentStatus.crowds.Add(handler.CrowdStatus);
            //     }
            //     foreach (NeutralCharacter neutralCharacter in neutralCharacters)
            //     {
            //         currentStatus.audiences.Add(neutralCharacter.NCharacterStatus);
            //     }
            //     if (progressWrite != null)
            //     {
            //         progressWrite.Write(JsonUtility.ToJson(currentStatus));
            //         progressWrite.WriteLine();
            //     }
            //     currentStartFrame++;
            // }
            // if (Controller.CurrentPost.type == PostType.WATCH || (Controller.PlatformCardState == PostCardState.WATCH || Controller.PlatformCardState == PostCardState.REPLAY))
            // {
            //     if (replayContents != null && replayContents.Length > 0)
            //     {
            //         if (currentIndex < replayContents.Length)
            //         {
            //             currentStatus = JsonUtility.FromJson<GameStatus>(replayContents[currentIndex]);
            //             if (currentStatus != null)
            //             {
            //                 float remainingTime = Mathf.Clamp(gameTimer - currentStatus.gameTimeElapsed, 0f, gameTimer);
            //                 TimeSpan remainingTimeSpan = TimeSpan.FromSeconds(remainingTime);
            //                 string etaTimeString = string.Format("{0:D2}:{1:D2}", remainingTimeSpan.Minutes, remainingTimeSpan.Seconds);
            //                 timerTxt.text = etaTimeString;

            //                 killCounter = currentStatus.killStatus;
            //                 UpdateKill(killCounter);

            //                 int index = 0;
            //                 foreach (Crowd crowd in currentStatus.crowds)
            //                 {
            //                     CrowdHandlers[index].UpdateStatus(crowd);
            //                     index++;
            //                 }
            //                 index = 0;
            //                 foreach (PowerupStatus status in currentStatus.powerups)
            //                 {
            //                     powerUps[index].UpdateStatus(status);
            //                     index++;
            //                 }
            //                 index = 0;
            //                 foreach (NeutralCharacterStatus status in currentStatus.audiences)
            //                 {
            //                     neutralCharacters[index].UpdateStatus(status, false);
            //                     index++;
            //                 }
            //             }
            //             currentIndex++;
            //         }
            //         else
            //         {
            //             currentIndex = 0;
            //         }
            //     }
            // }
        }

        public void Initialise(CrowdCityController _controller)
        {
            this.m_controller = _controller;
            pooler.OnLoaded();

            m_RandomAICount = opponents.transform.childCount;
            CrowdHandlers = new();

            int index = 0;
            if (powerUps == null || powerUps.Length <= powerupParent.childCount)
            {
                powerUps = new PowerUp[powerupParent.childCount];
                foreach (Transform child in powerupParent)
                {
                    powerUps[index] = child.gameObject.GetComponent<PowerUp>();
                    index++;
                }
            }

            playerCrowd = new Crowd();
            playerCrowd.crowdCount = 1;
            playerCrowd.crowdGangName = "King";
            playerCrowd.crowdColor = Color.blue;
            playerCrowd.crowdTag = "guy";
            //playerCrowd.characterId = Controller.CurrentPost.game.characters[0].characterId;
            playerCrowd.randomCustomiseId = -1;
            playerCrowd.isPlayer = true;
            playerCrowd.initialPosition = playerHandler.transform.position;
            playerCrowd.currentPosition = playerHandler.transform.position;
            playerCrowd.currentRotation = Vector3.zero;
            playerHandler.Initialise(this);
            CrowdHandlers.Add(playerHandler);
            foreach (Transform child in opponents.transform)
            {
                CrowdHandler crowdHandler = child.GetComponent<CrowdHandler>();
                CrowdHandlers.Add(crowdHandler);
                crowdHandler.Initialise(this);
            }
        }
        public void Construct()
        {
            if (m_characterPool == null || m_characterPool.Count <= 0)
            {
                m_characterPool = new();
                // AssetsController.Instance.GetCharacter(this.Controller.CurrentPost.game.characters[0].address, 300, (obj) =>
                // {
                //     if (obj != null && obj.Length > 0)
                //     {
                //         foreach (GameObject _go in obj)
                //         {
                //             _go.transform.SetParent(this.transform);
                //             m_characterPool.Add(_go);
                //         }
                //     }
                //     PlaceNeutralCharacter();
                //     RandomisePowerups();
                //     foreach (CrowdHandler handler in CrowdHandlers)
                //     {
                //         handler.OnRender();
                //     }

                //     if (Controller.CurrentPost.type == PostType.WATCH)
                //     {
                //         // AssetsController.Instance.InitiateDownloadZip(Controller.CurrentPost.game.replayUrl, Controller.CurrentPost.post_id, (_path) =>
                //         // {
                //         //     Controller.OnRenderingCompleted();
                //         // });
                //     }
                //     else
                //     {
                //         Controller.OnRenderingCompleted();
                //     }
                // });
            }
            else
            {
                // if (Controller.CurrentPost.type == PostType.WATCH)
                // {
                //     AssetsController.Instance.InitiateDownloadZip(Controller.CurrentPost.game.replayUrl, Controller.CurrentPost.post_id, (_path) =>
                //     {
                //         Controller.OnRenderingCompleted();
                //     });
                // }
                // else
                // {
                //     Controller.OnRenderingCompleted();
                // }
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
            // PlatformController.Instance.NInput.OnMoveEvent -= OnMovement;
            isGameStarted = false;
            foreach (CrowdHandler handler in CrowdHandlers)
            {
                handler.OnPause();
            }
            if (progressWrite != null)
            {
                progressWrite.Close();
                progressWrite = null;
            }
        }
        public void OnRestart(bool _isStart = true)
        {
            // if (Controller.CurrentPost.type == PostType.WATCH)
            // {
                // AssetsController.Instance.InitiateDownloadZip(Controller.CurrentPost.game.replayUrl, Controller.CurrentPost.post_id, (_path) =>
                // {
                //     joystick.gameObject.SetActive(false);
                //     currentIndex = 0;
                //     SetupForWatch(_path);
                // });
                MyUtils.Execute(0.1f, () =>
                {
                //     if (Controller.PlatformCardState == PostCardState.FOCUSSED || Controller.PlatformCardState == PostCardState.START
                // || Controller.PlatformCardState == PostCardState.CONTINUE || Controller.PlatformCardState == PostCardState.RESTART)
                //     {
                //         AssetsController.Instance.InitiateDownloadZip(Controller.CurrentPost.game.replayUrl, Controller.CurrentPost.post_id, (_path) =>
                //         {
                //             if (Controller.PlatformCardState == PostCardState.FOCUSSED || Controller.PlatformCardState == PostCardState.START
                // || Controller.PlatformCardState == PostCardState.CONTINUE || Controller.PlatformCardState == PostCardState.RESTART)
                //             {
                //                 joystick.gameObject.SetActive(false);
                //                 currentIndex = 0;
                //                 SetupForWatch(_path);
                //             }
                //         });
                //     }
                });
                return;
            // }
            foreach (CrowdHandler handler in CrowdHandlers)
            {
                handler.OnRestart();
            }
            int index = 0;
            // recordPath = Path.Combine(Application.persistentDataPath, $"{Controller.CurrentPost.post_id}.txt");
            bool isFileExists = File.Exists(recordPath);
            if (isFileExists && _isStart == true)
            {
                string[] contents = File.ReadAllLines(recordPath);
                if (contents.Length > 0)
                {
                    GameStatus status = JsonUtility.FromJson<GameStatus>(contents[contents.Length - 1]);
                    if (status != null)
                    {
                        index = 0;
                        currentStartFrame = contents.Length - 1;
                        gameElapsedTime = status.gameTimeElapsed;
                        killCounter = status.killStatus;
                        UpdateKill(killCounter);
                        foreach (PowerupStatus p_status in status.powerups)
                        {
                            if (index >= 0 && index < powerUps.Length)
                            {
                                powerUps[index].SetPowerup(p_status.operation, p_status.randomValue, true);
                                powerUps[index].UpdateStatus(p_status);
                            }
                            index++;
                        }
                        foreach (NeutralCharacterStatus n_status in status.audiences)
                        {
                            if (index >= 0 && index < neutralCharacters.Count)
                            {
                                neutralCharacters[index].UpdateStatus(n_status, true);
                            }
                            index++;
                        }
                        index = 1;
                        foreach (Crowd crowd in status.crowds)
                        {
                            if (index >= 0 && index < CrowdHandlers.Count)
                            {
                                crowd.crowdCount = crowd.currentCount;
                                if (crowd.isPlayer == true)
                                {
                                    CrowdHandlers[0].SetCrowdSettings(crowd);
                                }
                                else
                                {
                                    CrowdHandlers[index].SetCrowdSettings(crowd);
                                    index++;
                                }
                            }
                        }
                    }
                }
                if ((gameElapsedTime >= gameTimer || playerCrowd.currentCount <= 0))
                {
                    GameEnd();
                }
                else
                {
                    inGamePanel.SetActive(true);
                    isGameOver = false;
                    isGameStarted = true;
                    FileStream fileStream = new FileStream(recordPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    progressWrite = new StreamWriter(recordPath, true);
                    progressWrite.AutoFlush = true;
                    joystick.gameObject.SetActive(true);
                    // PlatformController.Instance.NInput.OnMoveEvent += OnMovement;
                    foreach (CrowdHandler handler in CrowdHandlers)
                    {
                        handler.OnStart();
                    }
                }
            }
            else
            {
                replayContents = null;
                UpdateKill(0);
                Shuffle(randomIndex);
                PlaceNeutralCharacter();
                RandomisePowerups();
                foreach (CrowdHandler handler in CrowdHandlers)
                {
                    if (handler.IsPlayerCrowd)
                    {
                        playerCrowd.currentPosition = playerCrowd.initialPosition;
                        playerCrowd.currentCount = 1;
                        playerCrowd.currentRotation = Vector3.zero;
                        handler.SetCrowdSettings(playerCrowd);
                    }
                    else
                    {
                        string[] characters = new[] { "BeachGuy", "Thief", "PunkGuy", "AmericanFootballer" };
                        int initialCount = Random.Range(3, 7);
                        string[] names = new[] { "Joey", "Lark", "Stark", "Joseph" };
                        Color[] colors = new[] { Color.magenta, Color.green, Color.yellow, Color.red };
                        int currentId = randomIndex[index];
                        Crowd aiCrowd = new Crowd();
                        aiCrowd.crowdCount = Random.Range(3, 7);
                        aiCrowd.crowdGangName = names[currentId];
                        aiCrowd.crowdColor = colors[currentId];
                        aiCrowd.crowdTag = characters[currentId];
                        // aiCrowd.characterId = Controller.CurrentPost.game.characters[0].baseCharacterId;
                        // aiCrowd.randomCustomiseId = Random.Range(0, PlatformController.Instance.AICustomiseCharacters.characters[0].variants.Count);
                        aiCrowd.isPlayer = false;
                        aiCrowd.initialPosition = GetRandomAIPoints();
                        aiCrowd.currentPosition = aiCrowd.initialPosition;
                        handler.SetCrowdSettings(aiCrowd);
                        if (index < randomIndex.Length - 1)
                        {
                            index++;
                        }
                    }
                }
                MyUtils.Execute(0.1f, () =>
                {
                    foreach (CrowdHandler handler in CrowdHandlers)
                    {
                        handler.OnRender();
                        if (_isStart == false)
                        {
                            inGamePanel.SetActive(false);
                            handler.OnFocussed();
                        }
                        else
                        {
                            inGamePanel.SetActive(true);
                            gameElapsedTime = 0;
                            isGameOver = false;
                            isGameStarted = true;
                            currentStartFrame = 0;
                            isGameOver = false;
                            gameElapsedTime = 0;
                            FileStream fileStream = new FileStream(recordPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                            progressWrite = new StreamWriter(recordPath, true);
                            progressWrite.AutoFlush = true;
                            joystick.gameObject.SetActive(true);
                            // PlatformController.Instance.NInput.OnMoveEvent += OnMovement;
                            handler.OnStart();
                        }
                    }
                });
            }
        }
        public void OnHidden()
        {
            isGameStarted = false;
            replayContents = null;
            ResetPoolObjects();
            foreach (CrowdHandler handler in CrowdHandlers)
            {
                handler.OnHidden();
            }
            if (m_characterPool != null)
            {
                foreach (GameObject go in m_characterPool)
                {
                    // AssetsController.Instance.AddToPooledObject(go);
                }
                m_characterPool.Clear();
            }
        }
        public void OnWatchReplay()
        {
            joystick.gameObject.SetActive(false);
            // PlatformController.Instance.NInput.OnMoveEvent -= OnMovement;
        }

        void RandomisePowerups()
        {
            if (powerUps != null)
            {
                int randomValue;
                int randomWeight;
                int lastWeight;
                for (int i = 0; i < powerUps.Length; i++)
                {
                    PowerUp powerUp = powerUps[i];
                    randomWeight = Random.Range(0, 100);
                    lastWeight = 0;
                    foreach (PowerupHolder holder in randomPowerups)
                    {
                        lastWeight += holder.weight;
                        if (lastWeight > randomWeight)
                        {
                            randomValue = Random.Range(holder.minRand, holder.maxRand);
                            powerUp.SetPowerup(holder.operation, randomValue, true);
                            break;
                        }
                    }
                }
            }
        }
        void PlaceNeutralCharacter()
        {
            if (neutralCharacters != null && neutralCharacters.Count > 0)
            {
                foreach (NeutralCharacter nCharacter in neutralCharacters)
                {
                    if (nCharacter != null)
                    {
                        if (nCharacter.CharacterGO != null)
                            PutCharacterBack(nCharacter.CharacterGO);
                        PutObjectToPool(nCharacter.GetComponent<PooledObjectResetter>(), false);
                    }
                }
                neutralCharacters = new();
            }
            List<int> placeholders = new List<int>();
            int index = 0;
            foreach (Transform child in crowdPoints)
            {
                placeholders.Add(index);
                index++;
            }
            poolSize = Mathf.Clamp(poolSize, 20, placeholders.Count);
            for (int i = 0; i < poolSize; i++)
            {
                int randomInd = Random.Range(0, placeholders.Count);
                if (crowdPoints.childCount > placeholders[randomInd])
                {
                    GameObject neutralAgent = Pooler.SpawnFromPool("Character");
                    GameObject agent = GetCharacter(CharacterType.NEUTRAL);
                    if (agent != null && neutralAgent != null)
                    {
                        neutralAgent.transform.SetParent(crowdPoints.GetChild(placeholders[randomInd]));
                        neutralAgent.transform.localPosition = Vector3.zero;
                        neutralAgent.transform.localEulerAngles = Vector3.zero;
                        neutralAgent.transform.localScale = Vector3.one;
                        NeutralCharacter nChar = neutralAgent.AddComponent<NeutralCharacter>();
                        nChar.SetAgent(agent, placeholders[randomInd]);
                        neutralAgent.SetActive(true);

                        agent.transform.SetParent(neutralAgent.transform);
                        agent.transform.localPosition = Vector3.zero;
                        agent.transform.localEulerAngles = Vector3.zero;
                        agent.transform.localScale = Vector3.one;
                        agent.SetActive(true);
                        neutralCharacters.Add(nChar);
                    }
                }
                placeholders.RemoveAt(randomInd);
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

        public GameObject GetCharacter(CharacterType type, int customId = -1)
        {
            if (m_characterPool.Count > 0)
            {
                GameObject go = m_characterPool[0];
                m_characterPool.RemoveAt(0);
                m_characterInUse.Add(go);
                if (type == CharacterType.PLAYER)
                {
                    // AssetsController.Instance.CustomiseCharacter(go, m_controller.CurrentPost.game.characters[0]);
                }
                else if (type == CharacterType.NEUTRAL)
                {
                    // customId = Random.Range(0, PlatformController.Instance.AICustomiseCharacters.characters[0].variants.Count);
                    // AssetsController.Instance.CustomisedAIChar(go, customId);
                }
                else if (type == CharacterType.AI)
                {
                    // AssetsController.Instance.CustomisedAIChar(go, customId);
                }
                return go;
            }
            return null;
        }
        public void PutCharacterBack(GameObject go)
        {
            if (m_characterInUse.Contains(go))
            {
                m_characterInUse.Remove(go);
            }
            go.transform.SetParent(this.transform);
            go.SetActive(false);
            m_characterPool.Add(go);
        }
        public void PutObjectToPool(PooledObjectResetter pooledObj, bool shouldRemoveFromList = true)
        {
            if (pooledObj != null)
            {
                Pooler.AddToPool(pooledObj);
                NeutralCharacter nCharacter = pooledObj.GetComponent<NeutralCharacter>();
                if (nCharacter != null)
                {
                    if (shouldRemoveFromList == true && neutralCharacters.Contains(nCharacter))
                    {
                        neutralCharacters.Remove(nCharacter);
                    }
                    Destroy(nCharacter);
                }
            }
        }
        public Vector3 GetRandomAIPoints()
        {
            return aiPoints.GetChild(Random.Range(0, aiPoints.childCount)).position;
        }
        private void SetupForWatch(string path)
        {
            // PlatformController.Instance.NInput.OnMoveEvent -= OnMovement;
            foreach (CrowdHandler handler in CrowdHandlers)
            {
                handler.OnRestart();
            }
            if (File.Exists(path))
            {
                isGameOver = true;
                int index = 0;
                currentStartFrame = 0;
                string[] contents = File.ReadAllLines(path);
                if (contents != null && contents.Length > 0)
                {
                    GameStatus status = JsonUtility.FromJson<GameStatus>(contents[currentStartFrame]);
                    if (status != null)
                    {
                        inGamePanel.SetActive(true);
                        gameElapsedTime = status.gameTimeElapsed;
                        killCounter = status.killStatus;
                        UpdateKill(killCounter);
                        foreach (PowerupStatus p_status in status.powerups)
                        {
                            if (index >= 0 && index < powerUps.Length)
                            {
                                powerUps[index].SetPowerup(p_status.operation, p_status.randomValue, false);
                                powerUps[index].UpdateStatus(p_status);
                            }
                            index++;
                        }
                        foreach (NeutralCharacterStatus n_status in status.audiences)
                        {
                            if (index >= 0 && index < neutralCharacters.Count)
                            {
                                neutralCharacters[index].UpdateStatus(n_status, false);
                            }
                            index++;
                        }
                        index = 1;
                        foreach (Crowd crowd in status.crowds)
                        {
                            if (index >= 0 && index < CrowdHandlers.Count)
                            {
                                crowd.crowdCount = crowd.currentCount;
                                if (crowd.isPlayer == true)
                                {
                                    CrowdHandlers[0].SetCrowdSettings(crowd);
                                    CrowdHandlers[0].OnWatch(crowd);
                                }
                                else
                                {
                                    CrowdHandlers[index].SetCrowdSettings(crowd);
                                    CrowdHandlers[index].OnWatch(crowd);
                                    index++;
                                }
                            }
                        }
                    }
                    replayContents = contents;
                }
            }
        }
        private void OnMovement(Vector2 _direction)
        {
            moveDirection = _direction;
        }
        public void UpdateKill(int amount = 0)
        {
            // if (Controller.PlatformCardState == PostCardState.START || Controller.PlatformCardState == PostCardState.RESTART || Controller.PlatformCardState == PostCardState.CONTINUE || Controller.PlatformCardState == PostCardState.WATCH || Controller.PlatformCardState == PostCardState.REPLAY)
            {
                killCounter += amount;
                killsTxt.text = $"KILLS: {killCounter}";
            }
        }
        public void ResetPowerup()
        {
            if (currentStatus == null) return;
            int index = 0;
            foreach (PowerupStatus p_status in currentStatus.powerups)
            {
                if (index >= 0 && index < powerUps.Length)
                {
                    if (powerUps[index] != null && p_status != null && p_status.isVisible == true && p_status.operation == 2)
                    {
                        p_status.operation = 0;
                        powerUps[index].SetPowerup(p_status.operation, p_status.randomValue, false);
                        powerUps[index].UpdateStatus(p_status);
                    }
                }
                index++;
            }
        }
        public void WinGame()
        {
            GameEnd();
        }
        public void LoseGame(string killedBy)
        {
            foreach (CrowdHandler handler in CrowdHandlers)
            {
                handler.OnStop();
            }
            // if (Controller.PlatformCardState == PostCardState.FOCUSSED)
            // {
                // OnRestart();
            // }
            // else if (Controller.PlatformCardState == PostCardState.START || Controller.PlatformCardState == PostCardState.CONTINUE || Controller.PlatformCardState == PostCardState.RESTART)
            // {
            //     GameEnd();
            // }
        }
        public void GameEnd()
        {
            if (progressWrite != null)
            {
                progressWrite.Close();
                progressWrite = null;
            }

            isGameOver = true;
            isGameStarted = false;
            gameElapsedTime = 0;
            killCounter = 0;
            // joystick.OnPointerUp(null);TODO Deepak

            // GameOverLeaderboard lb = new GameOverLeaderboard();
            // lb.lb = new List<GameOverRank>();
            // List<CrowdHandler> sortedList = CrowdHandlers.ToArray().OrderByDescending(crowd => crowd.CrowdCount).ToList();
            // foreach (CrowdHandler crowd in sortedList)
            // {
            //     GameOverRank rank = new GameOverRank();
            //     rank.playerName = crowd.CrowdStatus.crowdGangName;
            //     rank.playerScore = crowd.CrowdStatus.currentCount;
            //     rank.isPlayer = crowd.IsPlayerCrowd;
            //     lb.lb.Add(rank);
            // }
            // GameOverObject gameOverObject = new GameOverObject();
            // gameOverObject.leaderboard = lb;
            // gameOverObject.replayAvailable = currentStartFrame > 1000;
            // Controller.GameOver(gameOverObject);
            if (string.IsNullOrEmpty(recordPath) == false && File.Exists(recordPath))
            {
                SetupForWatch(recordPath);
            }
            else
            {
                OnRestart(false);
            }
        }
        public void ResetPoolObjects()
        {
            foreach (CrowdHandler handler in CrowdHandlers)
            {
                handler.OnGameOver();
            }
            foreach (PowerUp powerup in powerUps)
            {
                powerup.OnGameOver();
            }
            foreach (NeutralCharacter nCharacter in neutralCharacters)
            {
                PutCharacterBack(nCharacter.CharacterGO);
                PutObjectToPool(nCharacter.GetComponent<PooledObjectResetter>(), false);
            }
            m_characterInUse.Clear();
            neutralCharacters = new();
        }
    }
}
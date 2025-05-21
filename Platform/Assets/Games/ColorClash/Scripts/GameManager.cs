using DG.Tweening;
using nostra.character;
using nostra.core.games;
using nostra.quickplay.core.Recorder;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace nostra.sarvottam.colorclash
{
    public class GameManager : MonoBehaviour
    {
        public bool Gamestart { private set; get; }
        public bool GameOver { private set; get; }
        public List<PlayerMoment> allPlayers => spawnedPlayers;

        private int currentLevel;
        [Header("GameConfig")]
        [SerializeField] private int rows, colom;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject[] allTypeoftiles;
        [SerializeField] private PowerupController powerupController;
        [SerializeField] private ColorClashHandler colorClashHandler;

        [Header("Player")]
        [SerializeField] private Transform player_holder;
        [SerializeField] private List<PlayerMoment> Players;

        [SerializeField] private AudioSource backgroud_sound;
        [SerializeField] private float countdownDelay;
        [SerializeField] private float GameTimer;
        [SerializeField] private GameObject Level_holder;
        [SerializeField] private GameObject scorepanel;

        [Header("UI")]
        [SerializeField] private TextAsset[] Levels;
        [SerializeField] private Image[] allimage;
        [SerializeField] private List<TextMeshProUGUI> allPlayerScore;
        [SerializeField] private TextMeshProUGUI TimeText;
        [SerializeField] private TextMeshProUGUI TimeText1;
        [SerializeField] private TextMeshProUGUI countdown_text;
        [SerializeField] private TextMeshProUGUI[] playername;
        [SerializeField] private FloatingJoystick joystick;

        List<Vector3> temppos = new List<Vector3>();
        List<Tiles> alltiles = new List<Tiles>();
        List<PlayerMoment> spawnedPlayers = new List<PlayerMoment>();
        Coroutine gameCoroutine;
        NostraCharacter[] characters;
        int obstacleCount = 0;
        float Timer;
        Dictionary<string, IReconstructable> replayMapper = new Dictionary<string, IReconstructable>();

        public Camera GameCamera => mainCamera;
        private CameraController _camController;

        private void Update()
        {
            if (Gamestart)
            {
                TimerShowing();
            }
        }
        private void LateUpdate()
        {
            if(CanSave())
            {
                colorClashHandler?.WriteAction();
            }
        }

        public void OnLoaded()
        {
            replayMapper = new Dictionary<string, IReconstructable>();
            countdown_text.gameObject.SetActive(false);
            _camController = mainCamera.GetComponent<CameraController>();
            GridSpawn();
            PlayerSpawn();
            powerupController.OnLoaded(this);

            StateRegistry.Register<ColorClashPlayerState>();
            StateRegistry.Register<TilesState>();
            colorClashHandler.SetReplayMapper(replayMapper);
        }
        public void OnPreFocus()
        {
            joystick.enabled = false;
            joystick.OnPointerUp(null);
            characters = colorClashHandler.GetCharacters(spawnedPlayers.Count);
            PlayerControllerSpawn();
            Gamestart = false;
            GameOver = false;
            foreach (Tiles item in alltiles)
            {
                item.OnReset();
            }
        }
        public void OnFocussed()
        {
            foreach (PlayerMoment item in spawnedPlayers)
            {
                item.Ai = true;
                item.OnFocussed();
            }
            powerupController.OnFocussed();
            ScoreUpdate();
        }
        public void OnStart()
        {
            joystick.enabled = false;
            joystick.OnPointerUp(null);
            Gamestart = false;
            GameOver = false;
            float minutes = Mathf.FloorToInt(Timer / 60);
            float seconds = Mathf.FloorToInt(Timer % 60);
            TimeText.text = string.Format("{00:00}:{01:00}", minutes, seconds);
            TimeText1.text = string.Format("{00:00}:{01:00}", minutes, seconds);
            foreach (Tiles item in alltiles)
            {
                item.OnReset();
            }
            foreach (PlayerMoment item in spawnedPlayers)
            {
                item.OnReset();
            }
            spawnedPlayers[0].Ai = false;
            powerupController.OnReset();
            PlayerControllerSpawn();
            Timer = GameTimer;
            TimerShowing();
            gameCoroutine = StartCoroutine(StartCountdownTimer());
        }
        public void OnPause()
        {
            if (gameCoroutine != null)
            {
                StopCoroutine(gameCoroutine);
                gameCoroutine = null;
            }
            ToggleRecording(false);
            if(Gamestart == true)
            {
                OnFocussed();
            }
        }
        public void onRestart()
        {
            joystick.enabled = false;
            joystick.OnPointerUp(null);
            if (gameCoroutine != null)
            {
                StopCoroutine(gameCoroutine);
                gameCoroutine = null;
            }
            OnStart();
        }
        public Vector3 GetJoystickDirection()
        {
            if (Gamestart == false || GameOver == true)
            {
                return Vector3.zero;
            }
            return new Vector3(joystick.Direction.y, 0, -joystick.Direction.x);
        }
        public void ScoreUpdate()
        {
            for (int i = 0; i < spawnedPlayers.Count; i++)
            {
                float total_tiles = rows * colom;
                float value = total_tiles / 100;
                if (!spawnedPlayers[i].Ai)
                {
                    allPlayerScore[i].text = $"<b>" + spawnedPlayers[i].score.ToString("00") + "</b>";
                    playername[i].text = spawnedPlayers[i].playercolor;
                }
                else
                {
                    allPlayerScore[i].text = spawnedPlayers[i].score.ToString("00");
                    playername[i].text = spawnedPlayers[i].playercolor;
                }
                allimage[i].color = spawnedPlayers[i].LightColor;
            }
        }
        public Vector3 GetRandomPosition(bool isPower = false)
        {
            Vector3 randompos;
            if (isPower == true)
            {
                randompos = new Vector3(Random.Range(0, rows), 1, Random.Range(0, colom));
            }
            else
            {
                randompos = new Vector3(Random.Range(1, rows), 1, Random.Range(1, colom));
            }
            for (int i = 0; i < temppos.Count; i++)
            {
                if (randompos.x == temppos[i].x && randompos.z == temppos[i].z)
                {
                    if (isPower == true)
                    {
                        randompos = new Vector3(Random.Range(0, rows), 1, Random.Range(0, colom));
                    }
                    else
                    {
                        randompos = new Vector3(Random.Range(1, rows), 1, Random.Range(1, colom));
                    }
                    i = 0;
                }
            }
            return randompos;
        }
        public void PlayCharacterAnimation(NostraCharacter _character, string _animation, bool _play)
        {
            colorClashHandler?.PlayCharacterAnimation(_character, _animation, _play);
        }
        public void PlayCharacterAnimation(NostraCharacter _character, string _animation, float _speed)
        {
            colorClashHandler?.PlayCharacterAnimation(_character, _animation, _speed);
        }

        void GridSpawn()
        {
            string[] row = Levels[currentLevel].text.Split(":");
            int x = 0;
            int z = 0;
            int id = 0;
            foreach (var rows in row)
            {
                z = 0;
                string[] colomns = rows.Split(",");
                foreach (var coloms in colomns)
                {
                    Tiles tile = Instantiate(allTypeoftiles[int.Parse(coloms)], new Vector3(x, 0, z), Quaternion.identity, Level_holder.transform).GetComponent<Tiles>();
                    tile.OnLoaded();
                    tile.id = id;
                    if (tile.Obstacle)
                    {
                        temppos.Add(tile.transform.position);
                        obstacleCount++;
                    }
                    else
                    {
                        replayMapper.Add(tile.id.ToString(), tile);
                        colorClashHandler.RegisterTrackable(tile);
                    }
                    if (int.Parse(coloms) == 1 || int.Parse(coloms) == 2)
                    {
                        alltiles.Add(tile);
                    }
                    
                    z++;
                    id++;
                }
                x++;
            }
            for (int i = 0; i < rows; i++)
            {
                Tiles left_tile = Instantiate(allTypeoftiles[0], new Vector3(i, 0, -1), Quaternion.identity, Level_holder.transform).GetComponent<Tiles>();
                left_tile.OnLoaded();
                Tiles right_tile = Instantiate(allTypeoftiles[0], new Vector3(i, 0, colom), Quaternion.identity, Level_holder.transform).GetComponent<Tiles>();
                right_tile.OnLoaded();
            }
            AdjustCameraOrthographic(rows, colom);
        }
        void AdjustCameraOrthographic(int rows, int cols)
        {
            var defPos = new Vector3(6, 14f, 5f);
            mainCamera.transform.position = defPos;
            mainCamera.orthographicSize = 12;
            _camController.SetDefaultPos(defPos);
            // mainCamera.transform.position = new Vector3(6, 14f, (cols / 2f) - .5f);
            // float aspectRatio = (float)Screen.width / 1600f;
            // float halfGridHeight = rows / 2f;
            // float halfGridWidth = cols / 2f / aspectRatio;
            // mainCamera.orthographicSize = Mathf.Max(halfGridHeight, halfGridWidth);
        }
        void PlayerSpawn()
        {
            Vector3 pos;
            foreach (PlayerMoment item in Players)
            {
                pos = GetRandomPosition();
                PlayerMoment player = Instantiate(item.gameObject, pos, Quaternion.identity, player_holder).GetComponent<PlayerMoment>();
                spawnedPlayers.Add(player);
                player.OnLoaded(this);
                replayMapper.Add(player.id.ToString(), player);
                colorClashHandler.RegisterTrackable(player);

                if(player.Ai == false)
                {
                    _camController.SetPlayerTransform(player.transform);
                }
            }
        }
        void PlayerControllerSpawn()
        {
            int totalCount = temppos.Count;
            for (int i = obstacleCount; i < totalCount; i++)
            {
                temppos.RemoveAt(temppos.Count - 1);
            }
            Vector3 pos;
            for (int i = 0; i < spawnedPlayers.Count; i++)
            {
                pos = GetRandomPosition();
                temppos.Add(pos);
                spawnedPlayers[i].transform.position = pos;
                spawnedPlayers[i].transform.rotation = Quaternion.identity;
                spawnedPlayers[i].id = i;
                spawnedPlayers[i].SetCharacter(characters[i]);
            }
        }

        IEnumerator StartCountdownTimer()
        {
            countdown_text.gameObject.SetActive(true);
            countdown_text.GetComponent<AudioSource>().volume = 1;
            countdown_text.GetComponent<AudioSource>().volume *= 0.80f;
            countdown_text.GetComponent<AudioSource>().Play();

            countdown_text.text = "3";
            yield return new WaitForSeconds(countdownDelay);

            countdown_text.text = "2";
            yield return new WaitForSeconds(countdownDelay);

            countdown_text.text = "1";
            yield return new WaitForSeconds(countdownDelay);

            countdown_text.text = "Go!";

            backgroud_sound.Play();
            yield return new WaitForSeconds(countdownDelay);

            countdown_text.GetComponent<AudioSource>().volume /= 0.80f;
            countdown_text.gameObject.SetActive(false);

            StartGame();
        }
        void StartGame()
        {
            Gamestart = true;
            gameCoroutine = StartCoroutine(TimeOver());
            countdown_text.text = "";
            foreach (PlayerMoment item in spawnedPlayers)
            {
                item.OnStart();
            }
            powerupController.OnStart();
            joystick.enabled = true;
            ToggleRecording(true);
        }

        void TimerShowing()
        {
            float minutes = Mathf.FloorToInt(Timer / 60);
            float seconds = Mathf.FloorToInt(Timer % 60);
            TimeText.text = string.Format("{00:00}:{01:00}", minutes, seconds);
            TimeText1.text = string.Format("{00:00}:{01:00}", minutes, seconds);
            Timer -= Time.deltaTime;
        }
        IEnumerator TimeOver()
        {
            yield return new WaitForSeconds(Timer - 1);
            
            GameOver = true;
            ToggleRecording(false);
            Gamestart = false;
            StopCoroutine(gameCoroutine);
            gameCoroutine = null;
            backgroud_sound.Stop();
            joystick.enabled = false;
            joystick.OnPointerUp(null);
            List<PlayerMoment> sortedList = spawnedPlayers.OrderByDescending(player => player.score).ToList();
            GameOverLeaderboard leaderboard = new GameOverLeaderboard();
            foreach (PlayerMoment item in sortedList)
            {
                GameOverRank rank = new GameOverRank();
                rank.playerName = item.playercolor;
                rank.playerScore = item.score;
                rank.isPlayer = !item.Ai;
                leaderboard.lb.Add(rank);
            }
            colorClashHandler.GameOverScreen(leaderboard);
            OnReplayStart();
        }

        //Recording game events
        public void WriteAction(string _actionType, Vector3 _position, int _id, Dictionary<string, object> _data = null)
        {
            // colorClashHandler.WriteAction(_actionType, _position, _id, _data);
        }
        public void ToggleRecording(bool _toggle)
        {
            if (_toggle == true)
            {
                colorClashHandler.StartRecording();
            }
            else
            {
                colorClashHandler.StopRecording();
            }
        }
        public void OnReplayStart()
        {
            powerupController.OnPause();
            foreach (PlayerMoment item in spawnedPlayers)
            {
                item.OnWatch();
            }
            foreach (Tiles item in alltiles)
            {
                item.OnReset();
            }
            foreach (PlayerMoment item in spawnedPlayers)
            {
                item.OnReset();
            }
            UpdateGameTimerOnSimulate(0);
        }
        public void OnReplayEnd()
        {
            powerupController.OnPause();
            foreach (PlayerMoment item in spawnedPlayers)
            {
                item.OnWatch();
            }
            foreach (Tiles item in alltiles)
            {
                item.OnReset();
            }
            foreach (PlayerMoment item in spawnedPlayers)
            {
                item.OnReset();
            }
            UpdateGameTimerOnSimulate(0);
        }
        public PlayerMoment GetPlayerById(int id)
        {
            return spawnedPlayers.Find(p => p.id == id);
        }
        public Tiles GetTileById(int id)
        {
            return alltiles.Find(p => p.id == id);
        }
        public void UpdateGameTimerOnSimulate(float _time)
        {
            Timer = GameTimer - _time;
            float minutes = Mathf.FloorToInt(Timer / 60);
            float seconds = Mathf.FloorToInt(Timer % 60);
            TimeText.text = string.Format("{00:00}:{01:00}", minutes, seconds);
            TimeText1.text = string.Format("{00:00}:{01:00}", minutes, seconds);
        }
        public bool CanSave()
        {
            switch (colorClashHandler.CardState)
            {
                case CardState.START:
                case CardState.RESTART:
                case CardState.REDIRECT:
                    if(Gamestart == true && GameOver == false)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
        public void SetCameraMode(int idx)
        {
            if(!_camController) return;
            switch(idx)
            {
                case 0:
                    _camController.SetCameraMode(CameraMode.Default);
                    break;
                case 1: 
                    _camController.SetCameraMode(CameraMode.ThirdPersonFront);
                    break;
                case 2:
                    _camController.SetCameraMode(CameraMode.ThirdPerson);
                    break;
            }
        }
    }
}

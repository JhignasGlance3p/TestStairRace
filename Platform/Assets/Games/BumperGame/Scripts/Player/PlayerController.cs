using com.nampstudios.bumper.CameraUnit;
using com.nampstudios.bumper.Zone;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.nampstudios.bumper.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float playerSpeed;
        [SerializeField] private float rotateSpeed;
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private float nextTakeDamageDelay;
        [SerializeField] private float takePushDistance;
        [SerializeField] private float takePushDuration;
        [SerializeField] private Transform characterTransform;
        [SerializeField] private RuntimeAnimatorController RTAnimatorController;

        [Header("Powerups")]
        [SerializeField] private float powerupDuration;
        [SerializeField] private float boostedSpeed;
        [SerializeField] private float powerMultiplier;

        GameManager m_gameManager;
        private List<Waypoint> currentPathWayPoints = new();
        private Waypoint currentWaypoint;
        private Waypoint[] waypointsInZone;
        public int targetIndex;

        private Rigidbody rigidBody;
        private bool isInvincible = false;
        private Coroutine powerupRoutine;
        private float speed;
        private bool isMoving;
        private float timer;
        private Vector3 moveInput;
        private Vector3 newPos;
        private Vector2 input;
        private bool canMove = false;
        public Coroutine coroutine = null;//StartCoroutine

        public Transform CharacterTransform => characterTransform;
        public bool gameOver = false;

        public int hasPushed = 0;
        public List<hOrderIndex> zoneOrderIndex = null;
        private void Start()
        {
            isMoving = false;
            speed = playerSpeed;
            rigidBody = GetComponent<Rigidbody>();
            /*if (CameraManager.Instance != null)
                CameraManager.Instance.InitCamera(transform);*/
        }

        public void Initialise(GameManager _gameManager)
        {
            m_gameManager = _gameManager;
            var input = m_gameManager.Input_Manager;
            input.OnMovementInput += StoreInput;
            input.OnStopInput += OnStopInput;
            isMoving = false;
            speed = playerSpeed;
            rigidBody = GetComponent<Rigidbody>();
            weaponManager.Initialise(m_gameManager);
        }

        public void setAnimator(GameObject go)
        {
            Animator anim = go.GetComponent<Animator>();
            anim.runtimeAnimatorController = RTAnimatorController;
            playerAnimator = anim;
            InitCamera();
        }
        public void InitCamera()
        {
            if (CameraManager.Instance != null)
                CameraManager.Instance.InitCamera(transform);

            Start();
            weaponManager.on_Start();
            if (m_gameManager.isAutoPlay)
            {
                waypointsInZone = m_gameManager.Zone_Manager.getZoneOneWayPoint();
                currentWaypoint = waypointsInZone[5];
                Waypoint targetWaypoint = GetClosestWaypoint(currentWaypoint.transform.position);

                if (targetWaypoint == null)
                    return;

                var newPath = WaypointPathFinder.FindPath(GetClosestWaypoint(transform.position), targetWaypoint);

                if (newPath == null)
                {
                    Debug.LogError("Path not found from " + currentWaypoint.gameObject.name + " " + targetWaypoint.gameObject.name);
                    return;
                }
                //currentPathWayPoints = newPath;
                targetIndex = 1;
                coroutine = StartCoroutine(push());
                canMove = true;
            }
        }

        void watchMovePlayer()
        {
            Vector3 activeInput = Vector3.zero;
            //if (m_gameManager.scene_Loader.IsWatch == true)
            //{
            // Debug.LogError("[Watch ]" + m_gameManager.scene_Loader.currentIndex + ":::" + m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.foundIndex].frame);
            if (m_gameManager.scene_Loader.currentIndex < m_gameManager.scene_Loader.progressList.progressData.Count /*&& m_gameManager.scene_Loader.currentIndex == m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.foundIndex].frame*/)
            {
                activeInput = m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.currentIndex].playerProgress.input;
                if (activeInput == Vector3.zero)
                {
                    if (isMoving)
                    {
                        isMoving = false;
                        playerAnimator.SetBool(GameConstants.IS_MOVING, false);
                    }
                }
                else
                {
                    if (!isMoving)
                    {
                        isMoving = true;
                        playerAnimator.SetBool(GameConstants.IS_MOVING, true);
                    }
                }
                transform.localPosition = m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.currentIndex].playerProgress.input;
                transform.localEulerAngles = m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.currentIndex].playerProgress.rotationValue;
                int pushBar = m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.currentIndex].hasPushed;
                if (pushBar == 1)
                {
                    m_gameManager.Input_Manager.CallPush();
                }
                var zdata = m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.currentIndex].zoneData;
                if (zdata != null && zdata.zonePowerUp >= 0)
                {
                    m_gameManager.zone_PowerUp = zdata.zonePowerUp;
                }
                var zoneEnemy_Data = m_gameManager.scene_Loader.progressList.progressData[m_gameManager.scene_Loader.currentIndex].OrderIndex;

                if (zoneEnemy_Data != null && zoneEnemy_Data.Count > 0)
                {
                    //Debug.LogError($"Data Length {zoneEnemy_Data.Count}");
                    m_gameManager.savedZoneOrder = zoneEnemy_Data;
                }
                //EnemyData
                /*if (progressList.progressData[currentIndex].powerupProgress != null && progressList.progressData[currentIndex].powerupProgress.powerups != null)
                {
                    foreach (PowerupIndex powerupIndex in progressList.progressData[currentIndex].powerupProgress.powerups)
                    {
                        powerUps[powerupIndex.index].SetPowerup(powerupIndex.operation, powerupIndex.randomValue);
                    }
                }*/
                //m_gameManager.scene_Loader.foundIndex++;
            }
            m_gameManager.scene_Loader.currentIndex++;
            //}
        }
        void Update()
        {
            CheckIfGrounded();
            timer += Time.deltaTime;
        }
        void FixedUpdate()
        {
            // if (m_gameManager.scene_Loader.IsWatch)
            // {
            //     watchMovePlayer();
            //     return;
            // }
            // if (m_gameManager.isAutoPlay)
            // {
            //     if (canMove)
            //         SetMoving();
            // }
            // else
            // {
            //     MovePlayer();
            // }
        }

        #region AutoPlayPlayer
        private void SetMoving()
        {
            MoveUsingPathFinding();
            if (!isMoving)
            {
                isMoving = true;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool(GameConstants.IS_MOVING, true);
                }
            }
            /*if (moveTimer > moveDelay)
            {
                MoveUsingPathFinding();

                if (!isMoving)
                {
                    isMoving = true;
                    playerAnimator.SetBool(GameConstants.IS_MOVING, true);
                }
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;
                    playerAnimator.SetBool(GameConstants.IS_MOVING, false);
                }
            }*/
        }
        private Waypoint GetClosestWaypoint(Vector3 position)
        {
            if (waypointsInZone == null || waypointsInZone.Length == 0)
                return null;
            Waypoint closestWaypoint = null;
            float closestDistance = Mathf.Infinity;

            foreach (var waypoint in waypointsInZone)
            {
                float distance = Vector3.Distance(position, waypoint.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWaypoint = waypoint;
                }
            }

            return closestWaypoint;
        }
        private void MoveUsingPathFinding()
        {
            if (waypointsInZone == null || waypointsInZone.Length <= 0) return;

            Vector3 targetPosition;
            bool setToPlayer;
            if (waypointsInZone == null || targetIndex >= waypointsInZone.Length)
            {
                //TODO GetRandom Enemy
                targetPosition = new(waypointsInZone[targetIndex].transform.position.x, transform.position.y,
                    waypointsInZone[targetIndex].transform.position.z + 0.888f);
                setToPlayer = true;
            }
            else
            {
                targetPosition = new(waypointsInZone[targetIndex].transform.position.x, transform.position.y,
                    waypointsInZone[targetIndex].transform.position.z + 0.888f);
                setToPlayer = false;
            }
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 newPosition = transform.position + (Time.deltaTime * speed * direction);
            transform.LookAt(targetPosition);
            rigidBody.MovePosition(newPosition);

            var v1 = new Vector3(transform.position.x, 0, transform.position.z);
            var v2 = new Vector3(targetPosition.x, 0, targetPosition.z);
            var dist = Vector3.Distance(v1, v2);
            if (dist < 0.1f || setToPlayer)
            {
                UpdatePath();
            }
        }
        private void UpdatePath()
        {
            //TODO GetRandom Enemy
            //if (playerTransform != null)
            //{
            int _index = Random.Range(0, waypointsInZone.Length);
            Waypoint targetWaypoint = GetClosestWaypoint(waypointsInZone[_index].transform.position);
            if (targetWaypoint == null)
                return;

            var newPath = WaypointPathFinder.FindPath(GetClosestWaypoint(transform.position), targetWaypoint);
            if (newPath == null)
            {
                Debug.LogError("Path not found from " + currentWaypoint.gameObject.name + " " + targetWaypoint.gameObject.name);
                return;
            }
            //currentPathWayPoints = newPath;

            targetIndex = _index;
            //}
        }
        public IEnumerator push()
        {
            yield return new WaitForSeconds(1f + Random.Range(0.7f, 0.85f));
            m_gameManager.Input_Manager.CallPush();
            coroutine = StartCoroutine(push());
        }
        #endregion

        /*void LateUpdate()
        {
            if (!m_gameManager.isAutoPlay)
            {
                if (moveInput != Vector3.zero)
                {
                    m_gameManager.scene_Loader.progressList.progressData.Add(new GameProgressData(Time.frameCount - m_gameManager.scene_Loader.currentStartFrame, new PlayerProgress(transform.localPosition, transform.localEulerAngles), EnemyData, m_gameManager.Player.hasPushed, m_gameManager.scene_Loader._zoneData));
                    m_gameManager.scene_Loader._zoneData = new PlayerZoneData(-1, -1, -1);
                    EnemyData = null;
                    hasPushed = 0;
                }
                else
                {
                    m_gameManager.scene_Loader.progressList.progressData.Add(new GameProgressData(Time.frameCount - m_gameManager.scene_Loader.currentStartFrame, new PlayerProgress(transform.localPosition, transform.localEulerAngles), EnemyData, m_gameManager.Player.hasPushed, m_gameManager.scene_Loader._zoneData));
                    m_gameManager.scene_Loader._zoneData = new PlayerZoneData(-1, -1, -1);
                    EnemyData = null; 
                    hasPushed = 0;
                }
            }
        }*/
        private void OnStopInput()
        {
            if (isMoving)
            {
                hasPushed = 1;
                isMoving = false;
                playerAnimator.SetBool(GameConstants.IS_MOVING, false);
            }
        }
        private void StoreInput(Vector2 input)
        {
            this.input = input;
        }
        private void CheckIfGrounded()
        {
            if (transform.position.y < -0.35f && gameOver == false)
            {
                rigidBody.isKinematic = false;
                Invoke(nameof(TriggerGameOver), 0.25f);
            }
            if (transform.position.y < -100)
            {
                transform.position = new Vector3(transform.position.x, -100, transform.position.z);
            }
        }
        private void TriggerGameOver()
        {
            if (m_gameManager != null)
                m_gameManager.TriggerGameOver(false);
        }
        private void MovePlayer()
        {
            //TODO MovePlayer
            moveInput = new Vector3(input.x, 0, input.y).normalized;
            if (moveInput != Vector3.zero)
            {
                if (!isMoving)
                {
                    isMoving = true;
                    playerAnimator.SetBool(GameConstants.IS_MOVING, true);
                }
                newPos = transform.position + (Time.deltaTime * speed * moveInput);
                rigidBody.MovePosition(newPos);


                Quaternion targetRotation = Quaternion.LookRotation(moveInput);
                targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                if (!m_gameManager.isAutoPlay)
                {
                    m_gameManager.scene_Loader.progressList.progressData.Add(new GameProgressData(Time.frameCount - m_gameManager.scene_Loader.currentStartFrame, new PlayerProgress(transform.localPosition, transform.localEulerAngles), zoneOrderIndex, m_gameManager.Player.hasPushed, m_gameManager.scene_Loader._zoneData));
                    m_gameManager.scene_Loader._zoneData = new PlayerZoneData(-1);
                    zoneOrderIndex = null;
                    hasPushed = 0;
                }
            }
            else
            {
                if (!m_gameManager.isAutoPlay && m_gameManager.scene_Loader.progressList != null)
                { //TODO Stop                   
                    m_gameManager.scene_Loader.progressList.progressData.Add(new GameProgressData(Time.frameCount - m_gameManager.scene_Loader.currentStartFrame, new PlayerProgress(transform.localPosition, transform.localEulerAngles), zoneOrderIndex, m_gameManager.Player.hasPushed, m_gameManager.scene_Loader._zoneData));
                    m_gameManager.scene_Loader._zoneData = new PlayerZoneData(-1);
                    zoneOrderIndex = null;
                    hasPushed = 0;
                }
                if (isMoving)
                {
                    isMoving = false;
                    playerAnimator.SetBool(GameConstants.IS_MOVING, false);
                }
            }
        }
        public Transform _getTransform()
        {
            return transform;
        }
        public void TakePush(Vector3 direction, float multiplier)
        {
            if (isInvincible || timer < nextTakeDamageDelay) return;

            timer = 0f;
            direction.Normalize();

            Vector3 targetPosition = transform.position + multiplier * takePushDistance * direction;
            targetPosition.y = transform.position.y;
            StartCoroutine(SmoothMoveToPosition(targetPosition));
            playerAnimator.SetTrigger(GameConstants.FALL);
        }
        private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
        {
            float elapsedTime = 0f;
            Vector3 startingPosition = transform.position;

            while (elapsedTime < takePushDuration)
            {
                elapsedTime += Time.deltaTime;
                Vector3 newPosition = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / takePushDuration);
                rigidBody.MovePosition(newPosition);
                yield return null;
            }

            rigidBody.MovePosition(targetPosition);
            rigidBody.linearVelocity = Vector3.zero;
        }

        public void SpeedBoost()
        {
            if (powerupRoutine != null)
                StopCoroutine(powerupRoutine);
            powerupRoutine = StartCoroutine(RunPowerup(PowerupType.SpeedBoost));
        }
        public void PowerBoost()
        {
            if (powerupRoutine != null)
                StopCoroutine(powerupRoutine);
            powerupRoutine = StartCoroutine(RunPowerup(PowerupType.PowerBoost));
        }
        public void Invincibility()
        {
            if (powerupRoutine != null)
                StopCoroutine(powerupRoutine);
            powerupRoutine = StartCoroutine(RunPowerup(PowerupType.Invincibility));
        }

        private IEnumerator RunPowerup(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.SpeedBoost:
                    speed = boostedSpeed;
                    break;
                case PowerupType.PowerBoost:
                    if (weaponManager != null)
                        weaponManager.SetBoost(true, powerMultiplier);
                    break;
                case PowerupType.Invincibility:
                    isInvincible = true;
                    break;
            }

            yield return new WaitForSeconds(powerupDuration);

            speed = playerSpeed;
            if (weaponManager != null)
                weaponManager.SetBoost(false);
            isInvincible = false;
        }

        public void PlayAnimation(string name)
        {
            playerAnimator.Play(name);
        }

        private void OnDisable()
        {
            if (m_gameManager != null)
            {
                var input = m_gameManager.Input_Manager;
                input.OnMovementInput -= StoreInput;
                input.OnStopInput -= OnStopInput;
            }
            if (powerupRoutine != null)
                StopCoroutine(powerupRoutine);
        }
    }
}
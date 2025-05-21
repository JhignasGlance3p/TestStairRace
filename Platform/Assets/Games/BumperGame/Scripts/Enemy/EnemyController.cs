using com.nampstudios.bumper.Player;
using System.Collections;
using UnityEngine;
using System;
using com.nampstudios.bumper.Zone;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;

namespace com.nampstudios.bumper.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float minSpeed;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float moveDelay;
        [SerializeField] private float speedIncreasePerZone = 0.05f;
        [SerializeField] private Animator enemyAnimaor;

        [Header("Attack")]
        [SerializeField] private float enemyPushMultiplier;
        [SerializeField] private float pushInterval;

        [Header("TakeDamage")]
        [SerializeField] private int coinCountToFly;
        [SerializeField] private float takeDamageDelay = 0.5f;
        [SerializeField] private float takePushAngleOffset;
        [SerializeField] private float takePushDistanceOffset;
        [SerializeField] private int scoreForDeath;
        [SerializeField] private float takeSwingForce = 3f;
        [SerializeField] private float takePushDistance;
        [SerializeField] private float takePushDuration;

        GameManager m_gameManager;
        private Coroutine updatePathRoutine;
        private bool isMoving;
        private bool isDead;
        private bool initialized;
        private bool touchingWeapon;

        private int targetIndex;
        private float takeDamageTimer;
        private float pushTimer;
        private float moveTimer;
        private float moveSpeed;

        private Action<int> onDeathCB;
        private List<Waypoint> currentPathWayPoints = new();
        private Vector3 lastGroundPos;
        private Rigidbody rigidBody;
        private Transform playerTransform;
        private PlayerController playerController;
        private Waypoint currentWaypoint;
        private Waypoint[] waypointsInZone;
        private GameManager gameManager;
        private GenericPool<EnemyController> enemyPool;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            isDead = false;
            isMoving = false;
            touchingWeapon = false;

            pushTimer = pushInterval;
            moveTimer = moveDelay;
            takeDamageTimer = takeDamageDelay;
            enemyAnimaor.SetBool(GameConstants.IS_MOVING, false);
        }
        public void Initialize(GameManager _gameManager, Action<int> onDeathCallback, Waypoint[] wayPoints, int zoneIndex, GenericPool<EnemyController> pool, float _speed)
        {
            m_gameManager = _gameManager;
            targetIndex = 0;
            takeDamageTimer = 0f;
            pushTimer = 0f;
            moveTimer = 0f;
            moveSpeed = 0f;
            lastGroundPos = Vector3.zero;
            touchingWeapon = false;
            //////////
            waypointsInZone = wayPoints;
            onDeathCB = onDeathCallback;
            enemyPool = pool;
            gameManager = m_gameManager;
            playerController = gameManager.Player;
            if (gameManager.scene_Loader.IsWatch)
            {
                //moveSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                //moveSpeed = zoneIndex > 0 ? moveSpeed + minSpeed * speedIncreasePerZone * zoneIndex : moveSpeed;
                moveSpeed = _speed;
                currentWaypoint = GetClosestWaypoint(transform.position);
            }
            else
            {
                moveSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                moveSpeed = zoneIndex > 0 ? moveSpeed + minSpeed * speedIncreasePerZone * zoneIndex : moveSpeed;
                moveSpeed = Mathf.Clamp(moveSpeed, maxSpeed, maxSpeed * 1.5f);
                currentWaypoint = GetClosestWaypoint(transform.position);
            }
            GetPlayerReference();
            initialized = true;
        }

        public float _moveSpeed()
        {
            return moveSpeed;
        }

        public Waypoint _wayPoint()
        {
            return currentWaypoint;
        }
        public void reset()
        {
            isMoving = true;
            onPause(false);
        }
        public void onPause(bool status)
        {
            initialized = !status;
        }
        private void Update()
        {
            if (!initialized)
                return;
            if (playerTransform == null)
            {
                GetPlayerReference();
                return;
            }
            pushTimer += Time.deltaTime;
            moveTimer += Time.deltaTime;
            takeDamageTimer += Time.deltaTime;
            CheckIfGrounded();
            SetMoving();
        }

        private void SetMoving()
        {
            if (moveTimer > moveDelay)
            {
                MoveUsingPathFinding();

                if (!isMoving)
                {
                    isMoving = true;
                    enemyAnimaor.SetBool(GameConstants.IS_MOVING, true);
                }
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;
                    enemyAnimaor.SetBool(GameConstants.IS_MOVING, false);
                }
            }
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
            if (playerTransform == null) return;

            Vector3 targetPosition;
            bool setToPlayer;

            if (currentPathWayPoints == null || targetIndex >= currentPathWayPoints.Count)
            {
                targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
                setToPlayer = true;
            }
            else
            {
                targetPosition = new(currentPathWayPoints[targetIndex].transform.position.x, transform.position.y,
                    currentPathWayPoints[targetIndex].transform.position.z);
                setToPlayer = false;
            }

            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 newPosition = transform.position + (Time.deltaTime * moveSpeed * direction);
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
            if (playerTransform != null)
            {
                Waypoint targetWaypoint = GetClosestWaypoint(playerTransform.position);
                if (targetWaypoint == null)
                    return;

                var newPath = WaypointPathFinder.FindPath(GetClosestWaypoint(transform.position), targetWaypoint);
                if (newPath == null)
                {
                    Debug.Log("Path not found from " + currentWaypoint.gameObject.name + " " + targetWaypoint.gameObject.name);
                    return;
                }
                currentPathWayPoints = newPath;
                targetIndex = 0;
            }
        }

        private void CheckIfGrounded()
        {
            if (transform.position.y < -0.35f)
            {
                TriggerDeath();
            }
        }

        private void GetPlayerReference()
        {
            gameManager = m_gameManager;
            if (gameManager == null || gameManager.Player == null)
                return;
            playerController = gameManager.Player;
            playerTransform = playerController.CharacterTransform;
            lastGroundPos = transform.position;
            if (updatePathRoutine != null)
                StopCoroutine(updatePathRoutine);
            if (gameObject.activeInHierarchy)
            {
                UpdatePath();
            }
        }


        private void OnCollisionEnter(Collision other)
        {
            HandleCollision(other);
            if (other.gameObject.CompareTag(GameConstants.WALL_TAG))
            {
                Invoke(nameof(DisableColliders), 0.05f);
            }
        }

        private void OnCollisionStay(Collision other)
        {
            HandleCollision(other);
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag(GameConstants.PLAYER_TAG) || other.gameObject.CompareTag(GameConstants.WEAPON_TAG))
            {
                lastGroundPos = transform.position;
            }

            if (other.gameObject.CompareTag(GameConstants.WEAPON_TAG))
                touchingWeapon = false;
        }

        private void HandleCollision(Collision other)
        {
            if (other.gameObject.CompareTag(GameConstants.PLAYER_TAG))
            {
                if (pushTimer > pushInterval && !touchingWeapon)
                {
                    enemyAnimaor.Play(GameConstants.PUSH);
                    pushTimer = 0f;
                }
                moveTimer = 0f;
            }
            else if (other.gameObject.CompareTag(GameConstants.WEAPON_TAG))
            {
                touchingWeapon = true;
                moveTimer = 0f;
            }
        }

        private void DisableColliders()
        {
            var colliders = GetComponents<Collider>();
            foreach (var collider in colliders)
                collider.enabled = false;
        }
        public void PushPlayer()
        {
            if (playerController != null)
            {
                playerController.TakePush(transform.forward, enemyPushMultiplier);
            }
        }

        public void TakePush(Vector3 direction, float multiplier)
        {
            if (takeDamageTimer < takeDamageDelay) return;

            if (multiplier < 1)
                moveTimer = pushTimer = takeDamageTimer = 0f;

            float randomAngle = UnityEngine.Random.Range(-takePushAngleOffset, takePushAngleOffset);
            direction = Quaternion.Euler(0, randomAngle, 0) * direction;
            direction.Normalize();

            float force = (takePushDistance * multiplier) / takePushDuration;
            Vector3 velocity = direction * force;

            StartCoroutine(SmoothApplyVelocity(velocity));
        }

        private IEnumerator SmoothApplyVelocity(Vector3 targetVelocity)
        {
            float elapsedTime = 0f;
            Vector3 startingVelocity = rigidBody.linearVelocity;
            enemyAnimaor.SetTrigger(GameConstants.FALL);

            while (elapsedTime < takePushDuration)
            {
                elapsedTime += Time.deltaTime;
                Vector3 newVelocity = Vector3.Lerp(startingVelocity, targetVelocity, elapsedTime / takePushDuration);
                rigidBody.linearVelocity = newVelocity;
                yield return null;
            }
            rigidBody.linearVelocity /= 2;
            UpdatePath();
        }

        public void TakeSwing(Vector3 direction, float multiplier = 1f)
        {
            direction.y = 0;
            direction.Normalize();
            rigidBody.linearVelocity = direction * takeSwingForce * multiplier;
        }

        public void TriggerDeath()
        {
            if (!isDead)
            {
                isDead = true;
                onDeathCB.Invoke(scoreForDeath);
                if (gameManager.Ui_Manager.gameObject.activeInHierarchy)
                    gameManager.Ui_Manager.SpawnCoins(lastGroundPos, coinCountToFly);
                Invoke(nameof(DisableGameObject), 1f);
            }
        }
        public void onRestart()
        {
            if (!isDead)
            {
                isDead = true;
                DisableGameObject();
            }
        }
        private void DisableGameObject()
        {
            initialized = false;
            isMoving = false;
            targetIndex = 0;
            takeDamageTimer = 0f;
            pushTimer = 0f;
            moveTimer = 0f;
            moveSpeed = 0f;
            lastGroundPos = Vector3.zero;
            playerTransform = null;
            playerController = null;
            onDeathCB = null;
            currentWaypoint = null;
            gameManager = null;

            var colliders = GetComponents<Collider>();
            foreach (var collider in colliders)
                collider.enabled = true;

            rigidBody.linearVelocity = Vector3.zero;
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            enemyPool.ReturnItem(this);
            gameObject.SetActive(false);
            if (updatePathRoutine != null)
                StopCoroutine(updatePathRoutine);
            waypointsInZone = null;
            currentPathWayPoints?.Clear();
            enemyPool = null;
        }

        public void PlayAnimation(string name)
        {
            enemyAnimaor.Play(name);
        }
    }
}
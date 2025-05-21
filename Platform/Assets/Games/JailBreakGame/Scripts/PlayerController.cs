using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using nostra.input;
using nostra.character;
#if JAILBREAK_FUSION
using Fusion;
#endif
using System.Collections.Generic;

namespace nostra.PKPL.JailBreakGame
{
    public class PlayerController :
#if JAILBREAK_FUSION
    NetworkBehaviour
#else
    MonoBehaviour
#endif
    {
        [Header("Player Settings")]
        public NavMeshAgent agent;
        public bool isAI;
        string keyTag = "QP_Coin";
        string laserTag = "QP_Enemy";
        string policeTag = "QP_Enemy";

        [Header("State")]
        public bool hasEscaped = false;
        public bool isDead = false;
        public int keysCollected = 0;
        public int requiredKeys = 2;

        private GameObject currentKey;
        GameObject nearestPlayerWithKey;
        public GameManager gameManager;
        public string playerDoorTag
        {
            get { return gameManager.isOfflineMode ? offlineTag : doorTag; }
            set
            {
                if (gameManager.isOfflineMode)
                {
                    offlineTag = value;
                }
                else
                {
                    doorTag = value;
                }
            }
        }

#if JAILBREAK_FUSION
        [Networked]
#endif
        public string doorTag { get; set; } = "";
        public string offlineTag;
        Transform escapeDoor;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        public Collider playerCollider;

        private bool isMovingToDoor = false;

        public Animator playerAnimator;

        public GameObject holdingKeyIndicator;
        public GameObject spawnFXPrefab;
        public Transform spawnFXPosition;

        public bool canMove = false; // New variable to control player movement

#if JAILBREAK_FUSION
        public NetworkObject networkObject;
#endif

        bool isAIInitialized = false;

#if JAILBREAK_FUSION
        [Networked]
#endif
        public Vector3 moveInput { get; set; }

        public AudioSource stepAudioSource;
        public AudioClip stepClip;
        public float stepDelay = 0.5f;
        float stepTimer = 0;

        public int _Id = 0;
        public Color playerColor;

        public float policeAvoidanceRadius = 5f; // Radius within which the bot considers police too close
        public float fleeDistance = 2f; // Time in seconds the bot will flee from the police

        float m_playerSpeed = 0;
        [SerializeField] GameObject highLightGO;
        Coroutine m_HandleDeath;
        Coroutine m_Spawn;
        Coroutine m_behaviour;
        private NostraCharacter m_character;

        public void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = true;
            // Ensure the agent is active
            agent.isStopped = false;

            // Save initial position for respawning
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            escapeDoor = GameObject.FindWithTag(playerDoorTag).transform;

            GameObject spawnFX = Instantiate(spawnFXPrefab, gameManager.transform);
            foreach (var particle in spawnFX.GetComponentsInChildren<ParticleSystem>())
            {
                var main = particle.main;
                main.startColor = playerColor;
            }
            spawnFX.transform.position = spawnFXPosition.position;
            Destroy(spawnFX, 2);
        }

        public void play()
        {
            if (!gameManager.OnWatch)
            {
                if (isAI)
                {
                    if (!isAIInitialized)
                    {
                        agent.enabled = true;
                        agent.isStopped = false;
                        m_behaviour = StartCoroutine(AIBehavior());
                    }
                }
                else
                {
                    agent.enabled = true;
                    agent.isStopped = false;
                }
            }
        }
        public void SetCharacter(NostraCharacter _character)
        {
            m_character = _character;
            m_character.gameObject.SetActive(true);
            m_character.transform.SetParent(this.gameObject.transform);
            m_character.transform.localPosition = Vector3.zero;
            m_character.transform.localRotation = Quaternion.identity;
            m_character.transform.localScale = Vector3.one * 1.25f;
            if (m_character != null)
            {
                gameManager.PlayCharacterAnimation(m_character, "Playing", true);
                gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0);
            }
        }
        public void showHighLight(bool status)
        {
            highLightGO.SetActive(status);
        }
        public void SetSpeed(float _speed)
        {
            //playerAnimator.SetFloat("Speed", _speed);
            if (m_character != null)
            {
                gameManager.PlayCharacterAnimation(m_character, "walkBlend", _speed);
            }
        }
        public void PauseTheGame(bool status)
        {
            if (agent.enabled)
                agent.isStopped = status;

            if(status)
            {
                if (isAI && m_behaviour != null)
                {
                    StopCoroutine(m_behaviour);
                }
            }
            else
            {
                if (isAI && m_behaviour == null)
                {
                    m_behaviour = StartCoroutine(AIBehavior());
                }
            }
        }
        public float setSpeedToTxt()
        {
            return m_playerSpeed;
        }
#if JAILBREAK_FUSION
        public override void Spawned()
        {
            agent = GetComponent<NavMeshAgent>();

            // Ensure the agent is active
            agent.isStopped = false;

            // Save initial position for respawning
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            escapeDoor = GameObject.FindWithTag(playerDoorTag).transform;

            if (isAI)
            {
                if (gameManager.isOfflineMode || networkObject.HasStateAuthority)
                {
                    if (!isAIInitialized)
                    {
                        StartCoroutine(AIBehavior());
                    }
                }
            }


            GameObject spawnFX = Instantiate(spawnFXPrefab, gameManager.transform);
            spawnFX.transform.position = spawnFXPosition.position;
            Destroy(spawnFX, 2);

            if (!gameManager.players.Contains(this))
            {
                gameManager.players.Add(this);
            }
        }
#endif

        private void Update()
        {
            if(gameManager.OnWatch || gameManager.pauseTheGame)
            {
                return;
            }
            if (!gameManager.isOfflineMode)
            {
                //playerAnimator.SetFloat("Speed", moveInput.magnitude);

                if (isAI)
                {
                    if (m_character != null)
                    {
                        gameManager.PlayCharacterAnimation(m_character, "walkBlend", agent.velocity.magnitude / agent.speed);
                    }
                    //playerAnimator.SetFloat("Speed", agent.velocity.magnitude / agent.speed);
                }
                else
                {
                    if (m_character != null)
                    {
                        gameManager.PlayCharacterAnimation(m_character, "walkBlend", moveInput.magnitude);
                    }
                }
                return;
            }

            if (hasEscaped || !gameManager.IsRoundInProgress())
            {
                if (agent.enabled)
                    agent.isStopped = true;
                return;
            }

            if (isDead)
            {
                if (agent.enabled)
                    agent.isStopped = true;
                return;
            }
            else
            {
                if (agent.enabled)
                    agent.isStopped = !canMove; // Agent can move only if canMove is true
            }
            if (!isAI)
            {
                if (!canMove) return; // Prevent player from moving during countdown

                // Player input
                Vector2 moveInput = gameManager.GetJoystickMovement();
                Vector3 move = new Vector3(moveInput.y, 0, -moveInput.x);
                if (move.magnitude > 0.01f)
                {
                    agent.Move(move * agent.speed * Time.deltaTime);
                    transform.rotation = Quaternion.LookRotation(move);
                }
                m_playerSpeed = move.magnitude;
                //playerAnimator.SetFloat("Speed", move.magnitude);
                if (m_character != null)
                {
                    gameManager.PlayCharacterAnimation(m_character, "walkBlend", moveInput.magnitude);
                }
                float stepSpeed = Mathf.Max(0.2f, move.magnitude);
                stepTimer += stepSpeed * Time.deltaTime;
                if (stepTimer > stepDelay)
                {
                    stepAudioSource.volume = move.magnitude;
                    stepAudioSource.pitch = Random.Range(0.8f, 1.2f);
                    stepAudioSource.PlayOneShot(stepClip);
                    stepTimer = 0;
                }
            }
            else
            {
                float speed = agent.velocity.magnitude / agent.speed;
                m_playerSpeed = speed;
                //playerAnimator.SetFloat("Speed", speed);
                if (m_character != null)
                {
                    gameManager.PlayCharacterAnimation(m_character, "walkBlend", speed);
                }
                float stepSpeed = Mathf.Max(0.2f, speed);
                stepTimer += stepSpeed * Time.deltaTime;
                if (stepTimer > stepDelay)
                {
                    stepAudioSource.volume = speed;
                    stepAudioSource.pitch = Random.Range(0.8f, 1.2f);
                    stepAudioSource.PlayOneShot(stepClip);
                    stepTimer = 0;
                }
            }

        }

#if JAILBREAK_FUSION
        public override void FixedUpdateNetwork()
        {
            if (gameManager.isOfflineMode)
            {
                return;
            }
            if (!isAI)
            {
                if (GetInput(out NetworkInputData inputData))
                {
                    Vector3 move = new Vector3(inputData.direction.x, 0, inputData.direction.y);
                    moveInput = move;
                    if (moveInput.magnitude > 0f)
                    {
                        agent.Move(moveInput.normalized * agent.speed * gameManager.networkRunner.DeltaTime);
                        transform.rotation = Quaternion.LookRotation(moveInput);
                    }
                }
            }
            else
            {
                moveInput = agent.velocity;
            }
        }
#endif
        private IEnumerator AIBehavior()
        {
            isAIInitialized = true;
            while (gameManager.IsRoundInProgress() && !gameManager.pauseTheGame && !hasEscaped)
            {
                if (isDead)
                {
                    yield return null;
                    continue;
                }
                
                if (IsPoliceTooClose(out Vector3 policePosition))
                {
                    Vector3 fleeDirection = (transform.position - policePosition).normalized;
                    Vector3 fleeDestination = transform.position + fleeDirection * fleeDistance;
                    if (CheckIfPointIsValidForNavmesh(fleeDestination, out fleeDestination))
                    {
                        agent.SetDestination(fleeDestination);
                        isMovingToDoor = false;
                        yield return null;
                        continue;
                    }
                }

                if (escapeDoor != null && escapeDoor.GetComponent<PlayerDoor>().IsOpen() && !isMovingToDoor)
                {
                    // Door is open, escape
                    isMovingToDoor = true;
                    agent.SetDestination(escapeDoor.position);
                    yield return null;
                    continue;
                }
                if (keysCollected < 1)
                {
                    // Find the nearest key
                    if (currentKey == null)
                    {
                        //GameObject[] keys = GameObject.FindGameObjectsWithTag(keyTag);
                        GameObject[] keys = gameManager.m_worldManager.getActiveKeysAvailable();
                        if (keys.Length > 0)
                        {
                            GameObject nearestKey = null;
                            float minDistance = Mathf.Infinity;

                            foreach (GameObject key in keys)
                            {
                                float distance = Vector3.Distance(transform.position, key.transform.position);
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    nearestKey = key;
                                }
                            }

                            if (nearestKey != null)
                            {
                                currentKey = nearestKey;
                                nearestPlayerWithKey = null;
                            }
                        }
                        else
                        {
                            List<GameObject> playersWithKeys = new List<GameObject>();
                            foreach (var player in gameManager.players)
                            {
                                if (player.keysCollected > 0)
                                {
                                    playersWithKeys.Add(player.gameObject);
                                }
                            }
                            if (playersWithKeys.Count > 0)
                            {
                                GameObject nearestPlayer = null;
                                float minDistance = Mathf.Infinity;

                                foreach (var player in playersWithKeys)
                                {
                                    float distance = Vector3.Distance(transform.position, player.transform.position);
                                    if (distance < minDistance)
                                    {
                                        minDistance = distance;
                                        nearestPlayer = player;
                                    }
                                }

                                if (nearestPlayer != null)
                                {
                                    nearestPlayerWithKey = nearestPlayer;
                                    currentKey = null;
                                }
                            }
                        }
                    }

                    if (currentKey != null)
                    {
                        isMovingToDoor = false;
                        agent.SetDestination(currentKey.transform.position);
                    }
                    else if (nearestPlayerWithKey != null)
                    {
                        isMovingToDoor = false;
                        agent.SetDestination(nearestPlayerWithKey.transform.position);
                    }
                    else
                    {
                        // No keys available, wait
                        yield return new WaitForSeconds(0.5f);
                        continue;
                    }
                }
                else
                {
                    // Move towards the escape door
                    if (escapeDoor != null && !isMovingToDoor)
                    {
                        isMovingToDoor = true;
                        agent.SetDestination(escapeDoor.position);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
            isAIInitialized = false;
        }

        private bool IsPoliceTooClose(out Vector3 policePosition)
        {
            float closestDistance = Mathf.Infinity;
            policePosition = Vector3.zero;
            foreach (var police in gameManager.m_worldManager.policeControllers)
            {
                if (true)
                {
                    float distanceToPolice = Vector3.Distance(transform.position, police.transform.position);
                    if (distanceToPolice < policeAvoidanceRadius && distanceToPolice < closestDistance)
                    {
                        closestDistance = distanceToPolice;
                        policePosition = police.transform.position;
                    }
                }
            }
            return closestDistance < Mathf.Infinity;
        }

        bool CheckIfPointIsValidForNavmesh(Vector3 point, out Vector3 result)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, 1f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
            result = point;
            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead) return;

            if (other.CompareTag(keyTag) && keysCollected < 1)
            {
                if (gameManager.isOfflineMode)
                {
                    gameManager.m_audioManager.PlayClip(gameManager.m_audioManager.keyPicked);
                    CollectKey(other.gameObject);
                }
#if JAILBREAK_FUSION
                else if (Object.HasStateAuthority)
                {
                    NetworkObject keyNetworkObject = other.GetComponent<NetworkObject>();
                    if (keyNetworkObject != null)
                    {
                        RPC_CollectKey(keyNetworkObject.Id);
                    }
                }
#endif
            }
            else if (other.gameObject == escapeDoor.gameObject)
            {
                if (keysCollected >= 1)
                {
                    if (gameManager.isOfflineMode)
                    {
                        UseKeyAtDoor();
                    }
#if JAILBREAK_FUSION
                    else if (Object.HasStateAuthority)
                    {
                        RPC_UseKeyAtDoor();
                    }
#endif
                }
                else if (escapeDoor.GetComponent<PlayerDoor>().IsOpen())
                {
                    if (gameManager.isOfflineMode)
                    {
                        EscapeThroughDoor();
                    }
#if JAILBREAK_FUSION
                    else if (Object.HasStateAuthority)
                    {
                        RPC_EscapeThroughDoor();
                    }
#endif
                }
            }
            else if (other.CompareTag(policeTag))
            {
                // Collided with police
                if (gameManager.isOfflineMode
#if JAILBREAK_FUSION
                || networkObject.HasStateAuthority
#endif
                )
                {
                    m_HandleDeath = StartCoroutine(HandleDeath());
                }
            }
            else if (other.CompareTag(laserTag))
            {
                // Hit by laser
                if (gameManager.isOfflineMode
#if JAILBREAK_FUSION
                || networkObject.HasStateAuthority
#endif
                )
                {
                    m_HandleDeath = StartCoroutine(HandleDeath());
                }
            }
        }

        private void UseKeyAtDoor()
        {
            keysCollected--;
            holdingKeyIndicator.SetActive(false);
            PlayerDoor door = escapeDoor.GetComponent<PlayerDoor>();
            if (door != null)
            {
                door.IncreaseKeyCount();
                if (door.IsOpen())
                {
                    EscapeThroughDoor();
                    gameManager.m_audioManager.PlayClip(gameManager.m_audioManager.escaped);
                }
                else
                {
                    gameManager.m_audioManager.PlayClip(gameManager.m_audioManager.keyUsed);
                }
            }

            gameManager.m_worldManager.OnPlayerUsedKey();
        }

        private void EscapeThroughDoor()
        {
            hasEscaped = true;
            gameManager.PlayerEscaped(this);
            gameObject.SetActive(false);
        }

#if JAILBREAK_FUSION
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
#endif
        public void RPC_UseKeyAtDoor()
        {
            UseKeyAtDoor();
        }

#if JAILBREAK_FUSION
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
#endif
        public void RPC_EscapeThroughDoor()
        {
            EscapeThroughDoor();
        }

        private void CollectKey(GameObject keyObject)
        {
            keysCollected++;
            holdingKeyIndicator.SetActive(true);

            Transform keySpawnPoint = keyObject.transform.parent;

            gameManager.m_worldManager.OnKeyPickedUp(keyObject, keySpawnPoint);

            currentKey = null;
        }

#if JAILBREAK_FUSION
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CollectKey(NetworkId keyNetworkId)
        {
            CollectKeyLogic(keyNetworkId);
        }

        private void CollectKeyLogic(NetworkId keyNetworkId)
        {
            keysCollected++;
            holdingKeyIndicator.SetActive(true);

            NetworkObject keyNetworkObject = Runner.FindObject(keyNetworkId);
            if (keyNetworkObject != null)
            {
                GameObject keyObject = keyNetworkObject.gameObject;
                Transform keySpawnPoint = keyObject.transform.parent;

                 gameManager.m_worldManager.OnKeyPickedUp(keyObject, keySpawnPoint);
                currentKey = null;
            }
        }
#endif

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // Steal key from other player
                PlayerController otherPlayer = other.gameObject.GetComponent<PlayerController>();
                if (otherPlayer != null && otherPlayer.keysCollected > 0 && keysCollected < 1)
                {
                    otherPlayer.keysCollected--;
                    otherPlayer.holdingKeyIndicator.SetActive(false);
                    keysCollected++;
                    holdingKeyIndicator.SetActive(true);
                }
            }
        }

        public IEnumerator HandleDeath()
        {
            if (isDead) yield break; // Prevent multiple deaths

            if (gameManager.isOfflineMode)
            {
                HandleDeathLogic();
            }
#if JAILBREAK_FUSION
            else if (Object.HasStateAuthority)
            {
                RPC_HandleDeath();
            }
#endif
            yield return null;
        }

        private void HandleDeathLogic()
        {
            gameManager.m_audioManager.PlayClip(gameManager.m_audioManager.dead);
            isDead = true;
            //playerAnimator.SetTrigger("Die");
            if (m_character != null)
            {
                gameManager.PlayCharacterAnimation(m_character, "die", true);
            }
            playerCollider.enabled = false;
            agent.enabled = false;
            currentKey = null;
            nearestPlayerWithKey = null;

            if (keysCollected > 0)
            {
                keysCollected = 0;
                holdingKeyIndicator.SetActive(false);
                gameManager.m_worldManager.OnPlayerLostKey();
            }

            gameManager.m_worldManager.OnLaserEvent();

           m_Spawn = StartCoroutine(RespawnRoutine());
        }

#if JAILBREAK_FUSION
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
#endif
        public void RPC_HandleDeath()
        {
            HandleDeathLogic();
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(4f);
            transform.position = initialPosition;
            transform.rotation = initialRotation;

            //playerAnimator.SetTrigger("Walk");
            if (m_character != null)
            {
                gameManager.PlayCharacterAnimation(m_character, "Playing", true);
            }
            playerCollider.enabled = true;
            agent.enabled = true;
            isDead = false;
            var spawnFX = Instantiate(spawnFXPrefab, gameManager.transform);
            foreach (var particle in spawnFX.GetComponentsInChildren<ParticleSystem>())
            {
                var main = particle.main;
                main.startColor = playerColor;
            }
            spawnFX.transform.position = spawnFXPosition.position;
            Destroy(spawnFX, 2);
        }

        public void ResetPlayer()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            //agent.isStopped = true;
            agent.enabled = false;
            currentKey = null;
            playerCollider.enabled = true;
            isDead = false;
            keysCollected = 0;
            hasEscaped = false;
            isAIInitialized = false;
            holdingKeyIndicator.SetActive(false);
            //playerAnimator.SetTrigger("Walk");
            if (m_character != null)
            {
                gameManager.PlayCharacterAnimation(m_character, "Playing", true);
                gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0);
            }
            //playerAnimator.SetFloat("Speed", 0f);
            
            if(m_behaviour != null)
            {
                StopCoroutine(m_behaviour);
            }
            if(m_HandleDeath != null)
            {
                StopCoroutine(m_HandleDeath);
            }
            if(m_Spawn != null)
            {
                StopCoroutine(m_Spawn);
            }
        }
    }
}

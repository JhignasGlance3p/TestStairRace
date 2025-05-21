using nostra.character;
using nostra.quickplay.core.Recorder;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace nostra.pkpl.stairrace
{
    public class IPlayer : MonoBehaviour, ITrackable, IReconstructable
    {
        [SerializeField] protected Transform backTransform;
        [SerializeField] protected int stairsToCollectMin;
        [SerializeField] protected int stairsToCollectMax;
        [SerializeField] protected float rotationSpeed;
        [SerializeField] protected float dropRadius = 0.2f;
        [SerializeField] protected float hitDelay = 0.25f;
        [SerializeField] protected float fallAnimTime = 1.5f;
        [SerializeField] protected int maxBlocksCanHold = 15;

        protected StairRaceManager m_gameManager;
        protected UnityEngine.AI.NavMeshAgent navMeshAgent;
        protected NostraCharacter nostracharacter = null;
        protected SRPlayerProgress playerProgress;
        protected List<CollectibleBlock> currentTargets;
        protected CollectibleBlock currentTarget;
        protected BridgeController currentBridge;
        protected State currentState;
        protected int stairsToCollect;
        protected int m_platformIndex;
        protected int m_bridgeIndex;
        protected bool canStart = false;
        protected Stack<CollectibleBlock> collectedBricks = new();
        protected bool canCollide;
        protected ColorName color;
        protected int collectedStairCount;
        protected bool playerControl = false;
        protected float timer;
        protected Coroutine fallRoutine;
        protected float originalSpeed;
        protected int m_playerIndex;
        protected float m_animSpeed = 0f;

        public bool CanCollide { get { return canCollide; } }
        public ColorName Color { get { return color; } }
        public int CollectedStairCount { get { return collectedStairCount; } }
        public Transform BackTransform => collectedBricks.Count == 0 ? backTransform : collectedBricks.Peek().transform;
        public int NumberOfPickupsPossible
        {
            get
            {
                if (IsPlayer)
                {
                    return maxBlocksCanHold - CollectedStairCount;
                }
                else
                {
                    return stairsToCollect - CollectedStairCount;
                }
            }
        }
        public SRPlayerProgress SaveProgress => playerProgress;
        public int PlayerIndex => m_playerIndex;
        public int platformIndex => m_platformIndex;

        SRPlayerData playerData;
        bool isWatching = false;

        public virtual bool IsPlayer
        {
            get
            {
                return false;
            }
        }
        public virtual bool CanCollectBlock
        {
            get
            {
                return false;
            }
        }

        public IGameObjectState CaptureState ()
        {
            isWatching = false;
            if(playerData == null)
            {
                playerData = new SRPlayerData();
                playerData.playerIndex = m_playerIndex;
                playerData.color = color;
                playerData.currentPosition = new SerializableVector3(this.transform.position);
                playerData.curentRotation = new SerializableVector3(this.transform.localEulerAngles);
                playerData.animSpeed = m_animSpeed;
            }
            else
            {
                if(playerData.currentPosition.ToVector3() == new SerializableVector3(this.transform.position).ToVector3() && playerData.curentRotation.ToVector3() == new SerializableVector3(this.transform.localEulerAngles).ToVector3() && playerData.animSpeed == m_animSpeed)
                {
                    playerData.canCapture = true;
                }
                else
                {
                    playerData.canCapture = true;
                    playerData.currentPosition = new SerializableVector3(this.transform.position);
                    playerData.curentRotation = new SerializableVector3(this.transform.localEulerAngles);
                    playerData.animSpeed = m_animSpeed;
                }
            }
            return playerData;
        }
        public void ApplyState(IGameObjectState _state)
        {
            isWatching = true;
            if (_state is SRPlayerData playerData)
            {
                this.transform.position = playerData.currentPosition.ToVector3();
                this.transform.localEulerAngles = playerData.curentRotation.ToVector3();
                if (nostracharacter != null)
                {
                    m_gameManager.PlayAnimation(nostracharacter, "walkBlend", playerData.animSpeed);
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // if (other.gameObject.CompareTag(GameConstants.PLAYER_TAG))
            // {
            //     IPlayer player = other.gameObject.GetComponent<IPlayer>();
            //     if (this.IsPlayer == false && player.IsPlayer == false)
            //     {
            //         return;
            //     }
            //     if (player != null && timer >= hitDelay)
            //     {
            //         if (collectedStairCount <= 0 || player.CollectedStairCount <= 0 || !player.CanCollide)
            //             return;
            //         timer = 0;
            //         if (player.CollectedStairCount > collectedStairCount)
            //             TakeAttack();
            //         else
            //             player.TakeAttack();
            //     }
            // }
        }
        protected virtual void Update()
        {
            if (isWatching) return;
            if (canStart == false)
            {
                if (currentState != State.Idle)
                    SwitchState(State.Idle);
                return;
            }
            timer += Time.deltaTime;
            canCollide = currentBridge == null || transform.position.z < currentBridge.StairsOnBridge[0].transform.position.z;
            if (playerControl == false)
            {
                UpdateRotationManually();
            }
            if (canSave() == true && playerProgress != null)
            {
                playerProgress.currentPosition = new SerializableVector3(this.transform.position);
                playerProgress.curentRotation = new SerializableVector3(this.transform.localEulerAngles);
            }
        }
        public virtual void OnFocussed()
        {
            navMeshAgent.enabled = true;
            navMeshAgent.updateRotation = false;
            canStart = true;
            canCollide = true;
            playerControl = false;
            isWatching = false;
            timer = 0f;
            SwitchState(State.Collecting);
        }
        public virtual void OnWatchFocus()
        {
            navMeshAgent.enabled = false;
            navMeshAgent.updateRotation = false;
            canStart = false;
            canCollide = false;
            playerControl = false;
            isWatching = true;
        }
        public virtual void Reset()
        {
            SetPlayer(ColorName.None);
            canStart = false;
            canCollide = false;
            playerControl = false;
            collectedStairCount = 0;
            foreach (var brick in collectedBricks)
            {
                brick.ResetBrick();
            }
            collectedBricks.Clear();
            currentBridge = null;
            currentTarget = null;
            currentTargets = null;
            m_platformIndex = 0;
            m_playerIndex = 0;
            m_bridgeIndex = -1;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(transform.position);
            }
            navMeshAgent.updateRotation = false;
            navMeshAgent.enabled = false;
            playerData = null;
        }
        public virtual void OnReplayEnd()
        {
        }
        public void SetCharacter(NostraCharacter _character)
        {
            nostracharacter = _character;
            nostracharacter.transform.SetParent(this.transform);
            nostracharacter.transform.localPosition = Vector3.zero;
            nostracharacter.transform.localEulerAngles = Vector3.zero;
            nostracharacter.transform.localScale = Vector3.one * 1.25f;
            nostracharacter.gameObject.SetActive(true);
            m_gameManager.PlayAnimation(nostracharacter, "Playing", true);
        }
        public void SetProgress(SRPlayerProgress _playerProgress)
        {
            playerProgress = _playerProgress;
            SetPlayer(playerProgress.color);
            this.gameObject.transform.position = playerProgress.currentPosition.ToVector3();
            this.gameObject.transform.localEulerAngles = playerProgress.curentRotation.ToVector3();
            m_platformIndex = playerProgress.currentPlatformIndex;
            m_playerIndex = playerProgress.playerIndex;
            if (playerProgress.currentBridgeIndex != m_bridgeIndex && playerProgress.currentBridgeIndex >= 0)
            {
                m_bridgeIndex = playerProgress.currentBridgeIndex;
                currentBridge = m_gameManager.PlatformsManager.GetPlatform(m_platformIndex).BridgesOnPlatfom[playerProgress.currentBridgeIndex];
            }
            if (collectedStairCount != playerProgress.blockInHand)
            {
                if (collectedStairCount < playerProgress.blockInHand)
                {
                    for (int i = collectedStairCount; i < playerProgress.collectedBlocks.Count; i++)
                    {
                        CollectibleBlock _block = null;
                        if (playerProgress.collectedBlocks[i].isPowerupBlock == false)
                        {
                            PlatformManager _platform = m_gameManager.PlatformsManager.GetPlatform(playerProgress.collectedBlocks[i].platformIndex);
                            if (_platform != null)
                            {
                                _block = _platform.BlocksOnPlatform[playerProgress.collectedBlocks[i].blockIndex];
                                if (playerProgress.collectedBlocks[i].isPowerupBlock == false)
                                {
                                    _block = _platform.BlocksOnPlatform[playerProgress.collectedBlocks[i].blockIndex];
                                }
                            }
                        }
                        else
                        {
                            CollectibleBlock instance = m_gameManager.StairTypes[0].Prefab;
                            _block = Instantiate(instance, transform.position, Quaternion.identity, null);
                            _block.gameObject.SetActive(true);
                            _block.OnLoaded(m_gameManager, m_platformIndex, playerProgress.collectedBlocks[i].blockIndex);
                            _block.Reset(playerProgress.color);
                            _block.HandleCollection(this, true);
                        }
                        if (_block != null)
                        {
                            _block.SetCollected();
                            collectedBricks.Push(_block);
                            _block.transform.SetParent(backTransform);
                            float3 newPosition = new Vector3(0, collectedBricks.Count * (m_gameManager.CollectibleStairDimensions.y / transform.localScale.y), 0);
                            _block.transform.localPosition = newPosition;
                            _block.transform.rotation = backTransform.rotation;

                        }
                    }
                }
                else
                {
                    for (int i = collectedStairCount; i > playerProgress.collectedBlocks.Count; i--)
                    {
                        var item = collectedBricks.Pop();
                        collectedStairCount--;
                        item.ResetBrick();
                    }
                }
                collectedStairCount = playerProgress.blockInHand;
            }
            if (currentState != playerProgress.currentState)
            {
                SwitchState(playerProgress.currentState);
            }
        }
        public virtual void OnStart() { }
        public virtual void OnRestart()
        {
            OnStart();
        }
        public virtual void OnHidden()
        {
            nostracharacter = null;
        }
        public virtual void OnGameOver()
        {
            canStart = false;
            canCollide = false;
            playerControl = false;
            collectedStairCount = 0;
            foreach (var brick in collectedBricks)
            {
                brick.ResetBrick();
            }
            collectedBricks.Clear();
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(transform.position);
            }
            navMeshAgent.updateRotation = false;
            navMeshAgent.enabled = false;
            m_animSpeed = 0f;
            m_gameManager.PlayAnimation(nostracharacter, "walkBlend", m_animSpeed);
        }

        public virtual void IncreaseCollectedBlocks(int _platformIndex, int _blockIndex, bool _isPowerup)
        {
            if (canStart == false)
            {
                return;
            }
            collectedStairCount++;
            if (canSave() == true && playerProgress != null)
            {
                SRBlockProgress collectedBlock = new();
                collectedBlock.platformIndex = _platformIndex;
                collectedBlock.blockIndex = _blockIndex;
                collectedBlock.isPowerupBlock = _isPowerup;
                playerProgress.blockInHand = CollectedStairCount;
                playerProgress.collectedBlocks.Add(collectedBlock);
            }
        }
        public virtual void CollectBrick(CollectibleBlock brick)
        {
            if (canStart == false)
            {
                return;
            }
            collectedBricks.Push(brick);
            brick.transform.SetParent(backTransform);
            float3 newPosition = new Vector3(0, collectedBricks.Count * (m_gameManager.CollectibleStairDimensions.y / transform.localScale.y), 0);
            brick.transform.localPosition = newPosition;
            brick.transform.rotation = backTransform.rotation;
            if (playerControl == false)
            {
                if (currentState == State.Collecting)
                {
                    SwitchState(State.Collecting);
                }
            }
        }
        public virtual void PlacedBrickOnStair(BridgeController bridgeCont, int currentBridgeIndex, int stairIndex, bool isFirstIndex, bool isLastIndex)
        {
            if (canStart == false)
            {
                return;
            }
            if (canSave() == true && playerProgress != null)
            {
                playerProgress.currentStairIndex = stairIndex;
            }
            ReduceBrick(isLastIndex);
            if (isFirstIndex)
            {
                OnEnterBridge(bridgeCont, currentBridgeIndex);
            }
            if (isLastIndex)
            {
                MoveToNextPlatForm();
            }
        }
        public virtual void ReduceBrick(bool isLastIndex)
        {
            if (canStart == false)
            {
                return;
            }
            if (playerControl == false && collectedBricks.Count == 0)
            {
                SwitchState(State.Collecting);
                return;
            }

            var item = collectedBricks.Pop();
            collectedStairCount--;
            item.ResetBrick();
            if (canSave())
            {
                playerProgress.blockInHand = CollectedStairCount;
                if (playerProgress.collectedBlocks.Count > 0)
                    playerProgress.collectedBlocks.RemoveAt(playerProgress.collectedBlocks.Count - 1);
            }
            if (playerControl == false)
            {
                if (isLastIndex == false && currentState == State.Building && collectedBricks.Count == 0)
                {
                    SwitchState(State.Collecting);
                }
            }
        }
        public virtual void TakeAttack()
        {
            if (canStart == false)
            {
                return;
            }
            timer = 0f;
            // playerAnimator.Play(GameConstants.FALL);
            foreach (var stair in collectedBricks)
            {
                stair.transform.parent = null;
                float3 randomPosition = transform.position + new Vector3(UnityEngine.Random.Range(-dropRadius, dropRadius), 0,
                    UnityEngine.Random.Range(-dropRadius, dropRadius));
                stair.FallDown(randomPosition, this);
            }
            foreach (var brick in collectedBricks)
            {
                brick.ResetBrick();
            }
            collectedBricks.Clear();
            collectedStairCount = 0;
            if (canSave())
            {
                playerProgress.blockInHand = CollectedStairCount;
                if (playerProgress.collectedBlocks.Count > 0)
                    playerProgress.collectedBlocks.Clear();
                playerProgress.collectedBlocks = new();
            }
            FallAndRecover();
        }
        public virtual void CollectPowerup(SRPowerupData _powerup, PowerupController _powerupController)
        {
            if (canStart == false)
            {
                return;
            }
            if (_powerup.operation == 0 || _powerup.operation == 2)
            {
                _powerupController.ApplyPowerup(this);
            }
            else
            {
                int toremove = 0;
                if (_powerup.operation == 1)
                {
                    toremove = _powerup.randomValue;
                }
                else if (_powerup.operation == 3)
                {
                    toremove = Mathf.RoundToInt(collectedBricks.Count / _powerup.randomValue);
                }
                toremove = Mathf.Min(toremove, collectedBricks.Count);
                for (int i = 0; i < toremove; i++)
                {
                    ReduceBrick(false);
                }
            }
        }
        public virtual void OnMoveEvent(Vector2 _direction)
        {
        }

        protected void OnEnterBridge(BridgeController bridgeCont, int currentBridgeIndex)
        {
            currentBridge = bridgeCont;
            if (canSave() == true && playerProgress != null)
            {
                playerProgress.currentBridgeIndex = currentBridgeIndex;
            }
        }
        protected void MoveToNextPlatForm()
        {
            SwitchState(State.MovingToNextPlatform);
        }
        protected virtual void SetPlayer(ColorName _color)
        {
            color = _color;
            timer = hitDelay;
        }
        protected void SwitchState(State newState)
        {
            if (canStart == false)
            {
                return;
            }
            currentState = newState;
            if (canSave() == true)
            {
                playerProgress.currentState = currentState;
            }
            switch (currentState)
            {
                case State.Idle:
                    HandleIdleState();
                    break;
                case State.Collecting:
                    HandleCollectingState();
                    break;
                case State.Building:
                    HandleBuildingState();
                    break;
                case State.MovingToNextPlatform:
                    HandleMovingToNextPlatformState();
                    break;
                case State.MoveToFinalEntrance:
                    HandleMoveToFinalEntrance();
                    break;
            }
        }
        protected void HandleIdleState()
        {
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(transform.position);
            }
            m_animSpeed = 0f;
            m_gameManager.PlayAnimation(nostracharacter, "walkBlend", m_animSpeed);
        }
        protected void UpdateRotationManually()
        {
            Vector3 direction = navMeshAgent.steeringTarget - transform.position;
            direction.y = 0;
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        protected void HandleCollectingState()
        {
            if (currentTargets == null || currentTargets.Count == 0)
            {
                var currentPlatfrom = m_gameManager.PlatformsManager.GetPlatform(m_platformIndex);
                if (currentPlatfrom == null)
                {
                    Debug.LogError("Couldn't retrieve Platform");
                    return;
                }
                currentTargets = currentPlatfrom.GetTargetBlocks(Color);
                stairsToCollect = UnityEngine.Random.Range(stairsToCollectMin, stairsToCollectMax);
            }

            if (currentTargets != null && currentTargets.Count > 0)
            {
                currentTarget = currentTargets.Find(item => item.CanCollect(this));
                if (currentTarget == null)
                {
                    SwitchState(State.Building);
                    Debug.LogError("Couldn't find a free target " + this.gameObject.name);
                    return;
                }
                if (navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.SetDestination(currentTarget.transform.position);
                }
                m_animSpeed = 1f;
                m_gameManager.PlayAnimation(nostracharacter, "walkBlend", m_animSpeed);
                if (collectedBricks.Count >= stairsToCollect)
                {
                    SwitchState(State.Building);
                }
            }
        }
        protected void HandleBuildingState()
        {
            if (currentBridge == null)
            {
                ChooseFreeOrRandomBridge();
            }
            if (currentBridge != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(currentBridge.StairsOnBridge[^1].transform.position);
            }
            m_animSpeed = 1f;
            m_gameManager.PlayAnimation(nostracharacter, "walkBlend", m_animSpeed);
        }
        protected void HandleMovingToNextPlatformState()
        {
            currentBridge = null;
            currentTarget = null;
            currentTargets = null;
            m_platformIndex++;
            if (m_platformIndex == m_gameManager.PlatformsManager.NumberOfPlatforms)
            {
                SwitchState(State.MoveToFinalEntrance);
                return;
            }
            if (canSave() == true && playerProgress != null)
            {
                playerProgress.currentPlatformIndex = m_platformIndex;
            }
            SwitchState(State.Collecting);
        }
        protected void HandleMoveToFinalEntrance()
        {
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(m_gameManager.PlatformsManager.winPlatform.target.position);
            }
        }
        protected void ChooseFreeOrRandomBridge()
        {
            var currentPlatfrom = m_gameManager.PlatformsManager.GetPlatform(m_platformIndex);
            var freeBridge = currentPlatfrom.BridgesOnPlatfom.Find(bridge => bridge.IsBridgeOccupied == false);
            if (freeBridge != null)
            {
                currentBridge = freeBridge;
                m_bridgeIndex = freeBridge.BridgeIndex;
            }
            else
            {
                m_bridgeIndex = UnityEngine.Random.Range(0, currentPlatfrom.BridgesOnPlatfom.Count);
                currentBridge = currentPlatfrom.BridgesOnPlatfom[m_bridgeIndex];
            }
            if (canSave() == true && playerProgress != null)
            {
                playerProgress.currentBridgeIndex = m_bridgeIndex;
            }
            currentBridge.ClaimBridge(Color);
        }
        protected virtual void FallAndRecover() { }

        private bool canSave()
        {
            return m_gameManager.CanSave();
        }
    }
}
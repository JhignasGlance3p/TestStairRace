using nostra.quickplay.core.Recorder;
using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

namespace nostra.pkpl.stairrace
{
    public class CollectibleBlock : MonoBehaviour, ITrackable, IReconstructable
    {
        [SerializeField] private ColorName color;
        [SerializeField] private float speed;
        [SerializeField] private float yOffset;
        [SerializeField] private float3 colliderSizeAfterFall;
        [SerializeField] private float3 colliderSizeOriginal;
        [SerializeField] private TrailRenderer[] trails;

        private StairRaceManager m_GameManager;
        private int blockIndex;
        private int platformIndex;
        private bool collected;
        private float3 spawnPosition;
        private Quaternion spawnRotation;
        private Transform parent;
        private bool powerupAffected = false;
        private MeshRenderer mesh;
        private BoxCollider triggerCollider;
        private Coroutine collectRoutine;
        private int collapsedOwnerIndex = -1;

        public ColorName BlockColor => color;
        public int BlockIndex => blockIndex;
        public int PlatformIndex => platformIndex;

        SRCollectibleData m_CollectibleData;
        int ownerIndex = -1;
        bool isWatching = false;

        public IGameObjectState CaptureState ()
        {
            isWatching = false;
            if(m_CollectibleData == null)
            {
                m_CollectibleData = new SRCollectibleData();
                m_CollectibleData.Id = (platformIndex + "_" + blockIndex);
                m_CollectibleData.currentPlayerIndex = ownerIndex;
                m_CollectibleData.currentPosition = new SerializableVector3(this.transform.position);
                m_CollectibleData.curentRotation = new SerializableVector3(this.transform.localEulerAngles);
                m_CollectibleData.color = color;
                m_CollectibleData.canCapture = true;
                m_CollectibleData.isPowerup = powerupAffected;
            }
            else
            {
                if(m_CollectibleData.currentPlayerIndex != ownerIndex)
                {
                    m_CollectibleData.currentPlayerIndex = ownerIndex;
                    m_CollectibleData.canCapture = true;
                }
                else
                {
                    m_CollectibleData.canCapture = false;
                }
                m_CollectibleData.currentPosition = new SerializableVector3(this.transform.position);
                m_CollectibleData.curentRotation = new SerializableVector3(this.transform.localEulerAngles);
                m_CollectibleData.color = color;
            }
            return m_CollectibleData;
        }
        public void ApplyState(IGameObjectState _state)
        {
            isWatching = true;
            if (_state is SRCollectibleData collectibleData)
            {
                ownerIndex = collectibleData.currentPlayerIndex;
                color = collectibleData.color;
                mesh.material = m_GameManager.GetMaterial(color);
                if(ownerIndex < m_GameManager.Players.Count && ownerIndex >= 0)
                {
                    this.transform.SetParent(m_GameManager.Players[ownerIndex].BackTransform);
                    this.transform.position = collectibleData.currentPosition.ToVector3();
                    this.transform.localPosition = new Vector3(0, this.transform.localPosition.y, 0);
                    this.transform.rotation = m_GameManager.Players[ownerIndex].BackTransform.rotation;
                }
                else
                {
                    this.transform.SetParent(parent);
                    this.transform.position = spawnPosition;
                    this.transform.rotation = spawnRotation;
                    if(collectibleData.isPowerup)
                    {
                        m_GameManager.PowerupBlocks.Add(this);
                        this.gameObject.SetActive(false);
                    }
                }
                triggerCollider.size = colliderSizeOriginal;
                triggerCollider.enabled = false;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (collected || isWatching) return;
            if (other.CompareTag(GameConstants.PLAYER_TAG))
            {
                var controller = other.GetComponent<IPlayer>();
                if (controller != null)
                {
                    if (controller.PlayerIndex == collapsedOwnerIndex)
                        return;

                    if (controller.Color == color || color == ColorName.Grey)
                    {
                        if (controller.CanCollectBlock == false)
                            return;

                        if (color == ColorName.Grey)
                        {
                            color = controller.Color;
                            mesh.material = m_GameManager.GetMaterial(color);
                        }
                        triggerCollider.enabled = false;
                        collected = true;
                        collectRoutine = StartCoroutine(MoveToPlayer(controller, true));
                    }
                }
            }
        }

        public void OnLoaded(StairRaceManager _gameManager, int _platformIndex, int _index, bool _isPowerup = false)
        {
            m_GameManager = _gameManager;
            platformIndex = _platformIndex;
            blockIndex = _index;
            ownerIndex = -1;
            isWatching = false;
            powerupAffected = _isPowerup;

            mesh = GetComponent<MeshRenderer>();
            parent = transform.parent;
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            triggerCollider = GetComponent<BoxCollider>();

            Reset(ColorName.None);
            _gameManager.RegisterBlock(this);
        }
        public void Reset(ColorName _color)
        {
            if (collectRoutine != null)
            {
                StopCoroutine(collectRoutine);
                collectRoutine = null;
            }
            collected = false;
            color = _color;
            collapsedOwnerIndex = -1;
            ownerIndex = -1;
            isWatching = false;
            mesh.material = m_GameManager.GetMaterial(color);
            triggerCollider.size = colliderSizeOriginal;
            triggerCollider.enabled = true;
            foreach (var trail in trails)
            {
                trail.enabled = false;
            }

            this.transform.SetParent(parent);
            this.transform.position = spawnPosition;
            this.transform.rotation = spawnRotation;
            m_CollectibleData = null;
        }
        public bool CanCollect(IPlayer player)
        {
            return (color == player.Color || color == ColorName.Grey) && !collected && (collapsedOwnerIndex == -1 ||
           collapsedOwnerIndex != player.PlayerIndex);
        }
        public void FallDown(float3 positon, IPlayer player)
        {
            positon.y = spawnPosition.y + 0.01f;
            transform.SetParent(parent);
            StartCoroutine(FallToGround(positon));
            collapsedOwnerIndex = player.PlayerIndex;
        }
        public void HandleCollection(IPlayer _player, bool _watch = false)
        {
            triggerCollider.enabled = false;
            collected = true;
            powerupAffected = true;
            if (_watch == false)
                collectRoutine = StartCoroutine(MoveToPlayer(_player, false, true));
        }
        public void ResetBrick()
        {
            transform.SetParent(parent);
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            color = color;
            triggerCollider.enabled = true;
            collected = false;
            ownerIndex = -1;
            collapsedOwnerIndex = -1;
            if (powerupAffected)
            {
                gameObject.SetActive(false);
                PlatformManager currentPlatfrom = m_GameManager.PlatformsManager.GetPlatform(platformIndex);
                if (currentPlatfrom != null)
                {
                    if (currentPlatfrom.PowerupsOnPlatform != null && blockIndex < currentPlatfrom.PowerupsOnPlatform.Count)
                    {
                        currentPlatfrom.PowerupsOnPlatform[blockIndex].ResetBlock(this);
                    }
                }
            }
        }
        public void SetCollected()
        {
            triggerCollider.enabled = false;
            collected = true;
        }
        private IEnumerator MoveToPlayer(IPlayer _player, bool canShowTrail = false, bool isPowerup = false)
        {
            if (canShowTrail)
            {
                foreach (var trail in trails)
                {
                    trail.material = m_GameManager.GetMaterial(color);
                    trail.enabled = true;
                }
            }
            _player.IncreaseCollectedBlocks(platformIndex, blockIndex, isPowerup);
            while (Vector3.Distance(transform.position, _player.BackTransform.position) > 0.075f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _player.BackTransform.position + Vector3.up * yOffset, speed * Time.deltaTime);
                Vector3 direction = (_player.BackTransform.position + Vector3.up * yOffset - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, speed * Time.deltaTime);
                }
                yield return null;
            }
            _player.CollectBrick(this);
            foreach (var trail in trails)
            {
                trail.enabled = false;
            }
            collectRoutine = null;
            ownerIndex = _player.PlayerIndex;
        }
        private IEnumerator FallToGround(float3 finalPosition)
        {
            Quaternion initialRotation = transform.rotation;
            float3 startPosition = transform.position;
            float targetYRotation = UnityEngine.Random.Range(0, 360);

            float elapsedTime = 0f;
            float duration = 0.35f;

            while (transform.position.y > finalPosition.y)
            {
                transform.position = Vector3.Lerp(startPosition, finalPosition, elapsedTime / duration);
                float currentYRotation = Mathf.LerpAngle(initialRotation.eulerAngles.y, targetYRotation, elapsedTime / duration);
                transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.SetPositionAndRotation(finalPosition, Quaternion.Euler(0, targetYRotation, 0));
            yield return new WaitForSeconds(0.25f);
            SetFree();
        }
        private void SetFree()
        {
            collected = false;
            ownerIndex = -1;
            color = ColorName.Grey;
            mesh.material = m_GameManager.GetMaterial(color);
            triggerCollider.size = colliderSizeAfterFall;
            triggerCollider.enabled = true;
            if (collectRoutine != null)
            {
                StopCoroutine(collectRoutine);
                collectRoutine = null;
            }
            foreach (var trail in trails)
            {
                trail.enabled = false;
            }
        }
    }
}
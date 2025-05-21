using nostra.quickplay.core.Recorder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace nostra.pkpl.stairrace
{
    public class PowerupController : MonoBehaviour, ITrackable, IReconstructable
    {
        [SerializeField] private TextMeshPro multiplierTxt;
        [SerializeField] private SkinnedMeshRenderer borderMesh;
        [SerializeField] private SkinnedMeshRenderer gradientMesh;
        [SerializeField] private Material borderRed;
        [SerializeField] private Material borderGreen;
        [SerializeField] private Material gradientRed;
        [SerializeField] private Material gradientGreen;

        StairRaceManager m_gameManager;
        SRPowerupData m_powerupData;
        bool collected;
        int powerupIndex;
        int m_platformIndex;
        public string id => "powerup_" + m_platformIndex + "_" + powerupIndex;
        List<CollectibleBlock> blocksToHandle = new List<CollectibleBlock>();
        SRPowerData m_powerupDataList;
        bool isWatching = false;

        public IGameObjectState CaptureState ()
        {
            isWatching = false;
            if(m_powerupDataList == null)
            {
                m_powerupDataList = new SRPowerData();
                m_powerupDataList.Id = id;
                m_powerupDataList.canCapture = true;
                m_powerupDataList.isVisible = multiplierTxt.gameObject.activeSelf;
                m_powerupDataList.operation = m_powerupData.operation;
                m_powerupDataList.randomValue = m_powerupData.randomValue;
            }
            else
            {
                if(m_powerupDataList.isVisible != multiplierTxt.gameObject.activeSelf)
                {
                    m_powerupDataList.canCapture = true;
                     m_powerupDataList.isVisible = multiplierTxt.gameObject.activeSelf;
                }
                else
                {
                    m_powerupDataList.canCapture = false;
                }
            }
            return m_powerupDataList;
        }
        public void ApplyState(IGameObjectState _state)
        {
            isWatching = true;
            if(_state is SRPowerData _powerupData)
            {
                switch (_powerupData.operation)
                {
                    case 0: //Add

                        multiplierTxt.text = "+" + _powerupData.randomValue;
                        borderMesh.material = borderGreen;
                        gradientMesh.material = gradientGreen;
                        break;

                    case 1: //Sub

                        multiplierTxt.text = "-" + _powerupData.randomValue;
                        borderMesh.material = borderRed;
                        gradientMesh.material = gradientRed;
                        break;

                    case 2: //Multiply

                        multiplierTxt.text = "x" + _powerupData.randomValue;
                        borderMesh.material = borderGreen;
                        gradientMesh.material = gradientGreen;
                        break;

                    case 3: //Divide

                        multiplierTxt.text = "/" + _powerupData.randomValue;
                        borderMesh.material = borderRed;
                        gradientMesh.material = gradientRed;
                        break;
                }
                Show(_powerupData.isVisible);
            }
        }
        void OnTriggerEnter(Collider other)
        {
            if(isWatching)
            {
                return;
            }
            if(multiplierTxt.gameObject.activeSelf == false)
            {
                return;
            }
            if (other.CompareTag(GameConstants.PLAYER_TAG))
            {
                var player = other.GetComponent<IPlayer>();
                if (!collected && player != null && player.IsPlayer)
                {
                    collected = true;
                    player.CollectPowerup(m_powerupData, this);
                    Show(false);
                }
            }
        }

        public void OnLoaded(StairRaceManager _gameManager, int _platformIndex, int _powerupIndex)
        {
            m_gameManager = _gameManager;
            powerupIndex = _powerupIndex;
            m_platformIndex = _platformIndex;
            m_gameManager.RegisterPowerup(this);
            Show(false);
        }
        public void Reset(SRPowerupData _powerupData)
        {
            m_powerupData = _powerupData;
            collected = false;
            switch (m_powerupData.operation)
            {
                case 0: //Add

                    multiplierTxt.text = "+" + m_powerupData.randomValue;
                    borderMesh.material = borderGreen;
                    gradientMesh.material = gradientGreen;
                    break;

                case 1: //Sub

                    multiplierTxt.text = "-" + m_powerupData.randomValue;
                    borderMesh.material = borderRed;
                    gradientMesh.material = gradientRed;
                    break;

                case 2: //Multiply

                    multiplierTxt.text = "x" + m_powerupData.randomValue;
                    borderMesh.material = borderGreen;
                    gradientMesh.material = gradientGreen;
                    break;

                case 3: //Divide

                    multiplierTxt.text = "/" + m_powerupData.randomValue;
                    borderMesh.material = borderRed;
                    gradientMesh.material = gradientRed;
                    break;
            }
            Show(true);
        }
        public void Show(bool status)
        {
            borderMesh.gameObject.SetActive(status);
            gradientMesh.gameObject.SetActive(status);
            multiplierTxt.gameObject.SetActive(status);
        }

        public void ApplyPowerup(IPlayer _player)
        {
            int spawnsRequired = 0;
            if (m_powerupData.operation == 0)
            {
                spawnsRequired = m_powerupData.randomValue;
            }
            else
            {
                spawnsRequired = (_player.CollectedStairCount * m_powerupData.randomValue) - _player.CollectedStairCount;
            }
            if (spawnsRequired > _player.NumberOfPickupsPossible)
            {
                spawnsRequired = _player.NumberOfPickupsPossible;
            }
            var stairBlocks = m_gameManager.StairTypes;
            CollectibleBlock block = stairBlocks[0].Prefab;
            for (int i = 0; i < spawnsRequired; i++)
            {
                //instance.OnLoaded(m_gameManager, _player.platformIndex, powerupIndex);
                CollectibleBlock instance = m_gameManager.PowerupBlocks[0];
                instance.Reset(_player.Color);
                instance.transform.position = transform.position;
                instance.transform.rotation = Quaternion.identity;
                 m_gameManager.PowerupBlocks.RemoveAt(0);
                instance.gameObject.SetActive(true);
                instance.HandleCollection(_player);
            }
        }
        public void ResetBlock(CollectibleBlock _block)
        {
            m_gameManager.PowerupBlocks.Add(_block);
            _block.transform.position = transform.position;
            _block.transform.rotation = Quaternion.identity;
            _block.gameObject.SetActive(false);
        }
    }
}
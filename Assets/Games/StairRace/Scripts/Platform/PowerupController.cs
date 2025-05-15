using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace nostra.pkpl.stairrace
{
    public class PowerupController : MonoBehaviour
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
        List<CollectibleBlock> blocksToHandle = new List<CollectibleBlock>();

        void OnTriggerEnter(Collider other)
        {
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

        public void OnLoaded(StairRaceManager _gameManager, int _powerupIndex)
        {
            m_gameManager = _gameManager;
            powerupIndex = _powerupIndex;
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
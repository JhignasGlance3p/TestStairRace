using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace nostra.pkpl.stairrace
{
    public class PlatformManager : MonoBehaviour
    {
        [SerializeField] StairRaceManager m_gameManager;
        [SerializeField] private float spacingBwStairs;
        [SerializeField] private StairBlockSpawnArea[] stairSpawnCentres;
        [SerializeField] List<PowerupController> powerups;
        [SerializeField] List<BridgeController> bridges;
        [SerializeField] BlockPlayer blockPlayer;
        
        int platformIndex;
        List<CollectibleBlock> blocksOnPlatform = new();
        SRPlatformProgress platformProgress;

        public List<BridgeController> BridgesOnPlatfom => bridges;
        public List<PowerupController> PowerupsOnPlatform => powerups;
        public List<CollectibleBlock> BlocksOnPlatform => blocksOnPlatform;
        public SRPlatformProgress SaveProgress => platformProgress;

        public void OnLoaded(int _index)
        {
            platformIndex = _index;

            PlaceBricks();
            // bridges = this.transform.GetComponentsInChildren<BridgeController>().ToList();
            int p_index = 0;
            foreach (BridgeController bridge in bridges)
            {
                bridge.OnLoaded(m_gameManager, platformIndex, p_index);
                p_index++;
            }
            // powerups = this.transform.GetComponentsInChildren<PowerupController>().ToList();
            foreach (PowerupController powerup in powerups)
            {
                powerup.OnLoaded(m_gameManager, platformIndex, p_index);
                p_index++;
            }
            if(blockPlayer != null)
            {
                blockPlayer.OnLoaded();
            }
        }
        public int maxBlocksOnPlatform()
        {
            return blocksOnPlatform.Count;
        }
        public void Reset(int[] randomColors, List<SRPowerupData> powerupData)
        {
            int _index = 0;
            foreach (CollectibleBlock block in blocksOnPlatform)
            {
                block.Reset((ColorName)randomColors[_index]);
                _index++;
            }
            foreach (BridgeController bridge in bridges)
            {
                bridge.Reset();
            }
            _index = 0;
            foreach (PowerupController powerup in powerups)
            {
                powerup.Reset(powerupData[_index]);
                _index++;
                if (_index >= powerupData.Count)
                {
                    _index = 0;
                }
            }
            if(blockPlayer != null)
            {
                blockPlayer.OnLoaded();
            }
        }
        public List<CollectibleBlock> GetTargetBlocks(ColorName color)
        {
            var blocks = new List<CollectibleBlock>();
            foreach (CollectibleBlock block in blocksOnPlatform)
            {
                if (block.BlockColor == color)
                    blocks.Add(block);
            }
            return blocks;
        }
        public void SetProgress(SRPlatformProgress _platformProgress)
        {
            platformProgress = _platformProgress;
            foreach (SRBridgeProgress bridgeProgress in _platformProgress.bridgeProgress)
            {
                if (bridgeProgress.bridgeIndex >= 0 && bridgeProgress.bridgeIndex < bridges.Count)
                    bridges[bridgeProgress.bridgeIndex].SetProgress(bridgeProgress);
            }
        }

        private void PlaceBricks()
        {
            List<float3> spawnPositions = new();
            float3 stairSize = m_gameManager.CollectibleStairDimensions;
            float3 startPosition;
            int _index = 0;
            int totalStairs;

            foreach (var spawnArea in stairSpawnCentres)
            {
                totalStairs = (int)((spawnArea.Size.x / (stairSize.x + spacingBwStairs)) * (spawnArea.Size.y / (stairSize.y + spacingBwStairs)));
                startPosition = new(spawnArea.Center.position.x - spawnArea.Size.x / 2 + stairSize.x / 2,
                    spawnArea.Center.position.y, spawnArea.Center.position.z - spawnArea.Size.y / 2 + stairSize.y / 2);
                spawnPositions.Clear();

                for (float x = startPosition.x; x < spawnArea.Center.position.x + spawnArea.Size.x / 2; x += stairSize.x + spacingBwStairs)
                {
                    for (float z = startPosition.z; z < spawnArea.Center.position.z + spawnArea.Size.y / 2; z += stairSize.y + spacingBwStairs)
                    {
                        spawnPositions.Add(new float3(x, spawnArea.Center.position.y, z));
                    }
                }

                var blockParent = new GameObject("BlocksParent");
                blockParent.transform.parent = transform;
                foreach (float3 position in spawnPositions)
                {
                    StairBlock stair = m_gameManager.StairTypes[0];
                    var stairInstance = Instantiate(stair.Prefab, position, Quaternion.identity, blockParent.transform);

                    stairInstance.OnLoaded(m_gameManager, platformIndex, _index);
                    stairInstance.gameObject.name = "Platform_" + platformIndex + "_block_" + _index;
                    blocksOnPlatform.Add(stairInstance);
                    _index++;
                }
            }
        }
    }
}
using com.nampstudios.bumper.Enemy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.nampstudios.bumper.Zone
{
    public class ZoneController : MonoBehaviour
    {
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private Waypoint[] wayPointsInZone;
        [Space]
        [SerializeField] private BridgeController bridge;
        [SerializeField] private int numberOfEnemies;
        [SerializeField] private float spawnRadius;
        [SerializeField] private float formationSpacing;
        [SerializeField] private Vector2 platformSize;
        [SerializeField] private int maxLargeEnemies;
        [Header("Powerup")]
        [SerializeField] private Transform powerupSpawnPoint;
        [SerializeField] private PowerupController powerupPrefab;

        private Transform ParentTransform;
        private int zoneIndex;
        private Action<int> onCompleteCB;
        private Action<int> onZoneEnter;
        private int spawnedEnemyCount;
        GameManager m_gameManager;

        public int ZoneIndex => zoneIndex;
        public Waypoint[] zoneWayPoint;

        int randomPowerUp = 0;
        float eSpeed = 0.0f;
        Vector3 ePosition = Vector3.zero;

        void OnTriggerEnter(Collider other)
        {
            var controller = other.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.TriggerDeath();
            }
            else if (other.CompareTag(GameConstants.PLAYER_TAG))
            {
                m_gameManager.TriggerGameOver(false);
            }
        }

        public void Initialize(GameManager _gameManager, int index, Action<int> onCompleteCallBack, Action<int> zoneEnter, Transform trans)
        {
            m_gameManager = _gameManager;
            zoneIndex = index;
            onCompleteCB = onCompleteCallBack;
            onZoneEnter = zoneEnter;
            ParentTransform = trans;
            bridge.OnEnterZone += HandleZoneEnter;
            if (zoneIndex == 0)
                UnlockZone();
        }
        public void UnlockZone()
        {
            bridge.OpenBridge();
        }
        public void HandleZoneEnter()
        {
            onZoneEnter?.Invoke(zoneIndex);
            SpawnRandomPowerup();
            SpawnEnemies();
        }
        public void CloseBridge()
        {
            bridge.CloseBridge();
        }

        private void SpawnRandomPowerup()
        {
            PowerupType[] types = (PowerupType[])Enum.GetValues(typeof(PowerupType));
            int randomIndex = UnityEngine.Random.Range(0, types.Length);

            if (m_gameManager.scene_Loader.IsWatch)
            {
                randomIndex = m_gameManager.zone_PowerUp;
            }
            else
            {
                randomPowerUp = randomIndex;
            }
            var powerup = Instantiate(powerupPrefab, powerupSpawnPoint);
            powerup.Initialize(m_gameManager, (PowerupType)randomIndex);
        }
        private void SpawnEnemies()
        {
            if (!m_gameManager.isAutoPlay)
            {
                if (m_gameManager.scene_Loader.IsWatch)
                {
                    if (m_gameManager.savedZoneOrder != null)
                    {
                        m_gameManager.Player.zoneOrderIndex = new List<hOrderIndex>(enemySpawnPoints.Length);
                        for (int i = 0; i < enemySpawnPoints.Length; i++)
                        {
                            hOrderIndex temp = new hOrderIndex(null, null);
                            m_gameManager.Player.zoneOrderIndex.Add(temp);
                        }
                    }
                }
                else
                {
                    m_gameManager.Player.zoneOrderIndex = new List<hOrderIndex>(enemySpawnPoints.Length);
                    for (int i = 0; i < enemySpawnPoints.Length; i++)
                    {
                        hOrderIndex temp = new hOrderIndex(null, null);
                        m_gameManager.Player.zoneOrderIndex.Add(temp);
                    }
                }
            }
            for (int hordeIndex = 0; hordeIndex < enemySpawnPoints.Length; hordeIndex++)
            {
                Formation formationType = Formation.Circle; //GetRandomFormation();
                List<Vector3> spawnPositions = GetSpawnPoints(formationType, hordeIndex);
                int numLargeEnemies = maxLargeEnemies <= 1 ? maxLargeEnemies : UnityEngine.Random.Range(0, maxLargeEnemies + 1);
                int numRegularEnemies = spawnPositions.Count - numLargeEnemies;
                int _total = 0;
                if (!m_gameManager.isAutoPlay)
                {
                    if (m_gameManager.scene_Loader.IsWatch)
                    {
                        if (hordeIndex == 0)
                        {
                            _total = (numRegularEnemies + numLargeEnemies);
                            for (int i = 0; i < m_gameManager.Player.zoneOrderIndex.Count; i++)
                            {
                                m_gameManager.Player.zoneOrderIndex[i].enemyConfig = new List<EnemyData>(_total);
                                m_gameManager.Player.zoneOrderIndex[i].zoneConfig = new List<ZoneEnemyCount>(1);
                                for (int j = 0; j < _total; j++)
                                {
                                    m_gameManager.Player.zoneOrderIndex[i].enemyConfig.Add(null);
                                }
                            }
                        }
                        numRegularEnemies = m_gameManager.savedZoneOrder[hordeIndex].zoneConfig[0].sEnemy;
                        numLargeEnemies = m_gameManager.savedZoneOrder[hordeIndex].zoneConfig[0].lEnemy;
                    }
                    else
                    {
                        if (hordeIndex == 0)
                        {
                            m_gameManager.scene_Loader._zoneData = new PlayerZoneData(randomPowerUp);
                            _total = (numRegularEnemies + numLargeEnemies);

                            for (int i = 0; i < m_gameManager.Player.zoneOrderIndex.Count; i++)
                            {
                                m_gameManager.Player.zoneOrderIndex[i].enemyConfig = new List<EnemyData>(_total);
                                for (int j = 0; j < _total; j++)
                                {
                                    m_gameManager.Player.zoneOrderIndex[i].enemyConfig.Add(null);
                                }
                            }
                        }
                        m_gameManager.Player.zoneOrderIndex[hordeIndex].zoneConfig = new List<ZoneEnemyCount>(1);
                        m_gameManager.Player.zoneOrderIndex[hordeIndex].zoneConfig.Add(new ZoneEnemyCount(numRegularEnemies, numLargeEnemies));
                    }
                }
                List<int> availablePositions = new();
                int enemyIndex = 0;

                for (int i = 0; i < spawnPositions.Count; i++)
                {
                    availablePositions.Add(i);
                }

                for (int i = 0; i < spawnPositions.Count; i++)
                {
                    if (availablePositions.Count == 0) break;

                    int positionIndex = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
                    availablePositions.Remove(positionIndex);
                    var enemyPool = numLargeEnemies > 0 ? m_gameManager.LargeEnemyPool : m_gameManager.SmallEnemyPool;
                    if (numLargeEnemies > 0)
                        numLargeEnemies--;
                    var enemy = enemyPool.GetItem();
                    if (enemy == null)
                    {
                        Debug.LogError("Didn't get enemy from Pool");
                        continue;
                    }
                    enemy.gameObject.SetActive(true);

                    if (positionIndex < 0 || positionIndex >= spawnPositions.Count)
                    {
                        Debug.LogError("Position index out of bounds: " + positionIndex);
                        return;
                    }
                    enemy.transform.SetParent(ParentTransform);
                    if (!m_gameManager.isAutoPlay && m_gameManager.scene_Loader.IsWatch)
                    {
                        enemy.reset();
                        enemy.transform.SetPositionAndRotation(m_gameManager.savedZoneOrder[hordeIndex].enemyConfig[i].position, Quaternion.identity);
                        eSpeed = m_gameManager.savedZoneOrder[hordeIndex].enemyConfig[i].speed;
                        //Debug.LogError($"Speed {m_gameManager.savedZoneOrder[hordeIndex].enemyConfig[i].speed} :: {m_gameManager.savedZoneOrder[hordeIndex].enemyConfig[i].position}");
                    }
                    else
                    {
                        enemy.transform.SetPositionAndRotation(spawnPositions[positionIndex], Quaternion.identity);
                    }
                    //enemy.transform.SetPositionAndRotation(spawnPositions[positionIndex], Quaternion.identity);
                    spawnedEnemyCount++;
                    //enemy.Initialize(RemoveEnemy, wayPointsInZone, zoneIndex, enemyPool, spawnPositions[positionIndex],i, hordeIndex);

                    enemy.Initialize(m_gameManager, RemoveEnemy, wayPointsInZone, zoneIndex, enemyPool, eSpeed);

                    if (m_gameManager.isAutoPlay == false)
                    {
                        if (m_gameManager.scene_Loader.IsWatch)
                        {
                            //Debug.LogError($"Speed {m_gameManager.savedZoneOrder[hordeIndex].enemyConfig[i].speed}");
                            //Debug.LogError($"Position {m_gameManager.savedZoneOrder[hordeIndex].enemyConfig[i].position}");
                        }
                        else
                        {
                            //Debug.LogError($"Enemy orderIndex {hordeIndex} : {i} name {enemy.gameObject.name} Waypoints {enemy._wayPoint()} :{spawnPositions[positionIndex]}");
                            m_gameManager.Player.zoneOrderIndex[hordeIndex].enemyConfig[i] = new EnemyData(enemy._moveSpeed(), enemy._wayPoint(), spawnPositions[positionIndex]);
                        }
                    }
                    enemyIndex++;
                }
            }
            m_gameManager.savedZoneOrder = null;
        }
        private List<Vector3> GetSpawnPoints(Formation formationType, int hordeIndex)
        {
            List<Vector3> spawnPositions = new List<Vector3>();
            switch (formationType)
            {
                case Formation.Circle:
                    spawnPositions = GetCircleFormation(enemySpawnPoints[hordeIndex]);
                    break;
                case Formation.Cube:
                    spawnPositions = GetCubeFormation(enemySpawnPoints[hordeIndex]);
                    break;
                case Formation.Triangle:
                    spawnPositions = GetTriangleFormation(enemySpawnPoints[hordeIndex]);
                    break;
                case Formation.Army:
                    spawnPositions = GetArmyFormation(enemySpawnPoints[hordeIndex]);
                    break;
            }
            return spawnPositions;
        }
        private void RemoveEnemy(int scoreForKill)
        {
            m_gameManager.OnEnemyDeath(scoreForKill);
            spawnedEnemyCount--;
            if (spawnedEnemyCount <= 0)
                OnZoneCleared();
        }
        private void OnZoneCleared()
        {
            if (m_gameManager.isAutoPlay)
            {
                m_gameManager.Player.transform.position = Vector3.zero;
                m_gameManager.Player.transform.localEulerAngles = Vector3.zero;
                m_gameManager.Player.targetIndex = 1;
                bridge.reset();
                UnlockZone();
            }
            else
            {
                onCompleteCB.Invoke(zoneIndex);
            }
        }

        private List<Vector3> GetCircleFormation(Transform enemySpawnPoint)
        {
            List<Vector3> positions = new();
            if (numberOfEnemies <= 1)
            {
                positions.Add(enemySpawnPoint.position);
                return positions;
            }
            float radius = Mathf.Min(platformSize.x, platformSize.y) / 2f - formationSpacing;
            for (int i = 0; i < numberOfEnemies; i++)
            {
                float angle = i * Mathf.PI * 2 / numberOfEnemies;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                positions.Add(new Vector3(x, 0, z) + enemySpawnPoint.position);
            }
            return positions;
        }
        private List<Vector3> GetTriangleFormation(Transform enemySpawnPoint)
        {
            List<Vector3> positions = new();
            if (numberOfEnemies <= 1)
            {
                positions.Add(enemySpawnPoint.position);
                return positions;
            }
            int rows = Mathf.CeilToInt(Mathf.Sqrt(2 * numberOfEnemies));
            float xOffset = formationSpacing;
            float zOffset = formationSpacing;
            int count = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (count < numberOfEnemies)
                    {
                        float xPos = (j - i / 2f) * xOffset;
                        float zPos = i * zOffset;
                        positions.Add(new Vector3(xPos, 0, -zPos) + enemySpawnPoint.position);
                        count++;
                    }
                }
            }
            return positions;
        }
        private List<Vector3> GetCubeFormation(Transform enemySpawnPoint)
        {
            List<Vector3> positions = new();
            if (numberOfEnemies <= 1)
            {
                positions.Add(enemySpawnPoint.position);
                return positions;
            }
            var enemyCount = numberOfEnemies - (numberOfEnemies % 4);
            if (enemyCount <= 0)
                for (int i = 0; i < numberOfEnemies; i++)
                {
                    positions.Add(enemySpawnPoint.position);
                }
            int sideCount = Mathf.CeilToInt(Mathf.Sqrt(enemyCount));
            float xOffset = (platformSize.x - (sideCount - 1) * formationSpacing) / 2f;
            float zOffset = (platformSize.y - (sideCount - 1) * formationSpacing) / 2f;

            for (int x = 0; x < sideCount; x++)
            {
                for (int z = 0; z < sideCount; z++)
                {
                    if (positions.Count < enemyCount)
                    {
                        float xPos = x * formationSpacing - (platformSize.x / 2) + xOffset;
                        float zPos = z * formationSpacing - (platformSize.y / 2) + zOffset;
                        positions.Add(new Vector3(xPos, 0, zPos) + enemySpawnPoint.position);
                    }
                }
            }
            return positions;
        }
        private List<Vector3> GetArmyFormation(Transform enemySpawnPoint)
        {
            List<Vector3> positions = new();
            if (numberOfEnemies <= 1)
            {
                positions.Add(enemySpawnPoint.position);
                return positions;
            }
            var enemyCount = numberOfEnemies - (numberOfEnemies % 4);
            if (enemyCount <= 0)
                for (int i = 0; i < numberOfEnemies; i++)
                {
                    positions.Add(enemySpawnPoint.position);
                }
            int columns = Mathf.CeilToInt(Mathf.Sqrt(enemyCount));
            float xOffset = (platformSize.x - (columns - 1) * formationSpacing) / 2f;
            float zOffset = (platformSize.y - (enemyCount / columns - 1) * formationSpacing) / 2f;

            for (int i = 0; i < enemyCount; i++)
            {
                int row = i / columns;
                int column = i % columns;
                float xPos = column * formationSpacing - (platformSize.x / 2) + xOffset;
                float zPos = row * formationSpacing - (platformSize.y / 2) + zOffset;
                positions.Add(new Vector3(xPos, 0, zPos) + enemySpawnPoint.position);
            }
            return positions;
        }
        private Formation GetRandomFormation()
        {
            Formation[] formations = (Formation[])System.Enum.GetValues(typeof(Formation));
            int randomIndex = UnityEngine.Random.Range(0, formations.Length);
            return formations[randomIndex];
        }
    }
}
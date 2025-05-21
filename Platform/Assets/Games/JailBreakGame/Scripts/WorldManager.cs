using System.Collections.Generic;
using UnityEngine;
#if JAILBREAK_FUSION
using Fusion;
#endif
using System.Linq;

namespace nostra.PKPL.JailBreakGame
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [Header("Environment Elements")]
        public List<GameObject> lasers; // All laser GameObjects
        public List<Transform> keySpawnPoints; // Positions where keys can spawn
        public List<Transform> playerSpawnPoints; // Initial spawn points for players
        public List<PoliceController> policeControllers;
        public List<PlayerDoor> playerDoors;

        [Header("Key and Laser Settings")]
        public int maxActiveLasers = 2;
        public int maxActiveKeys = 2;

        private List<GameObject> activeKeys = new List<GameObject>();
        private int keysInPlay = 0;

        [Header("Police Settings")]
        public GameObject policePrefab;
#if JAILBREAK_FUSION
        public NetworkPrefabRef policeNetworkPrefab;
#endif
        public List<Transform> policeSpawnPoints;
        public List<Transform> policePatrolPoints;

        [Header("Door Settings")]
        public List<GameObject> doorPrefabs;
#if JAILBREAK_FUSION
        public List<NetworkPrefabRef> doorNetworkPrefabs;
#endif
        public List<Transform> doorSpawnPoints;

        // Keep track of recently used spawn points to prevent immediate reuse
        private List<Transform> recentlyUsedKeySpawnPoints = new List<Transform>();

        public void resetVariables()
        {
            CleanUp();
            activeKeys = new List<GameObject>();
            InitializeLasers();
        }
        public void Start()
        {
            InitializeLasers();
        }

        public void SpawnInitialKeys()
        {
            if (!gameManager.OnWatch)
            {
                SpawnKeys(maxActiveKeys);
            }
        }

        private void InitializeLasers()
        {
            // Deactivate all lasers at start
            foreach (var laser in lasers)
            {
                laser.SetActive(false);
            }
        }
        //public void ResetSpawnPoliceAndDoors()
        //{
        //    int i = 0;
        //    foreach (var police in policeControllers)
        //    {
        //        police.gameObject.transform.position = policeSpawnPoints[i].position;
        //        police.gameObject.transform.rotation = policeSpawnPoints[i].rotation;
        //    }
        //    i = 0;
        //    foreach (var door in playerDoors)
        //    {
        //        door.gameObject.transform.position = doorSpawnPoints[i].position;
        //        door.gameObject.transform.rotation = doorSpawnPoints[i].rotation;
        //    }
        //}
        public GameObject[] getActiveKeysAvailable()
        {
            return activeKeys.ToArray();
        }
        public void SpawnPolice()
        {
            if (gameManager.isOfflineMode)
            {
                foreach (var spawnPoint in policeSpawnPoints)
                {
                    GameObject police = Instantiate(policePrefab, spawnPoint.position, spawnPoint.rotation, transform);
                    police.GetComponent<PoliceController>().gameManager = gameManager;
                    policeControllers.Add(police.GetComponent<PoliceController>());
                }
            }
#if JAILBREAK_FUSION
            else
            {
                foreach (var spawnPoint in policeSpawnPoints)
                {
                    NetworkObject police = gameManager.networkRunner.Spawn(policeNetworkPrefab, spawnPoint.position, spawnPoint.rotation);
                    police.GetComponent<PoliceController>().gameManager = gameManager;
                    policeControllers.Add(police.GetComponent<PoliceController>());
                }
            }
#endif
        }

        public void SpawnDoors()
        {
            for (int i = 0; i < doorSpawnPoints.Count; i++)
            {
                if (gameManager.isOfflineMode)
                {
                    GameObject door = Instantiate(doorPrefabs[i], doorSpawnPoints[i].position, doorSpawnPoints[i].rotation, transform);
                    door.GetComponent<PlayerDoor>().gameManager = gameManager;
                    playerDoors.Add(door.GetComponent<PlayerDoor>());
                }
#if JAILBREAK_FUSION
                else
                {
                    NetworkObject door = gameManager.networkRunner.Spawn(doorNetworkPrefabs[i], doorSpawnPoints[i].position, doorSpawnPoints[i].rotation);
                    door.GetComponent<PlayerDoor>().gameManager = gameManager;
                    playerDoors.Add(door.GetComponent<PlayerDoor>());
                }
#endif
            }
        }

        public void setDoorIntialValues()
        {
            foreach(var door in playerDoors)
            {
                if(door.doorInitialScales.Count == 0)
                {
                    door.doorInitialScales.Clear();
                    door.keyInitialColors.Clear();
                    door.OnStart();
                }
            }
        }
        public void SwitchLasers()
        {
            if (gameManager.isOfflineMode)
            {
                SwitchLasersLogic();
            }
#if JAILBREAK_FUSION
            else if (gameManager.networkRunner.IsSharedModeMasterClient)
            {
                List<int> laserIndices = new List<int>();

                // Randomly select lasers to activate
                List<int> availableIndices = Enumerable.Range(0, lasers.Count).ToList();
                for (int i = 0; i < maxActiveLasers && availableIndices.Count > 0; i++)
                {
                    int randomIndex = Random.Range(0, availableIndices.Count);
                    int laserIndex = availableIndices[randomIndex];
                    laserIndices.Add(laserIndex);
                    availableIndices.RemoveAt(randomIndex);
                }

                RPC_SwitchLasers(laserIndices.ToArray());
            }
#endif
        }

        private void SwitchLasersLogic(int[] laserIndices = null)
        {
            // Deactivate all lasers first
            foreach (var laser in lasers)
            {
                laser.SetActive(false);
            }

            if (laserIndices != null)
            {
                // Activate specified lasers
                foreach (int index in laserIndices)
                {
                    if (index >= 0 && index < lasers.Count)
                    {
                        lasers[index].SetActive(true);
                    }
                }
            }
            else
            {
                List<GameObject> availableLasers = new List<GameObject>(lasers);
                for (int i = 0; i < maxActiveLasers && availableLasers.Count > 0; i++)
                {
                    int index = Random.Range(0, availableLasers.Count);
                    gameManager.randomLaserIndex.Add(index);
                    GameObject laserToActivate = availableLasers[index];
                    laserToActivate.SetActive(true);
                    availableLasers.RemoveAt(index);
                }
            }
        }

#if JAILBREAK_FUSION
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
#endif
        public void RPC_SwitchLasers(int[] laserIndices)
        {
            SwitchLasersLogic(laserIndices);
        }

        public void SpawnKeysOnWatch()
        {
            List<Transform> availableSpawnPoints = new List<Transform>(keySpawnPoints);
            foreach (var usedPoint in recentlyUsedKeySpawnPoints)
            {
                availableSpawnPoints.Remove(usedPoint);
            }
            for (int i = 0; i < gameManager.randomKeyIndex.Count && availableSpawnPoints.Count > 0; i++)
            {
                int index = gameManager.randomKeyIndex[i];
                //try
                //{
                    Transform spawnPoint = availableSpawnPoints[index];
                    GameObject key = Instantiate(gameManager.keyPrefab, spawnPoint.position, Quaternion.identity, transform);
                    activeKeys.Add(key);
                
                    availableSpawnPoints.RemoveAt(index);
                    keysInPlay++;

                    recentlyUsedKeySpawnPoints.Add(spawnPoint);
                //}
                //catch (System.Exception e)
                //{
                //    Debug.LogError($"Error SpawnKeysOnWatch ::::{e}");
                //}
            }
            if (recentlyUsedKeySpawnPoints.Count > maxActiveKeys)
            {
                recentlyUsedKeySpawnPoints.RemoveAt(0);
            }
        }

        public void TriggerLaser()
        {
            foreach (var laser in lasers)
            {
                laser.SetActive(false);
            }

            List<GameObject> availableLasers = new List<GameObject>(lasers);
            for (int i = 0; i < gameManager.randomLaserIndex.Count && availableLasers.Count > 0; i++)
            {
                int index = gameManager.randomLaserIndex[i];
                GameObject laserToActivate = availableLasers[index];
                laserToActivate.SetActive(true);
                //availableLasers.RemoveAt(index);
            }
        }
        public void SpawnKeys(int count)
        {
            if(gameManager.OnWatch)
            {
                return;
            }
            if (gameManager.isOfflineMode
#if JAILBREAK_FUSION
            || gameManager.networkRunner.IsSharedModeMasterClient
#endif
            )
            {
                List<Transform> availableSpawnPoints = new List<Transform>(keySpawnPoints);
                // Remove recently used spawn points to avoid spawning keys at the same position
                foreach (var usedPoint in recentlyUsedKeySpawnPoints)
                {
                    availableSpawnPoints.Remove(usedPoint);
                }
                for (int i = 0; i < count && availableSpawnPoints.Count > 0; i++)
                {
                    int index = Random.Range(0, availableSpawnPoints.Count);
                    gameManager.randomKeyIndex.Add(index);
                    Transform spawnPoint = availableSpawnPoints[index];
                    if (gameManager.isOfflineMode)
                    {
                        // Spawn key as a regular object
                        GameObject key = Instantiate(gameManager.keyPrefab, spawnPoint.position, Quaternion.identity, transform);
                        activeKeys.Add(key);
                        gameManager.m_audioManager.PlayClip(gameManager.m_audioManager.keySpawned);
                    }
#if JAILBREAK_FUSION
                    else
                    {
                        // Spawn key as a networked object
                        NetworkObject key = gameManager.networkRunner.Spawn(
                            gameManager.keyNetworkPrefab,
                            spawnPoint.position,
                            Quaternion.identity
                        );
                        gameManager.m_audioManager.PlayClip(gameManager.m_audioManager.keySpawned);

                        activeKeys.Add(key.gameObject);
                    }
#endif
                    availableSpawnPoints.RemoveAt(index);
                    keysInPlay++;

                    // Add this spawn point to the recently used list
                    recentlyUsedKeySpawnPoints.Add(spawnPoint);
                }

                // Keep the recently used list to a reasonable size
                if (recentlyUsedKeySpawnPoints.Count > maxActiveKeys)
                {
                    recentlyUsedKeySpawnPoints.RemoveAt(0);
                }
            }
        }

        public void OnKeyPickedUp(GameObject key, Transform spawnPoint)
        {
            if (gameManager.isOfflineMode)
            {
                activeKeys.Remove(key);
                keysInPlay--;
                // Remove the spawn point from recently used list so it can be used again later
                recentlyUsedKeySpawnPoints.Remove(spawnPoint);
                Destroy(key);
            }
#if JAILBREAK_FUSION
            else if (gameManager.networkRunner.IsSharedModeMasterClient)
            {
                activeKeys.Remove(key.gameObject);
                keysInPlay--;
                // Remove the spawn point from recently used list so it can be used again later
                recentlyUsedKeySpawnPoints.Remove(spawnPoint);

                // Despawn the key network object
                gameManager.networkRunner.Despawn(key.GetComponent<NetworkObject>());
            }
#endif
        }

        public void OnPlayerLostKey()
        {
            if(gameManager.OnWatch)
            {
                return;
            }
            if (gameManager.isOfflineMode
#if JAILBREAK_FUSION
            || gameManager.networkRunner.IsSharedModeMasterClient
#endif
            )
            {
                // Spawn a new key since the player lost the key
                if (keysInPlay < maxActiveKeys)
                {
                    SpawnKeys(1);
                }
            }
        }

        public void OnPlayerUsedKey()
        {
            if (gameManager.OnWatch)
            {
                return;
            }
            if (gameManager.isOfflineMode
#if JAILBREAK_FUSION
            || gameManager.networkRunner.IsSharedModeMasterClient
#endif
            )
            {
                // Spawn a new key since the player used the key at the door
                if (keysInPlay < maxActiveKeys)
                {
                    SpawnKeys(1);
                }
            }
        }

        public void OnLaserEvent()
        {
            if (!gameManager.OnWatch)
            {
                // Switch lasers when a player respawns
                SwitchLasers();
            }
        }

        public void CleanUp()
        {
            if (gameManager.isOfflineMode
#if JAILBREAK_FUSION
            || gameManager.networkRunner.IsSharedModeMasterClient
#endif
            )
            {
#if JAILBREAK_FUSION
                if (!gameManager.isOfflineMode)
                {
                    foreach (var key in activeKeys)
                    {
                        gameManager.networkRunner.Despawn(key.GetComponent<NetworkObject>());
                    }
                }
                else
#endif
                {
                    foreach (var key in activeKeys)
                    {
                        Destroy(key);
                    }
                }
                keysInPlay = 0;
                recentlyUsedKeySpawnPoints.Clear();
                foreach (var door in playerDoors)
                {
                    door.ResetDoor();
                }
                foreach (var police in policeControllers)
                {
                    police.ResetPolice();
                }
            }
        }
    }
}

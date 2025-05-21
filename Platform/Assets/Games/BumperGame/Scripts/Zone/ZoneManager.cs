using System.Collections.Generic;
using UnityEngine;

namespace com.nampstudios.bumper.Zone
{
    public class ZoneManager : MonoBehaviour
    {
        [SerializeField] Transform zoneHolder;
        [SerializeField] private ZoneData[] zones;

        public GameObject ParentGO;
        public ZoneData currentZone;
        public ZoneController zoneContrller;

        ZoneController[] zoneControllers;
        GameManager m_gameManager;
        private Dictionary<int, ZoneController> spawnedZones = new();
        private List<ZoneController> completedZones = new();

        void OnDisable()
        {
            completedZones.Clear();
            spawnedZones.Clear();
        }

        public void Initialise(GameManager _gameManager)
        {
            m_gameManager = _gameManager;
            zoneControllers = zoneHolder.GetComponentsInChildren<ZoneController>();
            for (int i = 0; i < zoneControllers.Length; i++)
            {
                spawnedZones.Add(i, zoneControllers[i]);
                zoneControllers[i].Initialize(m_gameManager, i, OnZoneCleared, OnZoneEnter, ParentGO.transform);
            }
            m_gameManager.TriggerOnZonesReady();
            currentZone = zones[0];
        }
        public Waypoint[] getZoneOneWayPoint()
        {
            zoneContrller = zones[0].ZonePrefab;
            return zoneContrller.zoneWayPoint;
        }
        public void CleanZone()
        {
        }

        private void SpawnZone()
        {
            //TODO Deepak
            // zone.Initialize(zoneIndex, OnZoneCleared, OnZoneEnter, ParentGO.transform);
            // spawnedZones.Add(zoneIndex, zone);
        }
        private void OnZoneEnter(int index)
        {
            if (index <= 0)
                return;
            var prev = spawnedZones[index - 1];
            prev.CloseBridge();
        }
        private void OnZoneCleared(int zoneIndex)
        {
            var next = spawnedZones[zoneIndex + 1];
            next.UnlockZone();
            m_gameManager.Ui_Manager.ShowText($" Zone Cleared");
            SpawnZone();
            completedZones.Add(spawnedZones[zoneIndex]);
            if (completedZones.Count > 2)
            {
                var first = completedZones[0];
                spawnedZones.Remove(first.ZoneIndex);
                completedZones.RemoveAt(0);
                // Destroy(first.gameObject);TODO Deepak
                // if (spawnedStartStage != null)
                //     Destroy(spawnedStartStage);
            }
        }
    }
}
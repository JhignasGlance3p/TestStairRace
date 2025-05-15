using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;

namespace nostra.pkpl.stairrace
{
    public class PlatformsManager : MonoBehaviour
    {
        [SerializeField] Transform platformParent;
        [SerializeField] WinPlatform m_winPlatform;

        Dictionary<int, PlatformManager> spawnedPlatforms = new();

        public WinPlatform winPlatform
        {
            get
            {
                return m_winPlatform;
            }
        }
        public int NumberOfPlatforms => spawnedPlatforms.Count;

        public List<PlatformManager> OnLoaded()
        {
            PlatformManager[] platformHandlers = platformParent.GetComponentsInChildren<PlatformManager>();
            int index = 0;
            foreach (PlatformManager _platform in platformHandlers)
            {
                if (spawnedPlatforms.ContainsKey(index) == false)
                {
                    _platform.OnLoaded(index);
                    spawnedPlatforms.Add(index, _platform);
                }
                index++;
            }
            return platformHandlers.ToList();
        }
        public PlatformManager GetPlatform(int index)
        {
            if (spawnedPlatforms.TryGetValue(index, out PlatformManager platform))
            {
                return platform;
            }
            return null;
        }
    }
}
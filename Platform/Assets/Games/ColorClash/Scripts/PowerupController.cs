using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.sarvottam.colorclash
{
    public class PowerupController : MonoBehaviour
    {
        [SerializeField] private float PowerupsSpawnTime = 30f;
        [SerializeField] private List<PowerUps> powerUps;

        Coroutine powerupCoroutine;
        GameManager m_gameManager;
        List<PowerUps> powerupsList = new List<PowerUps>();

        public void OnLoaded(GameManager _gameManager)
        {
            m_gameManager = _gameManager;
        }
        public void OnFocussed()
        {
            OnReset();
        }
        public void OnReset()
        {
            foreach (PowerUps item in powerupsList)
            {
                if(item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            powerupsList.Clear();
            if(powerupCoroutine != null)
            {
                StopCoroutine(powerupCoroutine);
                powerupCoroutine = null;
            }
        }
        public void OnPause()
        {
            if(powerupCoroutine != null)
            {
                StopCoroutine(powerupCoroutine);
                powerupCoroutine = null;
            }
            foreach (PowerUps item in powerupsList)
            {
                if(item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            powerupsList.Clear();
        }
        public void OnStart()
        {
            powerupCoroutine = StartCoroutine(PowerUpsSpawn());
        }
        private IEnumerator PowerUpsSpawn()
        {
            while (true)
            {
                yield return new WaitForSeconds(PowerupsSpawnTime);
                
                Vector3 randompos = m_gameManager.GetRandomPosition(true);
                int powerupIndex = Random.Range(0, powerUps.Count);
                PowerUps powerup = Instantiate(powerUps[powerupIndex].gameObject, randompos, Quaternion.identity, this.transform).GetComponent<PowerUps>();
                powerup.GetComponent<PowerUps>().OnLoaded();
                powerup.gameObject.SetActive(true);
                powerupsList.Add(powerup);
            }
        }
    }
}
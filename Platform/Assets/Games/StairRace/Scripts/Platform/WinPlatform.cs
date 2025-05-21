using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class WinPlatform : MonoBehaviour
    {
        [SerializeField] StairRaceManager m_gameManager;
        [SerializeField] private Transform m_target;

        public Transform target => m_target;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(GameConstants.PLAYER_TAG) && m_gameManager != null)
            {
                if (other.GetComponent<PlayerController>() != null)
                {
                    m_gameManager.TriggerGameOver(true);
                }
                else
                {
                    m_gameManager.TriggerGameOver(false);
                }
            }
        }
    }
}
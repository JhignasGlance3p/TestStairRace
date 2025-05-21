using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class NeutralCharacter : MonoBehaviour
    {
        private GameObject agent;
        public GameObject CharacterGO => agent;

        NeutralCharacterStatus storingData;
        public NeutralCharacterStatus NCharacterStatus => storingData;

        bool m_canTrigger = false;

        public void SetAgent(GameObject go, int _parentIndex, bool _canTrigger = true)
        {
            storingData = new NeutralCharacterStatus();
            storingData.isVisible = true;
            storingData.parentIndex = _parentIndex;

            agent = go;
            m_canTrigger = _canTrigger;
        }
        public void UpdateStatus(NeutralCharacterStatus status, bool _cantrigger)
        {
            m_canTrigger = _cantrigger;
            this.gameObject.SetActive(status.isVisible);
        }
        private void OnTriggerEnter(Collider col)
        {
            if (this.gameObject.activeInHierarchy == false || m_canTrigger == false)
            {
                return;
            }
            if (col.CompareTag("Character"))
            {
                CrowdHandler crowdHandler = col.GetComponentInParent<CrowdHandler>();
                if (crowdHandler == null)
                    return;

                crowdHandler.UpdateCrowd(1);
                storingData.isVisible = false;
                this.gameObject.SetActive(false);
            }
        }
    }
}
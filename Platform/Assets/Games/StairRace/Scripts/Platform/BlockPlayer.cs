using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class BlockPlayer : MonoBehaviour
    {
        [SerializeField] private Collider collider;
        public void OnLoaded()
        {
            if(collider != null) collider.isTrigger = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController _))
            {
                if (other.transform.position.z > transform.position.z)
                {
                    if(collider != null) collider.isTrigger = false;
                }
            }
        }
    }
}
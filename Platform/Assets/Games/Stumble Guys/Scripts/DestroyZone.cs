using UnityEngine;

namespace nostra.origami.stumble
{
    public class DestroyZone : MonoBehaviour
    {
        [SerializeField] ParticleSystem bounce;

        private void OnTriggerEnter(Collider collision)
        {
            Debug.Log($"[DestroyZone] OnTriggerEnter: {collision.gameObject.name}, {this.gameObject.name}");
            if (collision.CompareTag("Player"))
            {
                collision.GetComponent<ICharacter>().Deactive();
            }
        }
    }
}
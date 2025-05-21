using UnityEngine;
using UnityEngine.UI;

namespace nostra.origami.stumble
{
    public class LHS_Particle : MonoBehaviour
    {
        [SerializeField] ParticleSystem winParticle;
        [SerializeField] ParticleSystem win;

        public void SuccessParticle()
        {
            winParticle.Play();
            win.Play();
        }
    }
}
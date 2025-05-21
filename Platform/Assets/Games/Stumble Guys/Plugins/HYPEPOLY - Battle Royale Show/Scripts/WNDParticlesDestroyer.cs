using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.origami.stumble
{
    public class WNDParticlesDestroyer : MonoBehaviour
    {
        ParticleSystem ParticleSystem;

        void Awake()
        {
            ParticleSystem = GetComponent<ParticleSystem>();
        }
        void FixedUpdate()
        {
            if(!ParticleSystem.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
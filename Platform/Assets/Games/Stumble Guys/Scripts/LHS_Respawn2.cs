using nostra.origami.common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.origami.stumble
{
    public class LHS_Respawn2 : MonoBehaviour
    {
        [SerializeField] ObjectPooler pooler;
        [SerializeField] Transform respawnPoint;

        Animator anim;
        GameObject player;

        void DownPlayer()
        {
            PlayAnimation("isFalling", true);
        }
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[LHS_Respawn2] OnTriggerEnter: {other.gameObject.name}, {this.gameObject.name}");
            if (other.CompareTag("Player"))
            {
                player = other.gameObject;
                if (other.gameObject == player)
                {
                    PlayAnimation("isFalling", false);
                    player.transform.position = respawnPoint.transform.position;
                }
                else
                {
                    other.gameObject.transform.position = respawnPoint.transform.position;
                }
            }
            if (other.CompareTag("QP_Floor"))
            {
                pooler.AddToPool(other.GetComponentInParent<PooledObjectResetter>());
            }
        }
        void PlayAnimation(string _animationName, bool _shouldPlay)
        {
            if (anim == null && player != null)
            {
                anim = player.GetComponentInChildren<Animator>();
            }
            if(anim != null)
            {
                anim.SetBool(_animationName, _shouldPlay);
            }
        }
    }
}
using nostra.origami.common;
using nostra.origami.stumble;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace nostra.origami.stumble
{
    public class AINavMesh : MonoBehaviour, ICharacter
    {
        [SerializeField] UIManager UI;
        [SerializeField] Rigidbody m_rigid;
        [SerializeField] NavMeshAgent m_agent;

        public Animator animator;
        public bool cannotControl;
        public NavMeshAgent agent => m_agent;
        public Rigidbody rigid => m_rigid;

        Vector3 startPos;
        GameObject destPos;

        public void OnLoaded()
        {
            startPos = transform.position;
        }
        public void OnFocus(GameObject _character)
        {
            _character.transform.SetParent(transform);
            _character.transform.localPosition = new Vector3(0, -1.05f, 0);
            _character.transform.localRotation = Quaternion.identity;
            _character.transform.localScale = Vector3.one;
            _character.SetActive(true);

            this.transform.position = startPos;
        }
        public void OnAutoPlay()
        {
            int randomPoint = Random.Range(1, 7);
            destPos = UI.AIDistGameObjects[randomPoint];
            isAgentStopped(true);

            rigid.isKinematic = false;
            if(animator != null) animator.SetBool("isMove", true);
            isAgentStopped(false);
            cannotControl = false;
        }

        public void isAgentStopped(bool status)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = status;
            }
            if (status)
            {
                Deactive();
            }
        }
        void FixedUpdate()
        {
            if (agent == null)
            {
                return;
            }
            if (agent.isStopped)
            {
                return;
            }
            /*if(UI.roundOver.gameObject.activeSelf)
            {
                agent.speed = 16f;
            }*/
            if (cannotControl || UI.RoundEnded)
            {
                agent.destination = transform.position;
                agent.isStopped = true;
                rigid.linearVelocity = Vector3.zero;
                rigid.isKinematic = true;
                if(animator != null) animator.SetBool("isMove", false);
                return;
            }
            if (destPos != null)
            {
                agent.destination = destPos.transform.position;
            }
            FreezeRotation();
        }
        void FreezeRotation()
        {
            rigid.angularVelocity = Vector3.zero;
        }
        public void Deactive()
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.isKinematic = true;
            if(animator != null)animator.SetBool("isMove", false);
            if (agent.isOnNavMesh)
            {
                agent.destination = transform.position;
                agent.isStopped = true;
            }
            cannotControl = true;
        }
    }
}
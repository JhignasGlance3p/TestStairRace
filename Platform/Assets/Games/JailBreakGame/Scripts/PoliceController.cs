using System.Collections;
using System.Collections.Generic;
#if JAILBREAK_FUSION
using Fusion;
#endif
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace nostra.PKPL.JailBreakGame
{
    public class PoliceController :
#if JAILBREAK_FUSION
    NetworkBehaviour
#else
    MonoBehaviour
#endif
    {
        [Header("Sentry Settings")]
        public NavMeshAgent agent;
        public float detectionRange = 10f;
        public float fieldOfView = 60f;
        public float detectionInterval = 0.2f;
        public LayerMask detectionLayerMask; // Set this to detect players

        private Transform targetPlayer;
        private float lastDetectionTime;
        private Transform targetDestination;

        public Animator policeAnimator;

        public Material lineMaterial;
        List<LineRenderer> lineRenderers = new List<LineRenderer>();
        public Transform raycastOrigin;
        public Color detectionColor = Color.red;
        public Color nonDetectionColor = Color.grey;

        public Color startColor = Color.red;
        public float startWidth = 0.1f;
        public float endWidth = 0.5f;

        Vector3 initialPosition;
        Quaternion initialRotation;

#if JAILBREAK_FUSION
        [Networked] 
#endif
        public float speed { get; set; }
        public GameManager gameManager;

        Coroutine m_behaviour;
        public void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            agent.enabled = true;
            // Ensure the agent is active
            agent.isStopped = false;

            for (int i = 0; i < 10; i++)
            {
                lineRenderers.Add(null);
            }

            m_behaviour = StartCoroutine(BehaviorRoutine());
        }

        public void stopBehavior()
        {
            if (agent.enabled)
            {
                agent.isStopped = false;
            }
            agent.enabled = false;
        }
        public void PauseTheGame(bool status)
        {
            if (agent.enabled)
                agent.isStopped = status;
            //agent.enabled = !status;
            if(status)
            {
                if (m_behaviour != null)
                {
                    StopCoroutine(m_behaviour);
                }
            }
            else
            {
                m_behaviour = StartCoroutine(BehaviorRoutine());
            }
        }
        public float setSpeed()
        {
            return agent.velocity.magnitude;
        }
        public void SetSpeedFromTxt(float _speed)
        {
            policeAnimator.SetFloat("Speed", _speed);
        }
        private void Update()
        {
            if(gameManager.OnWatch || gameManager.pauseTheGame)
            {
                return;
            }
            if (gameManager.isOfflineMode)
            {
                policeAnimator.SetFloat("Speed", agent.velocity.magnitude);
            }
            else
            {
                policeAnimator.SetFloat("Speed", speed);
            }
        }

#if JAILBREAK_FUSION
        public override void FixedUpdateNetwork()
        {
            speed = agent.velocity.magnitude / agent.speed;
        }
#endif
        private IEnumerator BehaviorRoutine()
        {
            while (!gameManager.OnWatch && gameManager.IsRoundInProgress() && !gameManager.pauseTheGame)
            {
                DetectPlayers();

                if (targetPlayer != null && Time.time - lastDetectionTime < detectionInterval)
                {
                    // Chase the player
                    agent.SetDestination(targetPlayer.position);
                }
                else
                {
                    targetPlayer = null;
                    // Move towards a random key spawn point
                    if (targetDestination == null || agent.remainingDistance < 0.5f)
                    {
                        // Get a random patrol point
                        if (gameManager.m_worldManager.policePatrolPoints.Count > 0)
                        {
                            int index = Random.Range(0, gameManager.m_worldManager.policePatrolPoints.Count);
                            targetDestination = gameManager.m_worldManager.policePatrolPoints[index];
                            agent.SetDestination(targetDestination.position);
                        }
                    }
                }

                yield return null;
            }
        }

        private void DetectPlayers()
        {
            float halfFOV = fieldOfView / 2f;
            float angleStep = fieldOfView / 9f; // 10 rays, 9 intervals

            Vector3[] directions = new Vector3[10];
            for (int i = 0; i < directions.Length; i++)
            {
                float angle = -halfFOV + (angleStep * i);
                directions[i] = Quaternion.Euler(0, angle, 0) * raycastOrigin.forward;
            }

            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 dir = directions[i];
                RaycastHit hit;
                if (Physics.Raycast(raycastOrigin.position, dir, out hit, detectionRange, detectionLayerMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        PlayerController playerController = hit.collider.GetComponent<PlayerController>();
                        if (playerController != null && !playerController.hasEscaped && !playerController.isDead)
                        {
                            targetPlayer = playerController.transform;
                            lastDetectionTime = Time.time;
                            DrawDetectionLines(i, raycastOrigin.position, hit.point, detectionColor);
                        }
                        else
                        {
                            DrawDetectionLines(i, raycastOrigin.position, hit.point, nonDetectionColor);
                        }
                    }
                    else
                    {
                        DrawDetectionLines(i, raycastOrigin.position, hit.point, nonDetectionColor);
                    }
                }
                else
                {
                    DrawDetectionLines(i, raycastOrigin.position, raycastOrigin.position + dir * detectionRange, nonDetectionColor);
                }
            }
        }

        //add line renderer component
        private void DrawDetectionLines(int index, Vector3 start, Vector3 end, Color color)
        {
            if (lineRenderers[index] == null)
            {
                var newChild = new GameObject("DetectionLine" + index).transform;
                newChild.position = start;
                newChild.parent = transform;
                lineRenderers[index] = newChild.gameObject.AddComponent<LineRenderer>();
                lineRenderers[index].material = lineMaterial;
            }
            lineRenderers[index].endWidth = endWidth;
            lineRenderers[index].startWidth = startWidth;
            lineRenderers[index].enabled = true;
            lineRenderers[index].SetPosition(0, start);
            lineRenderers[index].SetPosition(1, end);
            lineRenderers[index].startColor = startColor;
            lineRenderers[index].endColor = color;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                PlayerController playerController = collision.collider.GetComponent<PlayerController>();
                if (playerController != null && !playerController.hasEscaped)
                {
                    playerController.StartCoroutine(playerController.HandleDeath());
                }
            }
        }

        public void ResetPolice()
        {
            //agent.isStopped = true;
            agent.enabled = false;
            DisableDetectionLines();
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            //StopAllCoroutines();
            if(m_behaviour != null)
            {
                StopCoroutine(m_behaviour);
            }
        }

        void DisableDetectionLines()
        {
            foreach (var line in lineRenderers)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }
    }
}

using nostra.origami.common;
using UnityEngine;
using UnityEngine.AI;

namespace nostra.origami.crowdcity
{
    public class CrowdController : MonoBehaviour
    {
        [SerializeField] private float speed = 3f, currentTurnSmoothTime = 0.1f;
        [SerializeField] private Transform cam;
        [SerializeField] protected CrowdHandler crowdHandler;
        [SerializeField] protected NavMeshAgent agent;

        float turnSmoothVelocity;
        Vector3 targetPoint;
        bool m_autoPlay;
        float roamElapsedTime = 0f;

        public NavMeshAgent Agent { get { return agent; } }

        void Update()
        {
            if (m_autoPlay == false)
            {
                return;
            }
            float distance = targetPoint != null ? MyUtils.GetDistanceXZ(transform.position, targetPoint) : 0f;
            if (roamElapsedTime <= 0f || distance < 0.1f)
            {
                GetRandomRoamPoint();
            }
            else
            {
                roamElapsedTime -= Time.deltaTime;
            }
        }

        public void Init()
        {
            this.gameObject.transform.localPosition = Vector3.zero;
            agent.angularSpeed = agent.acceleration = float.MaxValue;
        }
        public void OnGameOver()
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(Vector3.zero);
                agent.isStopped = true;
            }
            agent.enabled = false;
        }
        public void SetAutoPlay(bool canAutoPlay, bool _agentEnabled)
        {
            SetPlayerNav(canAutoPlay);
            if (canAutoPlay == true)
            {
                roamElapsedTime = 0f;
                this.GetRandomRoamPoint();
            }
            m_autoPlay = canAutoPlay;
            targetPoint = transform.position;
            agent.enabled = _agentEnabled;
        }
        public void HandleInput(Vector3 movementDirection)
        {
            if (m_autoPlay == true)
            {
                return;
            }
            movementDirection.Normalize();
            if (movementDirection != Vector3.zero)
            {
                float targetAngle, angle;
                targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, currentTurnSmoothTime);
                transform.eulerAngles = new Vector3(0f, angle, 0f);
            }
            float velocity = movementDirection.magnitude * speed;
            if (velocity > 0f)
            {
                transform.Translate(Time.deltaTime * velocity * transform.forward, Space.World);
            }
        }

        protected void GetRandomRoamPoint()
        {
            if (agent.isOnNavMesh)
            {
                targetPoint = crowdHandler.GetRandomAIPoints();
                roamElapsedTime = Random.Range(8f, 15f);
                agent.SetDestination(targetPoint);
            }
        }
        private void SetPlayerNav(bool shouldSet)
        {
            if (shouldSet == true)
            {
                agent.speed = 6;
                agent.angularSpeed = 120;
                agent.acceleration = 8;
                if (agent.isOnNavMesh)
                    agent.isStopped = false;
            }
            else
            {
                agent.speed = 0;
                agent.angularSpeed = 0;
                agent.acceleration = 0;
                if (agent.isOnNavMesh)
                {
                    agent.SetDestination(this.transform.position);
                    agent.isStopped = true;
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

namespace nostra.pkpl.stairrace
{
    public class PlayerController : IPlayer
    {
        [SerializeField] private float player_rotationSpeed;
        [SerializeField] private float moveSpeed;
        [SerializeField] private Transform groundCheckTransform;
        [SerializeField] private float playerDeathTriggerVelocity = -20f;

        CameraManager m_cameraManager;
        CharacterController characterController;
        Vector2 moveDirection;
        float verticalVelocity = 0f;
        float3 movedDirection;
        float3 movedVelocity;
        float3 rotateDir;

        private bool IsGrounded => Physics.Raycast(groundCheckTransform.position, Vector3.down, 0.01f);

        void FixedUpdate()
        {
            if (playerControl == false)
            {
                return;
            }
            MovePlayer();
        }
        protected override void Update()
        {
            base.Update();
            if (playerControl == true)
            {
                if (IsGrounded)
                {
                    verticalVelocity = -0.1f;
                }
                else
                {
                    verticalVelocity += Physics.gravity.y * Time.deltaTime;
                }
                if (characterController.velocity.y < playerDeathTriggerVelocity)
                    m_gameManager.TriggerGameOver(false);
            }
        }

        public override bool IsPlayer
        {
            get
            {
                return true;
            }
        }
        public override bool CanCollectBlock
        {
            get
            {
                return CollectedStairCount < maxBlocksCanHold;
            }
        }

        public void OnLoaded(StairRaceManager _gameManager, CameraManager _cameraManager, int _index)
        {
            m_gameManager = _gameManager;
            m_cameraManager = _cameraManager;
            m_playerIndex = _index;
            navMeshAgent = GetComponent<NavMeshAgent>();
            originalSpeed = navMeshAgent.speed;
            characterController = GetComponent<CharacterController>();
            if (m_cameraManager != null)
            {
                m_cameraManager.SetTarget(transform);
            }
        }
        public override void OnFocussed()
        {
            characterController.enabled = false;
            base.OnFocussed();
        }
        public override void OnStart()
        {
            base.OnFocussed();
            navMeshAgent.enabled = false;
            characterController.enabled = true;
            playerControl = true;
        }

        protected override void FallAndRecover()
        {
            base.FallAndRecover();
            if (playerControl == true)
            {
                return;
            }
            if (fallRoutine != null)
            {
                StopCoroutine(fallRoutine);
                navMeshAgent.speed = originalSpeed;
            }
            navMeshAgent.speed *= 0.25f;
            StartCoroutine(FallAndRecoverCO());
            SwitchState(State.Collecting);
        }
        private IEnumerator FallAndRecoverCO()
        {
            yield return new WaitForSeconds(fallAnimTime);
            fallRoutine = null;
            navMeshAgent.speed = originalSpeed;
        }
        public override void OnMoveEvent(Vector2 _direction)
        {
            moveDirection = _direction;
        }
        private void MovePlayer()
        {
            movedDirection = new Vector3(moveDirection.x, 0, moveDirection.y).normalized;

            movedVelocity = movedDirection * moveSpeed;
            movedVelocity.y = verticalVelocity;

            characterController.Move(Time.deltaTime * movedVelocity);
            
            rotateDir = Vector3.zero;
            if (movedDirection.x != 0 || movedDirection.z != 0)
            {
                rotateDir.x = movedDirection.x;
                rotateDir.z = movedDirection.z;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rotateDir), player_rotationSpeed * Time.deltaTime);
            }
            if(movedVelocity.x == 0 && movedVelocity.z == 0)
            {
                m_gameManager.PlayAnimation(nostracharacter, "walkBlend", 0f);
            }
            else
            {
                m_gameManager.PlayAnimation(nostracharacter, "walkBlend", 1f);
            }
        }
    }
}
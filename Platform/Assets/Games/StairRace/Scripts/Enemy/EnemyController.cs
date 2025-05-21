using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace nostra.pkpl.stairrace
{
    public class EnemyController : IPlayer
    {
        public override bool CanCollectBlock
        {
            get
            {
                return CollectedStairCount < stairsToCollectMax;
            }
        }

        public void OnLoaded(StairRaceManager _gameManager, int _index)
        {
            m_gameManager = _gameManager;
            m_playerIndex = _index;
            navMeshAgent = GetComponent<NavMeshAgent>();
            originalSpeed = navMeshAgent.speed;
        }
        public override void OnStart()
        {
            base.OnFocussed();
        }

        protected override void FallAndRecover()
        {
            base.FallAndRecover();
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
    }
}
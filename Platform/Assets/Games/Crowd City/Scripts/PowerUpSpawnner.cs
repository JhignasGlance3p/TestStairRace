using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace nostra.origami.crowdcity
{
    public class PowerUpSpawnner : MonoBehaviour
    {
        public GameObject agentPrefab; // Prefab of the agent with NavMeshAgent component
        public int numberOfAgents = 10; // Number of agents to spawn
        public float xRangeMin = -10f; // Minimum x range for spawning
        public float xRangeMax = 10f; // Maximum x range for spawning
        public float zRangeMin = -10f; // Minimum z range for spawning
        public float zRangeMax = 10f; // Maximum z range for spawning

        private List<GameObject> agentPool; // Pool of agent game objects
        public int poolSize = 20; // Initial pool size

        void Start ()
        {
            Debug.LogError ( "PowerUpSpawnner: " + this.gameObject.name );
            agentPool = new List<GameObject> ( );

            // Initialize the pool with inactive agents
            for ( int i = 0; i < poolSize; i++ )
            {
                GameObject agent = Instantiate ( agentPrefab );
                agent.SetActive ( false );
                agentPool.Add ( agent );
            }

            // Spawn the initial agents
            for ( int i = 0; i < numberOfAgents; i++ )
            {
                SpawnAgentOnNavMesh ( );
            }
        }

        void SpawnAgentOnNavMesh ()
        {
            Vector3 randomPosition = GetRandomPosition ( );
            NavMeshHit hit;

            if ( NavMesh.SamplePosition ( randomPosition, out hit, 1.0f, NavMesh.AllAreas ) )
            {
                GameObject agent = GetPooledAgent ( );

                agent = Instantiate ( agentPrefab, hit.position + Vector3.up, Quaternion.identity );
                agentPool.Add ( agent );
                agent.transform.position = hit.position;
                agent.transform.rotation = Quaternion.identity;
                agent.SetActive ( true );

            }
        }

        GameObject GetPooledAgent ()
        {
            foreach ( GameObject agent in agentPool )
            {
                if ( !agent.activeInHierarchy )
                {
                    return agent;
                }
            }
            return null;
        }

        Vector3 GetRandomPosition ()
        {
            // Around the city
            //float randomX = Random.Range(xRangeMin, xRangeMax);
            //float randomZ = Random.Range(zRangeMin, zRangeMax);

            // around the gameobject
            float randomX = Random.Range ( transform.position.x + Random.Range ( xRangeMin, xRangeMax ), transform.position.x + Random.Range ( xRangeMin, xRangeMax ) );
            float randomZ = Random.Range ( transform.position.z + Random.Range ( zRangeMin, zRangeMax ), transform.position.z + Random.Range ( zRangeMin, zRangeMax ) );
            return new Vector3 ( randomX, 1.3f, randomZ );
        }
    }
}
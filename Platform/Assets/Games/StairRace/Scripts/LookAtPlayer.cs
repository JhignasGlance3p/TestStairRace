using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class LookAtPlayer : MonoBehaviour
    {
        float turn_speed = 5.0f;
        public Transform player;

        void Update()
        {
            if (player == null)
            {
                return;
            }

            // transform.LookAt(player, Vector3.up);
        }
    }
}
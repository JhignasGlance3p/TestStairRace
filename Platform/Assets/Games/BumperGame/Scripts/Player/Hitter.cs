using com.nampstudios.bumper.Enemy;
using UnityEngine;

namespace com.nampstudios.bumper.Player
{
    public class Hitter : MonoBehaviour
    {
        private float weaponPushMultiplier=0f;

        public void Init(float multiplier)
        {
            weaponPushMultiplier = multiplier;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var controller = collision.gameObject.GetComponent<EnemyController>();
            if(controller != null)
            {
                controller.TakePush(transform.forward, weaponPushMultiplier);
            }
        }
    }
}

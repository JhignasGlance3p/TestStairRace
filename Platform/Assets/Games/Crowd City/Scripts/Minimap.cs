using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class Minimap : MonoBehaviour
    {
        public Transform Player { get; set; }
        public int OrderLayer { get; set; }

        void LateUpdate ()
        {
            if ( Player == null )
            {
                return;
            }
            Vector3 newPosition = Player.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;
        }
    }
}
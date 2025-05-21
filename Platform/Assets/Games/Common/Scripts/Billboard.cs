using UnityEngine;

namespace nostra.origami.common
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] Transform cam;

        void LateUpdate()
        {
            if ( cam != null )
            {
                transform.LookAt ( transform.position + cam.forward );
            }
        }
    }
}
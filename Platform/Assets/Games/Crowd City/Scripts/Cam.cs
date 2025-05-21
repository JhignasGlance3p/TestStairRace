using System.Collections.Generic;
using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class Cam : MonoBehaviour
    {
        [SerializeField] private LayerMask layerToHide;
        [SerializeField] private float range;
        [SerializeField] private Transform raycastPoint;

        private List<ObjectFader> obstructingObjs = new ( ) { };
        private SmoothCameraFollow cameraFollow;
        private float elapsedTime = 0f;

        private void Start ()
        {
            cameraFollow = GetComponent<SmoothCameraFollow> ( );
        }

        void Update ()
        {
            if ( elapsedTime > 2f )
            {
                elapsedTime = 0f;
                if ( obstructingObjs.Count > 0 )
                {
                    foreach ( ObjectFader fader in obstructingObjs )
                    {
                        fader.Reset ( );
                    }
                }
            } else
            {
                elapsedTime += Time.deltaTime;
            }

            Vector3 targetPosition = cameraFollow.playerTarget.position;
            Vector3 direction = targetPosition - raycastPoint.position;
            direction.Normalize ( );

            Ray ray = new ( raycastPoint.position, direction );
            RaycastHit[] hits = Physics.RaycastAll ( ray, range, layerToHide );
            if ( hits.Length > 0 )
            {
                foreach ( RaycastHit hit in hits )
                {
                    if ( hit.collider != null )
                    {
                        Debug.DrawRay ( raycastPoint.position, direction * range, Color.green );
                        ObjectFader layer = hit.collider.gameObject.GetComponent<ObjectFader> ( );

                        layer.Fade ( );

                        if ( !obstructingObjs.Contains ( layer ) )
                        {
                            obstructingObjs.Add ( layer );
                        }
                    }
                }
            } else
            {
                Debug.DrawRay ( raycastPoint.position, direction * range, Color.red );
            }
        }
    }
}
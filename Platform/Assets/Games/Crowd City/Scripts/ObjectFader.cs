using UnityEngine;
using nostra.origami.common;

namespace nostra.origami.crowdcity
{
    public class ObjectFader : MonoBehaviour
    {
        private MeshRenderer[] rends;
        private bool fade;

        void Start ()
        {
            rends = GetComponentsInChildren<MeshRenderer> ( );
        }
        private void OnEnable ()
        {
            if ( rends == null || rends.Length <= 0 )
            {
                rends = GetComponentsInChildren<MeshRenderer> ( );
            }
        }

        public void Fade ()
        {
            if ( fade )
                return;

            fade = true;

            if ( rends != null )
            {
                foreach ( MeshRenderer rend in rends )
                {
                    rend.material.color = Color.black;
                    rend.material.color = MyUtils.SetColor ( 0, 0, 0, 50 );
                }
            }
        }

        public void Reset ()
        {
            if ( !fade )
                return;

            fade = false;

            if ( rends != null )
            {
                foreach ( MeshRenderer rend in rends )
                {
                    rend.material.color = Color.white;
                    rend.material.color = MyUtils.SetColor ( 255, 255, 255, 255 );
                }
            }
        }
    }
}
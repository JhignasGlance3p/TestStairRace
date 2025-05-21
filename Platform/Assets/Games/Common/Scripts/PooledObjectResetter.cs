using UnityEngine;

namespace nostra.origami.common
{
    public class PooledObjectResetter : MonoBehaviour
    {
        public ObjectPooler Pooler { get; set; }
        public string PoolTag { get; set; }
        public MonoBehaviour ResetObject;

        public void ResetPooledObject ()
        {
            if ( ResetObject != null )
                ResetObject.Invoke ( "ResetObject", 0f );
        }

        public void Back2Pool()
        {
            Pooler.AddToPool(this);
        }
    }
}
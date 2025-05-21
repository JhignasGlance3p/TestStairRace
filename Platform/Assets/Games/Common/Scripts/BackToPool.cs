using nostra.origami.common;
using UnityEngine;

public class BackToPool : MonoBehaviour
{
    public float timeToAddBack = 1f;

    private void OnEnable()
    {
        Invoke(nameof(AddBackToPool), timeToAddBack);
    }
    private void AddBackToPool()
    {
        PooledObjectResetter pooledObject = GetComponent<PooledObjectResetter>();
        pooledObject.Pooler.AddToPool(pooledObject);
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace nostra.origami.common
{
    public class ObjectPooler : MonoBehaviour
    {
        [SerializeField] List<Pool> pools;
        Dictionary<string, Queue<GameObject>> poolDictionary;

        public void OnLoaded()
        {
            InitPool();
        }

        void InitPool()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, transform.position, Quaternion.identity, transform);
                    PooledObjectResetter pooledObject = obj.AddComponent<PooledObjectResetter>();
                    pooledObject.PoolTag = pool.tag;
                    pooledObject.Pooler = this;

                    obj.transform.SetParent(transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        public GameObject SpawnFromPool(string tag, Transform parent = null, Vector3 position = default, Quaternion rotation = default, Vector3 scale = default, bool enableObj = true, bool returnBack = false, float defaultReturnTime = 2f)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                print("Pool tag with name " + tag + " doesn't exists!");
                return null;
            }

            GameObject objectToSpawn;
            if (poolDictionary[tag].Count > 0)
            {
                objectToSpawn = poolDictionary[tag].Dequeue();
            }
            else
            {
                objectToSpawn = Instantiate(ObjectInPool(tag).prefab);
                PooledObjectResetter pooledObject = objectToSpawn.AddComponent<PooledObjectResetter>();
                pooledObject.PoolTag = tag;
                poolDictionary[tag].Enqueue(objectToSpawn);
                objectToSpawn = poolDictionary[tag].Dequeue();
            }

            if (parent != null)
            {
                objectToSpawn.transform.SetParent(parent);
                objectToSpawn.transform.localPosition = position;
                objectToSpawn.transform.localRotation = rotation;
            }
            else
            {
                objectToSpawn.transform.parent = transform.parent != null ? transform.parent : null;
                objectToSpawn.transform.position = position;
                objectToSpawn.transform.rotation = rotation;
            }

            if (scale != Vector3.zero)
                objectToSpawn.transform.localScale = scale;
            else
                objectToSpawn.transform.localScale = Vector3.one;

            if (returnBack)
            {
                BackToPool backToPoolComp = objectToSpawn.GetComponent<BackToPool>();
                if (!backToPoolComp)
                    backToPoolComp = objectToSpawn.AddComponent<BackToPool>();
                backToPoolComp.timeToAddBack = defaultReturnTime;
            }
            objectToSpawn.SetActive(enableObj);
            return objectToSpawn;
        }
        public void AddToPool(PooledObjectResetter objToReset, bool activeSelf = false)
        {
            if (poolDictionary.ContainsKey(objToReset.PoolTag))
            {
                poolDictionary[objToReset.PoolTag].Enqueue(objToReset.gameObject);
                objToReset.gameObject.SetActive(activeSelf);
                objToReset.gameObject.transform.SetParent(transform);
            }
        }
        public Pool ObjectInPool(string tag)
        {
            return pools.Find(x => x.tag == tag);
        }
    }
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
}
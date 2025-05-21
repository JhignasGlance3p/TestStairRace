using UnityEngine;

namespace com.nampstudios.bumper.Enemy
{
    public class BaseEnemyPool : GenericPool<EnemyController>
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private int cacheCount;
        [SerializeField] private Transform parent;

        private void Start()
        {
            onStart();
        }
        public void onStart()
        {
            for (int i = 0; i < cacheCount; i++)
            {
                var pooledItem = CreateNewPooledItem();
                pooledItem.item.gameObject.SetActive(false);
            }
        }
        protected override EnemyController CreateItem()
        {
            return Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, parent);
        }
    }
}
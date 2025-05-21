using com.nampstudios.bumper.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.nampstudios.bumper.UI
{
    public class CoinObjectPool : GenericPool<GameObject>
    {
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Transform coinParent;
        [SerializeField] private int cacheCount;

        private void Start()
        {
            for (int i = 0; i < cacheCount; i++)
            {
                var pooledItem = CreateNewPooledItem();
                pooledItem.item.gameObject.SetActive(false);
            }
        }
        protected override GameObject CreateItem()
        {
            return Instantiate(coinPrefab, coinParent);
        }
    }
}

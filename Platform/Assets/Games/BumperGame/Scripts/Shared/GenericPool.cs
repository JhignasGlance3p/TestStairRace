using UnityEngine;
using System.Collections.Generic;

namespace com.nampstudios.bumper
{
    public class GenericPool<T> : MonoBehaviour where T : class
    {
        private List<PooledItem<T>> pooledItems = new();

        public virtual T GetItem()
        {
            PooledItem<T> pooledItem;
            if (pooledItems.Count > 0)
            {
                pooledItem = pooledItems.Find(i => !i.isUsed);
                if (pooledItem != null)
                {
                    pooledItem.isUsed = true;
                    return pooledItem.item;
                }
            }
            pooledItem = CreateNewPooledItem();
            pooledItem.isUsed = true;
            return pooledItem.item;
        }
        protected PooledItem<T> CreateNewPooledItem()
        {
            PooledItem<T> newItem = new PooledItem<T>
            {
                item = CreateItem(),
                isUsed = false
            };
            pooledItems.Add(newItem);
            return newItem;
        }
        protected virtual T CreateItem()
        {
            return (T)null;
        }

        public virtual void ReturnItem(T item)
        {
            PooledItem<T> pooledItem = pooledItems.Find(i => i.item.Equals(item));
            if (pooledItem != null)
            {
                pooledItem.isUsed = false;
            }
        }
    }
}
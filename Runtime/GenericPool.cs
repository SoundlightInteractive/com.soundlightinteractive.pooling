using System;
using System.Collections;
using System.Collections.Generic;
using SoundlightInteractive.EventSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundlightInteractive.Pooling
{
    public class GenericPool<T> where T : Actor
    {
        private Queue<T> _objectPool = new();
        private Func<T> _createFunc;

        public GenericPool(Func<T> createFunc, Transform parent, int initialBufferSize = 100)
        {
            _createFunc = createFunc;

            for (int i = 0; i < initialBufferSize; i++)
            {
                T newObj = _createFunc();
                newObj.transform.parent = parent;
                newObj.gameObject.SetActive(false);
                _objectPool.Enqueue(newObj);
            }
        }

        public T Get()
        {
            if (_objectPool.Count == 0)
            {
                T newObj = _createFunc();
                newObj.gameObject.SetActive(true);
                newObj.InitializeActor();
                return newObj;
            }

            T pooledObject = _objectPool.Dequeue();
            pooledObject.gameObject.SetActive(true);
            pooledObject.InitializeActor();
            return pooledObject;
        }

        public void Release(T obj)
        {
            obj.ResetActor();
            obj.gameObject.SetActive(false);
            _objectPool.Enqueue(obj);
        }

        public void AdjustPoolSize(int newSize)
        {
            while (_objectPool.Count > newSize)
            {
                T obj = _objectPool.Dequeue();
                Object.Destroy(obj.gameObject);
            }

            while (_objectPool.Count < newSize)
            {
                T newObj = _createFunc();
                newObj.gameObject.SetActive(false);
                newObj.ResetActor();
                _objectPool.Enqueue(newObj);
            }
        }

        public void Dispose()
        {
            while (_objectPool.Count > 0)
            {
                T obj = _objectPool.Dequeue();
                Object.Destroy(obj.gameObject);
            }
        }
    }
}
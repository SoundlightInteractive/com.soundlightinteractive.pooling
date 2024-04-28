using System;
using System.Collections;
using System.Collections.Generic;
using SoundlightInteractive.EventSystem;
using SoundlightInteractive.Manager;
using SoundlightInteractive.Utils;
using UnityEngine;

namespace SoundlightInteractive.Pooling
{
    public class PoolManager : Manager<PoolManager>
    {
        private Dictionary<string, object> _pools = new();

        public T SpawnFromPool<T>(string actorTag, Vector3 position, Quaternion rotation) where T : Actor
        {
            return SpawnFromPool<T>(actorTag, position, rotation, 0f);
        }

        public T SpawnFromPool<T>(string actorTag, Vector3 position, Quaternion rotation, float delay) where T : Actor
        {
            if (!_pools.ContainsKey(actorTag))
            {
                SIDebug.LogError($"<color=red>ERROR</color><color=orange>{actorTag} pool could not be found!</color>");
                return null;
            }
            
            if (_pools[actorTag] is not GenericPool<T> pool)
            {
                SIDebug.LogError($"<color=red>ERROR</color><color=orange>Incorrect pool type for tag {actorTag}</color>");
                return null;
            }

            T objectToSpawn = pool.Get();
            if (delay <= 0)
            {
                SetObjectProperties(objectToSpawn, position, rotation);
            }
            else
            {
                StartCoroutine(DelayedActivate(objectToSpawn, position, rotation, delay));
            }

            return objectToSpawn;
        }

        public void CreatePool<T>(string actorTag, T prefab, int size) where T : Actor
        {
            if (_pools.ContainsKey(actorTag))
            {
                SIDebug.LogError(
                    $"<color=red>ERROR</color><color=orange>A pool with tag {actorTag} already exists!</color>");
                return;
            }

            GameObject poolParent = new($"{actorTag} Parent")
            {
                transform =
                {
                    position = Vector3.zero,
                    eulerAngles = Vector3.zero,
                    localEulerAngles = Vector3.zero
                }
            };

            GenericPool<T> newPool = new(() => Instantiate(prefab), poolParent.transform, size);
            _pools[actorTag] = newPool;
        }

        public void ReleaseToPool<T>(string actorTag, T objectToRelease) where T : Actor
        {
            if (!_pools.ContainsKey(actorTag))
            {
                SIDebug.LogError($"<color=red>ERROR</color><color=orange>{actorTag} pool could not be found!</color>");
                return;
            }
            
            if (_pools[actorTag] is not GenericPool<T> pool)
            {
                SIDebug.LogError($"<color=red>ERROR</color><color=orange>Incorrect pool type for tag {actorTag}</color>");
                return;
            }
            
            pool.Release(objectToRelease);
        }

        public void DisposePool<T>(string actorTag) where T : Actor
        {
            if (!_pools.ContainsKey(actorTag))
            {
                return;
            }
            
            (_pools[tag] as GenericPool<T>)?.Dispose();
            _pools.Remove(tag);
        }

        public void DisposeAllPools()
        {
            foreach (object pool in _pools.Values)
            {
                (pool as IDisposable)?.Dispose();
            }

            _pools.Clear();
        }

        private IEnumerator DelayedActivate<T>(T objectToSpawn, Vector3 position, Quaternion rotation, float delay)
            where T : Actor
        {
            objectToSpawn.gameObject.SetActive(false);
            yield return new WaitForSeconds(delay);
            
            SetObjectProperties(objectToSpawn, position, rotation);
        }

        private void SetObjectProperties<T>(T objectToSpawn, Vector3 position, Quaternion rotation) where T : Actor
        {
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.gameObject.SetActive(true);
        }
        public override void ResetActor()
        {
            
        }

        public override void InitializeActor()
        {
            
        }
    }
}
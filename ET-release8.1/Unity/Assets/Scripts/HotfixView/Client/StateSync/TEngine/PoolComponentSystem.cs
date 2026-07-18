using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(PoolComponent))]
    [FriendOf(typeof(PoolComponent))]
    public static partial class PoolComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PoolComponent self)
        {
            GameObject poolRootGo = new GameObject("PoolComponent(PoolRoot)");
            self.PoolRoot = poolRootGo.transform;
            GameObject.DontDestroyOnLoad(self.PoolRoot);
        }

        [EntitySystem]
        private static void Destroy(this PoolComponent self)
        {
            foreach (List<GameObject> pool in self.Pools.Values)
            {
                foreach (GameObject go in pool)
                {
                    if (go != null)
                    {
                        UnityEngine.Object.Destroy(go);
                    }
                }
            }

            self.Pools.Clear();

            if (self.PoolRoot != null)
            {
                UnityEngine.Object.Destroy(self.PoolRoot.gameObject);
                self.PoolRoot = null;
            }
        }

        /// <summary>
        /// 从对象池获取 GameObject，key 一般为资源路径；池中没有则加载并创建。
        /// </summary>
        public static async ETTask<GameObject> GetEffect(this PoolComponent self, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Log.Error("PoolComponent GetEffect failed, key is empty");
                return null;
            }

            if (self.Pools.TryGetValue(key, out List<GameObject> pool) && pool.Count > 0)
            {
                GameObject go = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
                if (go != null)
                {
                    go.SetActive(true);
                    return go;
                }
            }

            Scene scene = self.Scene();
            ResourcesLoaderComponent resourcesLoader = scene.GetComponent<ResourcesLoaderComponent>();
            if (resourcesLoader == null)
            {
                Log.Error($"PoolComponent GetEffect failed, ResourcesLoaderComponent is null, key={key}");
                return null;
            }

            GameObject prefab = await resourcesLoader.LoadAssetAsync<GameObject>(key);
            if (prefab == null)
            {
                Log.Error($"PoolComponent GetEffect failed, prefab not found: {key}");
                return null;
            }

            GameObject newGo = UnityEngine.Object.Instantiate(prefab);
            newGo.name = prefab.name;
            return newGo;
        }

        /// <summary>
        /// 归还 GameObject 到对象池，key 需与 GetEffect 时一致。
        /// </summary>
        public static void ReturnEffect(this PoolComponent self, string key, GameObject go)
        {
            if (string.IsNullOrEmpty(key) || go == null)
            {
                return;
            }

            

            if (!self.Pools.TryGetValue(key, out List<GameObject> pool))
            {
                pool = new List<GameObject>();
                self.Pools.Add(key, pool);
            }

            if (pool!=null&&pool.Count<=10)
            {
                go.SetActive(false);
                go.transform.SetParent(self.PoolRoot, false);
                pool.Add(go);
            }
            else
            {
                GameObject.Destroy(go);
            }


        }
    }
}

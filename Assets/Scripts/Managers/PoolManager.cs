using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class PoolManager : MonoBehaviour
{
    private Dictionary<int, IObjectPool<GameObject>> _pools = new Dictionary<int, IObjectPool<GameObject>>();

    public void Init()
    {
        _pools.Clear();
    }
    
    private IObjectPool<GameObject> CreatePool(GameObject argPrefab)
    {
        return new ObjectPool<GameObject>(
            createFunc: () => Instantiate(argPrefab),
            actionOnGet: OnPoolGet,
            actionOnRelease: OnPoolRelease,
            actionOnDestroy: OnPoolDestroy
        );
    }

    private void OnPoolGet(GameObject argObject)
    {
        if (argObject == null)
            return;
        argObject.SetActive(true);
    }
    
    private void OnPoolRelease(GameObject argObject)
    {
        if (argObject == null)
            return;
        argObject.SetActive(false);
    }
    
    private void OnPoolDestroy(GameObject argObject)
    {
        if (argObject == null)
            return;
        Object.Destroy(argObject);
    }
    
    public GameObject Instantiate(PrefabID argPrefabId)
    {
        int id = (int)argPrefabId;
        if (!Managers.Data.TryGetPrefabInfo(id, out var info))
        {
            Debug.LogError($"No prefab found for id:{id}");
            return null;
        }
        if (!_pools.ContainsKey(id))
        {
            _pools[id] = CreatePool(info.prefab);
        }

        GameObject go = _pools[id].Get();
        if (go == null)
        {
            Debug.LogError($"instantiate failed. id:{id}");
            return null;
        }

        return go;
    }

    public void Destroy<T>(T argType, PrefabID argPrefabId) where T : Component
    {
        if (argType == null || argType.gameObject == null) return;

        int id = (int)argPrefabId;
        if (_pools.TryGetValue(id, out var pool))
        {
            pool.Release(argType.gameObject);
        }
        else
        {
            Debug.LogError($"{id} does not exist");
            Object.Destroy(argType.gameObject);
        }
    }
}

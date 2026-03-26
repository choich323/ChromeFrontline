using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    private const int DEFAULT_SLOT_INDEX = 0;
    
    [SerializeField] private Transform _entityParent;
    
    // slot과 coroutine의 인덱스를 동일하게 맞춰야 한다.
    private List<EntitySpawnSlot> _slotList = new List<EntitySpawnSlot>();
    private List<Coroutine> _coroutineList = new List<Coroutine>();
    private int _slotIndex = DEFAULT_SLOT_INDEX;
    private Team _team;
    private Transform _targetTransform;
    private Dictionary<Type, HashSet<AEntity>> _entityDict = new Dictionary<Type, HashSet<AEntity>>();
    
    public int SlotCount => _slotList.Count;
    public Transform TargetTransform => _targetTransform;
    
    public void Init(Team argTeam, Transform argTargetTransform)
    {
        ResetSpawner();
        
        _team = argTeam;
        _targetTransform = argTargetTransform;
        
        for (int i = 0; i < Managers.Game.SlotCountMax; i++)
        {
            AddSlot();
        }
    }

    public void AddSlot()
    {
        var slot = new EntitySpawnSlot();
        slot.Init(_slotIndex, OnSlotTargetChanged);
        _slotIndex++;
        _slotList.Add(slot);
        _coroutineList.Add(null);
    }

    void ResetSpawner()
    {
        StopAllCoroutines();
        _coroutineList.Clear();
        
        foreach (var slot in _slotList)
        {
            slot.Destroy();
        }
        _slotList.Clear();
        
        _slotIndex = DEFAULT_SLOT_INDEX;
        _team = Team.None;
        _targetTransform = null;
    }

    void OnSlotTargetChanged(int argSlotIndex)
    {
        StartSpawn(argSlotIndex);
    }

    void StopSpawn(int argSlotIndex)
    {
        if (_coroutineList[argSlotIndex] != null)
        {
            StopCoroutine(_coroutineList[argSlotIndex]);
            _coroutineList[argSlotIndex] = null;
        }
    }

    void StartSpawn(int argSlotIndex)
    {
        if (argSlotIndex < 0 || argSlotIndex >= _slotList.Count)
        {
            return;
        }

        StopSpawn(argSlotIndex);
        _coroutineList[argSlotIndex] = StartCoroutine(CoStartSpawn(argSlotIndex));
    }

    IEnumerator CoStartSpawn(int argSlotIndex)
    {
        var slot = _slotList[argSlotIndex];
        var targetId = slot.GetTargetId();
        Managers.Data.TryGetPrefabInfo((int)targetId, out var info);
        if(info == null) 
            yield break;

        while (true)
        {
            var entityInfo = info as EntityInfo;
            float productionTime = entityInfo.productionTime;
            float elapsedTime = 0f;
            while (elapsedTime < productionTime) {
                elapsedTime += Time.deltaTime;
                slot.SetProgress(Mathf.Clamp01(elapsedTime / productionTime));
                yield return null;
            }
            
            slot.SetProgress(0);
            Spawn(targetId, entityInfo);
            yield return null;
        }
    }
    
    void Spawn(PrefabID argPrefabId, EntityInfo argEntityInfo)
    {
        var entityObj = Managers.Pool.Instantiate(argPrefabId);
        if (entityObj != null)
        {
            entityObj.transform.position = transform.position;
            entityObj.transform.SetParent(_entityParent);
            var entity = entityObj.GetComponent<AEntity>();
            entity.Init(argPrefabId, Managers.Game.GetNewUid(), _team, argEntityInfo, _slotIndex, _targetTransform, DestroyEntity);
            
            OnSpawn(entity);
        }
    }

    void OnSpawn(AEntity argEntity)
    {
        AddEntity(argEntity);
    }

    public void Destroy()
    {
        DestroyEntities();
        
        ResetSpawner();
    }
    
    void AddEntity(AEntity argEntity)
    {
        Type type = argEntity.GetType();
        if (!_entityDict.ContainsKey(type))
        {
            _entityDict[type] = new HashSet<AEntity>();
        }
        _entityDict[type].Add(argEntity);
    }

    void RemoveEntity(AEntity argEntity)
    {
        Managers.Pool.Destroy(argEntity, argEntity.Id);
        
        Type type = argEntity.GetType();
        if (_entityDict.TryGetValue(type, out var entitySet))
        {
            entitySet.Remove(argEntity);
        }
        argEntity.ResetEntity();
    }
    
    public int GetEntityCount()
    {
        int count = 0;
        foreach (var kvp in _entityDict)
        {
            count += kvp.Value.Count;
        }

        return count;
    }
    
    void DestroyEntities()
    {
        List<AEntity> entityList = new List<AEntity>();
        foreach(var kvp in _entityDict)
        {
            foreach (var entity in kvp.Value)
            {
                entityList.Add(entity);
            }
        }
        _entityDict.Clear();

        foreach (var entity in entityList)
        {
            DestroyEntity(entity);
        }
    }

    public void DestroyEntity(AEntity argEntity)
    {
        RemoveEntity(argEntity);
    }
    
    [ContextMenu("Spawn")]
    void TestSpawn()
    {
        _slotList[0].ChangeTarget(PrefabID.Pioneer);
        _slotList[1].ChangeTarget(PrefabID.Alien);
    }
}

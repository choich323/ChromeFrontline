using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    private const int DEFAULT_SLOT_INDEX = 0;
    
    [SerializeField] private float SPAWN_POS_Y_OFFSET = 0.2f;
    [SerializeField] private Transform _entityParent;
    
    // slot과 coroutine의 인덱스를 동일하게 맞춰야 한다.
    private List<EntitySpawnSlot> _slotList = new List<EntitySpawnSlot>();
    private List<Coroutine> _coroutineList = new List<Coroutine>();
    private int _slotIndex = DEFAULT_SLOT_INDEX;
    private Team _team;
    private Lane _lane;
    private Transform _targetTransform;
    private Dictionary<Type, HashSet<AEntity>> _entityDict = new Dictionary<Type, HashSet<AEntity>>();
    private Action<long> _earnGold;
    private Action<long> _consumeGold;
    private Func<long> _getGold;
    private Func<float> _getProductionBonus;
    
    public int SlotCount => _slotList.Count;
    public Transform TargetTransform => _targetTransform;
    
    public void Init(Team argTeam, Lane argLane, Transform argTargetTransform, Action<long> argEarnGold, Action<long> argConsumeGold, Func<long> argGetGold, Func<float> argGetProductionBonus)
    {
        ResetSpawner();
        
        _team = argTeam;
        _lane = argLane;
        _targetTransform = argTargetTransform;
        if (_team == Team.Player)
        {
            _earnGold = argEarnGold;
            _consumeGold = argConsumeGold;
            _getGold = argGetGold;
            _getProductionBonus = argGetProductionBonus;
        }
        
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

    public EntitySpawnSlot GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slotList.Count)
            return null;
        
        return _slotList[slotIndex];
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
        _lane = Lane.None;
        _targetTransform = null;
        _consumeGold = null;
        _getGold = null;
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
            var slot = _slotList[argSlotIndex];
            var targetId = slot.GetTargetId();
            Managers.Data.TryGetPrefabInfo((int)targetId, out var info);
            if (info is EntityInfo entityInfo)
            {
                _earnGold?.Invoke(entityInfo.goldCost);
            }
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
        while (true)
        {
            Managers.Data.TryGetPrefabInfo((int)targetId, out var info);
            if(info == null) 
                yield break;
            var entityInfo = info as EntityInfo;
            var goldCost = entityInfo.goldCost;
            while (_getGold?.Invoke() < goldCost)
            {
                yield return null;
            }
            
            _consumeGold?.Invoke(goldCost);
            float productionTime = entityInfo.productionTime;
            productionTime *= (1 - _getProductionBonus?.Invoke() ?? 0f);
            float elapsedTime = 0f;
            while (elapsedTime < productionTime) {
                elapsedTime += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsedTime / productionTime);
                slot.SetProgress(progress);
                yield return null;
            }
            
            slot.SetProgress(0);
            Spawn(entityInfo);
            yield return null;
        }
    }
    
    public void ForceSpawn(List<EntityInfo> entityInfoList)
    {
        StartCoroutine(CoForceSpawn(entityInfoList));
    }
    
    IEnumerator CoForceSpawn(List<EntityInfo> entityInfoList)
    {
        var wait = new WaitForSeconds(1f);
        foreach (var info in entityInfoList)
        {
            Spawn(info);

            yield return wait;
        }
    }
    
    void Spawn(EntityInfo argEntityInfo)
    {
        var prefabId = argEntityInfo.GetPrefabID();
        if (prefabId.Equals(PrefabID.None))
        {
            Debug.LogError($"Invalid prefab ID for entity: {prefabId}");
            return;
        }
        var entityObj = Managers.Pool.Instantiate(prefabId);
        if (entityObj != null)
        {
            var randomYOffset = UnityEngine.Random.Range(-SPAWN_POS_Y_OFFSET, SPAWN_POS_Y_OFFSET);
            entityObj.transform.position = new Vector2(transform.position.x, transform.position.y + randomYOffset);
            entityObj.transform.SetParent(_entityParent);
            var entity = entityObj.GetComponent<AEntity>();
            entity.Init(prefabId, Managers.Game.GetNewUid(), _team, argEntityInfo, _slotIndex, _targetTransform, DestroyEntity, _earnGold);
            
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
    
    public int GetEntitiesCount()
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
}

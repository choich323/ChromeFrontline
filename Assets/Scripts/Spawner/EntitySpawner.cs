using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    private const int DEFAULT_SLOT_INDEX = 0;

    [SerializeField] private int _startSlotCount = 2;
    [SerializeField] private float _spawnerPosYOffset = 1.5f;
    [SerializeField] private float _enemySpawnWaitTime = 1f;
    [SerializeField] private Transform _entityParent;
    
    // slot과 coroutine의 인덱스를 동일하게 맞춰야 한다.
    private List<EntitySpawnSlot> _slotList = new List<EntitySpawnSlot>();
    private List<Coroutine> _coroutineList = new List<Coroutine>();
    private int _slotIndex = DEFAULT_SLOT_INDEX;
    private Team _team;
    private Transform _targetTransform;
    private Dictionary<Type, HashSet<AEntity>> _entityDict = new Dictionary<Type, HashSet<AEntity>>();
    private Action<long> _earnGold;
    private Action<long> _consumeGold;
    private Func<long> _getGold;
    private Func<float> _getProductionBonus;
    private Action<int> _entityCountChanged;
    
    public int SlotCount => _slotList.Count;
    public Transform TargetTransform => _targetTransform;
    
    public void Init(Team argTeam, Transform argTargetTransform, Action<long> argEarnGold, Action<long> argConsumeGold, Func<long> argGetGold, Func<float> argGetProductionBonus)
    {
        ResetSpawner();
        
        _team = argTeam;
        _targetTransform = argTargetTransform;
        if (_team == Team.Player)
        {
            _earnGold = argEarnGold;
            _consumeGold = argConsumeGold;
            _getGold = argGetGold;
            _getProductionBonus = argGetProductionBonus;
        }
        
        for (int i = 0; i < _startSlotCount; i++)
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

    public IEnumerator<EntitySpawnSlot> GetSlotEnumerator()
    {
        return _slotList.GetEnumerator();
    }

    public bool SetSlotGrade(int argIndex, Grade argGrade)
    {
        return _slotList[argIndex].SetGrade(argGrade);
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
        _consumeGold = null;
        _getGold = null;
    }

    void OnSlotTargetChanged(int argSlotIndex, bool argIsStop, int argPrevTargetId)
    {
        if (argIsStop)
        {
            StopSpawn(argSlotIndex, argPrevTargetId);
            return;
        }
        
        StartSpawn(argSlotIndex, argPrevTargetId);
    }

    void StopSpawn(int argSlotIndex, int argPrevTargetId)
    {
        if (_coroutineList[argSlotIndex] != null)
        {
            StopCoroutine(_coroutineList[argSlotIndex]);
            _coroutineList[argSlotIndex] = null;
            
            Managers.Data.TryGetPrefabInfo(argPrevTargetId, out var info);
            if (info is EntityInfo entityInfo)
            {
                _earnGold?.Invoke(entityInfo.goldCost);
            }
        }
    }

    void StartSpawn(int argSlotIndex, int argPrevTargetId)
    {
        if (argSlotIndex < 0 || argSlotIndex >= _slotList.Count)
        {
            return;
        }

        StopSpawn(argSlotIndex, argPrevTargetId);
        
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
            while ((_getGold?.Invoke() ?? 0) < goldCost)
            {
                yield return null;
            }
            
            _consumeGold?.Invoke(goldCost);
            float bonus = _getProductionBonus?.Invoke() ?? 0f;
            float productionTime = entityInfo.productionTime * (1 - bonus);
            float elapsedTime = 0f;
            
            while (elapsedTime < productionTime) {
                elapsedTime += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsedTime / productionTime);
                slot.SetProgress(progress);
                yield return null;
            }
            
            slot.SetProgress(0);
            Spawn(entityInfo, slot.Grade);
            yield return null;
        }
    }
    
    public void ForceSpawn(List<EntityInfo> entityInfoList, Grade argGrade)
    {
        StartCoroutine(CoForceSpawn(entityInfoList, argGrade));
    }
    
    IEnumerator CoForceSpawn(List<EntityInfo> entityInfoList, Grade argGrade)
    {
        var wait = new WaitForSeconds(_enemySpawnWaitTime);
        foreach (var info in entityInfoList)
        {
            Spawn(info, argGrade);

            yield return wait;
        }
    }
    
    void Spawn(EntityInfo argEntityInfo, Grade argGrade)
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
            var randomYOffset = UnityEngine.Random.Range(-_spawnerPosYOffset, _spawnerPosYOffset);
            entityObj.transform.position = new Vector2(transform.position.x, transform.position.y + randomYOffset);
            entityObj.transform.SetParent(_entityParent);
            var uid = Managers.Game.GetNewUid();
            entityObj.name = $"{argEntityInfo.id}_{uid}";
            var entity = entityObj.GetComponent<AEntity>();
            entity.Init(uid, _team, argEntityInfo, argGrade, _targetTransform, DestroyEntity, _earnGold);
            
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

    public void SetOnEntityCountChanged(Action<int> argOnEntityCountChanged)
    {
        _entityCountChanged = argOnEntityCountChanged;
    }
    
    void AddEntity(AEntity argEntity)
    {
        Type type = argEntity.GetType();
        if (!_entityDict.ContainsKey(type))
        {
            _entityDict[type] = new HashSet<AEntity>();
        }
        _entityDict[type].Add(argEntity);
        _entityCountChanged?.Invoke(GetEntitiesCount());
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
        _entityCountChanged?.Invoke(GetEntitiesCount());
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

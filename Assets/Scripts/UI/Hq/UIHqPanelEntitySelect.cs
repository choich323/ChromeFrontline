using System;
using System.Collections.Generic;
using UnityEngine;

public class UIHqPanelEntitySelect : AUIHqPanelSelect
{
    [SerializeField] private Transform _entityParent;
    
    private List<UIEntityUnit> _entityUnitList = new List<UIEntityUnit>();

    public override void SetType()
    {
        _panelType = HqRightPanelType.Entity;
    }
    
    public override void SetPanel()
    {
        DestroyEntityUnits();
        CreateEntityUnits();
    }

    void CreateEntityUnits()
    {
        var unlockEntityList = Managers.Game.GetUnlockEntityIDList();
        foreach (var entityId in unlockEntityList)
        {
            CreateEntityUnit(entityId);
        }
    }

    void CreateEntityUnit(PrefabID argPrefabID)
    {
        var entityUnitObj = Managers.Pool.Instantiate(PrefabID.UIEntityUnit);
        if (entityUnitObj == null)
            return;

        entityUnitObj.transform.SetParent(_entityParent);
        entityUnitObj.transform.localScale = Vector3.one;
        var entityUnit = entityUnitObj.GetComponent<UIEntityUnit>();
        entityUnit.Init(_transitionContent.lane, _transitionContent.slotIndex, argPrefabID, OnSelectEntityUnit);
        _entityUnitList.Add(entityUnit);
    }

    void OnSelectEntityUnit(PrefabID argPrefabID)
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)_transitionContent.lane);
        if (spawner == null) return;
        
        var slot = spawner.GetSlot(_transitionContent.slotIndex);
        if (slot == null) return;
        
        slot.ChangeTarget(argPrefabID);
        
        _goBack?.Invoke();
    }
    
    void DestroyEntityUnits()
    {
        foreach (var entityUnit in _entityUnitList)
        {
            Managers.Pool.Destroy(entityUnit, PrefabID.UIEntityUnit);
            entityUnit.Destroy();
        }
        _entityUnitList.Clear();
    }
    
    public override void Destroy()
    {
        DestroyEntityUnits();
        _transitionContent = null;
    }
}

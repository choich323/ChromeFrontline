using System.Collections;
using UnityEngine;using System.Collections.Generic;
using UnityEngine.UI;

public class UIWorldSelect : APopup
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private Transform _contentParent; // ScrollRect의 Content 객체
    
    private List<UIWorldSelectUnit> _unitList = new List<UIWorldSelectUnit>();

    public override void Init()
    {
        base.Init();
        RefreshList(true);
    }

    void RefreshList(bool argIsInit = false)
    {
        ClearUnitList();

        var catalog = Managers.Data.WorldCatalog;
        var ur = Managers.Game.UserRecord;
        var curWorldId = argIsInit ? Managers.Data.GetWorldId(ur.MaxUnlockedWorld) : ur.CurrentWorldId;
        UIWorldSelectUnit selectedUnit = null;
        
        var worldCount = ur.MaxUnlockedWorld;
        for (int i = 0; i < worldCount; i++)
        {
            string worldId = catalog.GetWorldIdByIndex(i);
            string worldName = catalog.GetWorldName(i);
            
            var obj = Managers.Pool.Instantiate(PrefabID.UIWorldSelectUnit);
            obj.transform.SetParent(_contentParent, false);
            obj.transform.SetSiblingIndex(i);
            
            var unit = obj.GetComponent<UIWorldSelectUnit>();
            unit.Init(worldId, i+1, worldName, OnSelectWorld);
            _unitList.Add(unit);

            bool isSelectedUnit = curWorldId == worldId;
            SetSelectedColor(unit, isSelectedUnit);
            if(isSelectedUnit)
            {
                selectedUnit = unit;
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
        
        _scrollRect.Focus((RectTransform)selectedUnit.transform);
    }

    void SetSelectedColor(UIWorldSelectUnit argUnit, bool argIsEnable)
    {
        argUnit.EnableSelectedColor(argIsEnable);
    }

    UIWorldSelectUnit GetWorldUnit(string argWorldId)
    {
        return _unitList.Find(unit => unit.WorldId == argWorldId);
    }
    
    void OnSelectWorld(string argSelectedWorldId)
    {
        var gm = Managers.Game;
        var userRecord = gm.UserRecord;
        var prevWorldId = userRecord.CurrentWorldId;

        if (argSelectedWorldId == prevWorldId)
        {
            return;
        }

        var selectedWorld = Managers.Data.GetWorldNumber(argSelectedWorldId);
        if (selectedWorld > userRecord.MaxUnlockedWorld)
        {
            return;
        }
        
        GetWorldUnit(prevWorldId).EnableSelectedColor(false);
        
        userRecord.SetCurrentWorldId(argSelectedWorldId);
        gm.SaveUserRecord(userRecord);

        Managers.Lobby.RefreshLobbyMap(argSelectedWorldId);
    }

    void ClearUnitList()
    {
        foreach (var unit in _unitList)
        {
            Managers.Pool.Destroy(unit, PrefabID.UIWorldSelectUnit);
        }
        _unitList.Clear();
    }
    
    public override void Clear()
    {
        base.Clear();

        ClearUnitList();
    }   
}

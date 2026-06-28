using UnityEngine;using System.Collections.Generic;

public class UIWorldSelect : APopup
{
    [SerializeField] private Transform _contentParent; // ScrollRect의 Content 객체
    
    private List<UIWorldSelectUnit> _unitList = new List<UIWorldSelectUnit>();

    public override void Init()
    {
        base.Init();
        RefreshList();
    }

    void RefreshList()
    {
        ClearUnitList();

        var catalog = Managers.Data.WorldCatalog;
        
        int worldCount = catalog.GetWorldCount();
        for (int i = 0; i < worldCount; i++)
        {
            string worldId = catalog.GetWorldId(i);
            string worldName = catalog.GetWorldName(i);
            
            var obj = Managers.Pool.Instantiate(PrefabID.UIWorldSelectUnit);
            obj.transform.SetParent(_contentParent, false);
            
            var unit = obj.GetComponent<UIWorldSelectUnit>();
            unit.Init(worldId, i+1, worldName, OnSelectWorld);
            _unitList.Add(unit);
        }
    }

    void OnSelectWorld(string argSelectedWorldId)
    {
        var gm = Managers.Game;
        var userRecord = gm.UserRecord;
        userRecord.SetCurrentWorldId(argSelectedWorldId);
        gm.SaveUserRecord(userRecord);

        Managers.Lobby.RefreshLobbyMap();
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

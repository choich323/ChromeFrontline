using System;
using UnityEngine;

public class UIHqPanelEntitySelect : AUIHqPanelSelect
{
    public override void SetPanel()
    {
        _panelType = HqRightPanelType.Entity;
    }

    public override void Destroy()
    {
        
    }
}

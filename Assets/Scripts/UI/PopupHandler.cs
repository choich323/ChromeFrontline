using UnityEngine;
using System.Collections.Generic;
using System;

public class PopupHandler
{
    private Stack<APopup> _popupStack = new Stack<APopup>();
    private Dictionary<Type, GameObject> _popupDict = new Dictionary<Type, GameObject>();

    public void Init()
    {
        
    }

    public void OnUpdate()
    {
        HandleEscapeKey();
    }

    void HandleEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_popupStack.Count > 0)
            {
                var top = _popupStack.Peek();
                if (top.InputMode == PopupInputMode.Modeless)
                {
                    top.Close();
                }
            }
            else
            {
                Managers.Game.ExitConfirm();
            }
        }
    }

    public APopup Top()
    {
        return _popupStack.Peek();
    }
    
    public T OpenPopup<T>(PrefabID argPrefabID) where T : APopup
    {
        T popup = null;
        
        if (!_popupDict.TryGetValue(typeof(T), out var obj))
        {
            obj = Managers.UI.InstantiateUIWithoutPool(argPrefabID);
            if (obj == null)
            {
                Debug.LogError("Popup Object is null. Can't show popup.");
                return null;
            }
            _popupDict.Add(typeof(T), obj);
        }
        
        obj.SetActive(true);
        var rect = obj.GetComponent<RectTransform>();
        Managers.UI.AttachToPopupParent(rect);
        
        popup = obj.GetComponent<T>();

        if (popup.StackAction == PopupStackAction.Exclusive && _popupStack.Count > 0)
        {
            foreach (var p in _popupStack)
            {
                p.gameObject.SetActive(false);
            }
        }
        
        _popupStack.Push(popup);
        popup.Init();
        popup.Open();

        CheckVisibility();
        
        return popup;
    }

    public void ClosePopup()
    {
        if (_popupStack.Count <= 0) return;
        
        APopup popup = _popupStack.Pop();
        popup.gameObject.SetActive(false);
        
        CheckVisibility();
    }

    void CheckVisibility()
    {
        if (_popupStack.Count <= 0) return;
        
        var popups = _popupStack.ToArray();
        bool isExclusiveFound = false;
        for (int i = 0; i < popups.Length; i++)
        {
            var cur = popups[i];
            if (isExclusiveFound)
            {
                cur.gameObject.SetActive(false);
            }
            else
            {
                cur.gameObject.SetActive(true);

                if (cur.StackAction == PopupStackAction.Exclusive)
                {
                    isExclusiveFound = true;
                }
            }
        }
    }

    public void CloseAllPopup()
    {
        while (_popupStack.Count > 0)
        {
            ClosePopup();
        }
    }
}

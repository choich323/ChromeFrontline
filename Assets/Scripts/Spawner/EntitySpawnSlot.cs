using System;
using UnityEngine;

public class EntitySpawnSlot
{
    private const PrefabID INVALID_TARGET_ID = PrefabID.None;
    private const float DEFALUT_PROGRESS = 0f;
    private const int INVALID_SLOT_INDEX = -1;

    private int _slotIndex;
    private PrefabID _targetId;
    private float _progress;
    private Action<int> _onTargetChange;
    
    private Action<int, float> _onSlotProgressChanged;
    public event Action<int, float> OnSlotProgressChanged
    {
        add => _onSlotProgressChanged += value;
        remove => _onSlotProgressChanged -= value;
    }
    
    public void Init(int argSlotIndex, Action<int> argOnTargetChange)
    {
        ResetSlot();

        _slotIndex = argSlotIndex;
        _onTargetChange = argOnTargetChange;
    }

    public void ChangeTarget(PrefabID argTargetId)
    {
        if (argTargetId == _targetId)
        {
            return;
        }
        _progress = DEFALUT_PROGRESS;
        _targetId = argTargetId;
        _onTargetChange?.Invoke(_slotIndex);
    }
    
    public void SetTargetId(PrefabID argId)
    {
        _targetId = argId;
    }

    public void SetProgress(float argProgress)
    {
        _progress = argProgress;
        _onSlotProgressChanged?.Invoke(_slotIndex, argProgress);
    }

    public PrefabID GetTargetId()
    {
        return _targetId;
    }

    public float GetProgress()
    {
        return _progress;
    }
    
    public void ResetSlot()
    {
        _slotIndex = INVALID_SLOT_INDEX;
        _targetId = INVALID_TARGET_ID;
        _progress = DEFALUT_PROGRESS;
        _onTargetChange = null;
    }

    public void Destroy()
    {
        ResetSlot();
    }
}

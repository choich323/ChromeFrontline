using System;
using System.Collections;
using UnityEngine;

public class SkillHandler
{
    private SkillActionExecutor _skillActionExecutor;
    private Action<IEnumerator> _runCoroutine;
    
    public void Run(Action<IEnumerator> argRunCoroutine)
    {
        _runCoroutine = argRunCoroutine;
        RunSkillActionExecutor();
    }

    void RunSkillActionExecutor()
    {
        if (_skillActionExecutor != null)
        {
            _skillActionExecutor.Destroy();
        }
        
        _skillActionExecutor = new SkillActionExecutor();
        _skillActionExecutor.Run();
    }

    public bool ExecuteSkill(int argId, Vector2 argPos)
    {
        

        return true;
    } 
    
    public void Destroy()
    {
        
    }
}

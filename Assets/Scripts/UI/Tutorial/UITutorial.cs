using System.Collections.Generic;
using UnityEngine;

public class UITutorial : MonoBehaviour
{
    [SerializeField] private UITutorialDimmer _dimmer;

    public void SetHole(List<RectTransform> argHoleList)
    {
        _dimmer.SetTargets(argHoleList);
    }
}

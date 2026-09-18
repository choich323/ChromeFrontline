using UnityEngine;
using UnityEngine.UI;

public class UISkillSelectButton : MonoBehaviour
{
    [SerializeField] private Button _btn;

    public void Init()
    {
        _btn.onClick.RemoveListener(OnClick);
        _btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        
    }
}

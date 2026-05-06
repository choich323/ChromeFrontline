using UnityEngine;

[CreateAssetMenu(fileName = "BGMData", menuName = "Custom/Sound/BGMData")]
public class BGMData : ScriptableObject
{
    [System.Serializable]
    public struct BGMInfo
    {
        public string name;
        public AudioClip clip;
    }

    public BGMInfo[] bgmList;

    // 이름으로 특정 클립을 찾는 헬퍼 메서드
    public AudioClip GetClip(string bgmName)
    {
        foreach (var bgm in bgmList)
        {
            if (bgm.name == bgmName) return bgm.clip;
        }
        return null;
    }
}

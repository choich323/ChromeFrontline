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

    public AudioClip GetClip(string bgmName)
    {
        foreach (var bgm in bgmList)
        {
            if (bgm.name == bgmName) return bgm.clip;
        }
        return null;
    }
}

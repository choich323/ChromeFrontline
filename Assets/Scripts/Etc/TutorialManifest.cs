using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TutorialManifest", menuName = "Custom/Tutorial/TutorialManifest")]
public class TutorialManifest : ScriptableObject
{
    public List<string> tutorialOrderList = new List<string>();

    public string GetNextTutorial(string argCurTutorial)
    {
        var index = tutorialOrderList.IndexOf(argCurTutorial);
        if (index + 1 >= tutorialOrderList.Count)
        {
            return string.Empty;
        }
        
        return tutorialOrderList[index + 1];
    }
}

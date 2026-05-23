using System;
using System.IO;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    private string _filePath;
    
    public void Awake()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "UserSave.sav");
    }

    public void Init()
    {
        
    }
    
    public void SaveRecord(UserRecord argUserRecord)
    {
        try
        {
            string json = JsonConvert.SerializeObject(argUserRecord);
            
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            string encodedJson = Convert.ToBase64String(bytes);
            
            File.WriteAllText(_filePath, encodedJson);
            Debug.Log("SaveData Complete.");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveData Failed. message:{e.Message}");
        }
    }

    public UserRecord LoadRecord()
    {
        if (!File.Exists(_filePath))
        {
            Debug.Log("No saved Data. Create a new file.");
            var newRecord = new UserRecord();
            newRecord.Init();
            SaveRecord(newRecord);
            return newRecord;
        }

        try
        {
            Debug.Log("[SaveManager]Try LoadUserSaveData");
            string encodedJson = File.ReadAllText(_filePath);
            
            byte[] bytes = Convert.FromBase64String(encodedJson);
            string json = Encoding.UTF8.GetString(bytes);
            
            var record = JsonConvert.DeserializeObject<UserRecord>(json);
            Debug.Log("[SaveManager]Load complete.");
            return record;
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadData failed. message:{e.Message}");
            return new UserRecord();
        }
    }
}

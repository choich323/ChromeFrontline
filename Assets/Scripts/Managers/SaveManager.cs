using System;
using System.IO;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.Security.Cryptography;

public class SaveManager : MonoBehaviour
{
    private string _filePath;
    
    private readonly byte[] _aesKey = Encoding.UTF8.GetBytes("PioneerRevoltDefense2026AES256!!"); 
    private readonly byte[] _aesIv  = Encoding.UTF8.GetBytes("InitVector16Byte");
    
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
            
            string encryptedJson = EncryptAES(json);
            
            File.WriteAllText(_filePath, encryptedJson);
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
            var newRecord = new UserRecord();
            SaveRecord(newRecord);
            return newRecord;
        }

        try
        {
            string encryptedJson = File.ReadAllText(_filePath);
            
            // 복호화 시도
            string decryptedJson = DecryptAES(encryptedJson);
            
            var record = JsonConvert.DeserializeObject<UserRecord>(decryptedJson);
            return record;
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadData failed. message:{e.Message}");
            return new UserRecord();
        }
    }
    
    private string EncryptAES(string plainText)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = _aesKey;
        aesAlg.IV = _aesIv;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
        
        swEncrypt.Write(plainText);
        swEncrypt.Close();
        
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private string DecryptAES(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = _aesKey;
        aesAlg.IV = _aesIv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new MemoryStream(cipherBytes);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new StreamReader(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }
}

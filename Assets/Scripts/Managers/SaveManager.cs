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
        _filePath = Path.Combine(Application.persistentDataPath, "ChromeFrontline_UserSave.sav");
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
    
    private string EncryptAES(string argPlainText)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = _aesKey;
        aesAlg.IV = _aesIv;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        // using 블록을 중첩하여 스코프가 끝날 때 자동으로 FlushFinalBlock이 안전하게 호출되도록 처리
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(argPlainText);
        } 
    
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private string DecryptAES(string argCipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(argCipherText);

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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

[ExecuteInEditMode]
public class Saver : MonoBehaviour
{
    private static Saver instance;

    public static Saver Instance { get { if (instance == null) instance = FindObjectOfType<Saver>(); return instance; } }

    

    void Awake()
    {
        if (instance != this && instance != null && Application.isPlaying)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public T Load<T>(int slot)
    {
        var save = LoadSave<T>(slot);
        if (save == null)
            return default; // failed
        return save;
    }

    public bool SlotUsed(int slot)
    {
        return
            File.Exists(Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat");
    }

    private T LoadSave<T>(int slot)
    {
        // most of the structure is from https://videlais.com/2021/02/28/encrypting-game-data-with-unity/

        if (File.Exists(Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat") && PlayerPrefs.HasKey("saveKey" + slot))
        {
            // Update key based on PlayerPrefs
            // (Convert the String into a Base64 byte[] array.)
            byte[] savedKey = Convert.FromBase64String(PlayerPrefs.GetString("saveKey" + slot));

            // Create FileStream for opening files.
            var file = new FileStream(Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat", FileMode.Open);

            // Create new AES instance.
            Aes oAes = Aes.Create();

            // Create an array of correct size based on AES IV.
            byte[] outputIV = new byte[oAes.IV.Length];

            // Read the IV from the file.
            file.Read(outputIV, 0, outputIV.Length);

            // Create CryptoStream, wrapping FileStream
            CryptoStream oStream = new CryptoStream(
                   file,
                   oAes.CreateDecryptor(savedKey, outputIV),
                   CryptoStreamMode.Read);

            var reader = new StreamReader(oStream);
            var str = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(str);
        }
        return default;
    }
    #region Save

    public void Save(int slot, dynamic content)
    {
        SaveSave(content, slot);
    }
    public string[] GetPath(int slot)
    {
        return PathPath(slot);
    }

    public string GetPathOnly(int slot)
    {
        return PathPathS(slot);
    }


    public void SetKey(int slot, string key)
    {
        KeyKey(slot, key);
    }

    public void Clear(int slot)
    {
        ClearClear(slot);
    }

    private void ClearClear(int slot)
    {
        if(!File.Exists(Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat"))
            return;
        File.Delete(Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat");
        
    }

    private void KeyKey(int slot, string key)
    {
        PlayerPrefs.SetString("saveKey" + slot, key);
    }

    private string PathPathS(int slot)
    {
        return Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat";
    }
    private string[] PathPath(int slot)
    {
        return new string[]{
            Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat",
            PlayerPrefs.GetString("saveKey" + slot) };
    }

    private void SaveSave<T>(T save, int slot)
    {
        // most of the structure is from https://videlais.com/2021/02/28/encrypting-game-data-with-unity/
        Aes iAes = Aes.Create();

        FileStream file = File.Create(Application.persistentDataPath
                     + "/BabblesSave" + slot + ".dat");


        byte[] savedKey = iAes.Key;

        PlayerPrefs.SetString("saveKey" + slot, Convert.ToBase64String(savedKey));

        byte[] inputIV = iAes.IV;

        file.Write(inputIV, 0, inputIV.Length);

        CryptoStream iStream = new CryptoStream(
              file,
              iAes.CreateEncryptor(iAes.Key, iAes.IV),
              CryptoStreamMode.Write);

        StreamWriter sWriter = new StreamWriter(iStream);

        var data = JsonConvert.SerializeObject(save);

        sWriter.Write(data);
        sWriter.Close();
        iStream.Close();
        file.Close();
    }

    #endregion Save


    public static void Replace<T>(T x, T y)
    where T : class
    {
        // replaces 'x' with 'y'
        if (x == null) throw new ArgumentNullException("x");
        if (y == null) throw new ArgumentNullException("y");

        var size = Marshal.SizeOf(typeof(T));
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(y, ptr, false);
        Marshal.PtrToStructure(ptr, x);
        Marshal.FreeHGlobal(ptr);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler 
{
    private string dataDirPath = "";

    private string datafileName = "";

    public FileDataHandler(string dataDirPath, string datafileName)
    {
        this.dataDirPath = dataDirPath;
        this.datafileName = datafileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, datafileName);
        GameData loadData = null; 
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e) 
            {
                Debug.LogError("Error Occured when trying to load the file " + fullPath + "\n" + e);
            }
        }
        return loadData;
    }
    public void Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, datafileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error Occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}


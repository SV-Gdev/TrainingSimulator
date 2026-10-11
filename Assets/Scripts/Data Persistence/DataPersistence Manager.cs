using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string defaultFileName = "slot1.json";

    private GameData gameData;
    private FileDataHandler fileDataHandler;
    private string selectedProfileId = "1";

    [HideInInspector] public bool skipLoadOnce = false;

    public static DataPersistenceManager Instance { get; private set; }

    
    private List<IDataPersistence> dataPersistenceObjects = new List<IDataPersistence>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        EnsureFileHandler();
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Start()
    {
        if (gameData == null && !IsMenuScene(SceneManager.GetActiveScene()))
        {
            LoadGame();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[DataPersistenceManager] Scene loaded: '{scene.name}'.");

        if (IsMenuScene(scene)) return;

        if (skipLoadOnce)
        {
            NewGame();
            skipLoadOnce = false;
            PushDataToObjects();
        }
        else
        {
            LoadGame();
        }
    }

    private bool IsMenuScene(Scene scene)
    {
        return scene.name.Contains("Menu");
    }

    public void RegisterDataPersistence(IDataPersistence dataPersistenceObj)
    {
        if (dataPersistenceObj != null && !dataPersistenceObjects.Contains(dataPersistenceObj))
        {
            dataPersistenceObjects.Add(dataPersistenceObj);
        }
    }

    public void UnregisterDataPersistence(IDataPersistence dataPersistenceObj)
    {
        if (dataPersistenceObj != null && dataPersistenceObjects.Contains(dataPersistenceObj))
        {
            dataPersistenceObjects.Remove(dataPersistenceObj);
        }
    }

    private void EnsureFileHandler()
    {
        if (string.IsNullOrEmpty(selectedProfileId))
        {
            selectedProfileId = "1";
        }

        if (fileDataHandler == null)
        {
            fileDataHandler = new FileDataHandler(Application.persistentDataPath, "slot" + selectedProfileId + ".json");
        }
    }

    public void ChangeSelectedProfileSlot(int slotID)
    {
        selectedProfileId = slotID.ToString();
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, "slot" + selectedProfileId + ".json");

        LoadGame();
    }

    public GameData GetGameDataForSlot(int slotID)
    {
        string profileName = "slot" + slotID;
        FileDataHandler temporaryHandler = new FileDataHandler(Application.persistentDataPath, profileName + ".json");
        return temporaryHandler.Load();
    }

    public bool CurrentSlotHasSaveFile()
    {
        string profile = string.IsNullOrEmpty(selectedProfileId) ? "slot1" : "slot" + selectedProfileId;
        string fullPath = System.IO.Path.Combine(Application.persistentDataPath, profile + ".json");

        return System.IO.File.Exists(fullPath);
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        EnsureFileHandler();

        this.gameData = fileDataHandler.Load();

        if (this.gameData == null)
        {
            NewGame();
        }
        else
        {
            Debug.Log($"[DataPersistenceManager] Save file successfully loaded for Slot {selectedProfileId}.");
        }

        PushDataToObjects();
    }

    public void SaveGame()
    {
        EnsureFileHandler();

        if (this.gameData == null)
        {
            NewGame();
        }

        // Iterate directly over registered objects copy
        List<IDataPersistence> objects = new List<IDataPersistence>(dataPersistenceObjects);
       

        foreach (IDataPersistence obj in objects)
        {
            try
            {
                obj.SaveData(ref this.gameData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataPersistenceManager] SaveData failed on {obj}: {e}");
            }
        }

        fileDataHandler.Save(this.gameData);
       
    }

    private void PushDataToObjects()
    {
        if (this.gameData == null) return;

        // Iterate directly over registered objects copy
        foreach (IDataPersistence obj in new List<IDataPersistence>(dataPersistenceObjects))
        {
            try
            {
                obj.LoadData(this.gameData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataPersistenceManager] LoadData failed on {obj}: {e}");
            }
        }
    }
}
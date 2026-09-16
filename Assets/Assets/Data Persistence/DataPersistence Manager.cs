using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName = "gamesave.json";

    private GameData gameData;
    // FIXED: Initialized the list immediately so scripts can safely register themselves
    private List<IDataPersistence> dataPersistenceObjects = new List<IDataPersistence>();
    private FileDataHandler fileDataHandler;
    private string selectedProfileId = "";

    [HideInInspector] public bool skipLoadOnce = false;

    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // FIXED: Clear stale references from the previous scene before the new scene objects wake up
        dataPersistenceObjects.Clear();

        if (skipLoadOnce)
        {
            NewGame();
            skipLoadOnce = false;

            // If any persistent objects initialized super early, push data to them
            PushDataToRegisteredObjects();
        }
        else
        {
            LoadGame();
        }
    }

    // =======================================================================
    // NEW: Self-Registration Methods (Completely replaces "Find" methods)
    // =======================================================================
    public void RegisterDataPersistence(IDataPersistence dataPersistenceObj)
    {
        if (!dataPersistenceObjects.Contains(dataPersistenceObj))
        {
            dataPersistenceObjects.Add(dataPersistenceObj);
        }

        // If the game state has already loaded data from the disk, 
        // give it to this object immediately as it wakes up!
        if (gameData != null)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void UnregisterDataPersistence(IDataPersistence dataPersistenceObj)
    {
        dataPersistenceObjects.Remove(dataPersistenceObj);
    }

    public void ChangeSelectedProfileSlot(int slotID)
    {
        this.selectedProfileId = "slot" + slotID;
        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, this.selectedProfileId + ".json");
    }

    public void SaveGame()
    {
        if (string.IsNullOrEmpty(selectedProfileId)) ChangeSelectedProfileSlot(1);

        // FIXED: No longer searching the entire scene hierarchically!
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        fileDataHandler.Save(gameData);
    }

    public GameData GetGameDataForSlot(int slotID)
    {
        string profileName = "slot" + slotID;
        FileDataHandler temporaryHandler = new FileDataHandler(Application.persistentDataPath, profileName + ".json");
        return temporaryHandler.Load();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        this.gameData = fileDataHandler.Load();

        if (this.gameData == null)
        {
            NewGame();
        }

        PushDataToRegisteredObjects();
    }

    private void PushDataToRegisteredObjects()
    {
        if (this.dataPersistenceObjects != null)
        {
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(gameData);
            }
        }
    }

    public bool CurrentSlotHasSaveFile()
    {
        string profile = string.IsNullOrEmpty(selectedProfileId) ? "slot1" : selectedProfileId;
        string fullPath = System.IO.Path.Combine(Application.persistentDataPath, profile + ".json");

        return System.IO.File.Exists(fullPath);
    }

    // REMOVED: FindAllDataPersistenceObjects() is gone forever!
}
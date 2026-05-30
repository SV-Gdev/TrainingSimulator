using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName = "gamesave.json";

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
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
        

        this.dataPersistenceObjects = FindAllDataPersistenceObjects();

       
        if (skipLoadOnce)
        {
           
            NewGame();
            skipLoadOnce = false; 

            
            if (this.dataPersistenceObjects != null)
            {
                foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
                {
                    dataPersistenceObj.LoadData(gameData);
                }
            }
        }
        else
        {
            LoadGame();
        }
    }

    public void ChangeSelectedProfileSlot(int slotID)
    {
        this.selectedProfileId = "slot" + slotID;
        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, this.selectedProfileId + ".json");
    }

    public void SaveGame()
    {
        if (string.IsNullOrEmpty(selectedProfileId)) ChangeSelectedProfileSlot(1);

        this.dataPersistenceObjects = FindAllDataPersistenceObjects();

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
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
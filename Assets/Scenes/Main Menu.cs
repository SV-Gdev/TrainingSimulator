using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Config")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    
    public void StartNewGameSlot(int slotID)
    {
        if (DataPersistenceManager.Instance != null)
        {
           
            DataPersistenceManager.Instance.ChangeSelectedProfileSlot(slotID);

            
            DataPersistenceManager.Instance.NewGame();

            
            string fullPath = Path.Combine(Application.persistentDataPath, $"slot{slotID}.json");
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Debug.Log($"[Main Menu] Old Slot {slotID} save file deleted successfully.");
            }
        }

       
        SceneManager.LoadScene(gameplaySceneName);
    }

   
    public void LoadSavedGameSlot(int slotID)
    {
        if (DataPersistenceManager.Instance != null)
        {
           
            DataPersistenceManager.Instance.ChangeSelectedProfileSlot(slotID);

           
            DataPersistenceManager.Instance.LoadGame();

            Debug.Log($"[Main Menu] Successfully read file data. Continuing Checkpoint on Slot {slotID}.");
        }

        // 2. Switch to game world scene
        SceneManager.LoadScene(gameplaySceneName);
    }
    public void StartPureFreshGame()
    {
        if (DataPersistenceManager.Instance != null)
        {
           
            DataPersistenceManager.Instance.skipLoadOnce = true;

            
            DataPersistenceManager.Instance.NewGame();

            Debug.Log("[Main Menu] Booting a completely fresh run with default player position and stats.");
        }

       
        SceneManager.LoadScene(gameplaySceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game executed.");
    }
}
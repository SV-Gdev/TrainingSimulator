using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] string mainMenuScene = "MainMenu Scene";
    bool isPaused = false;

    [Header("Override Popup Elements")]
   
    [SerializeField] private GameObject overrideWarningPanel;

    void Start()
    {
        
        if (DataPersistenceManager.Instance == null)
        {
            DataPersistenceManager existingManager = FindObjectOfType<DataPersistenceManager>();

            if (existingManager == null)
            {
                Debug.Log("<color=yellow>[Save System]</color> No manager found anywhere. Creating fallback for scene testing.");
                GameObject fallbackManager = new GameObject("DataPersistenceManager_Fallback");
                fallbackManager.AddComponent<DataPersistenceManager>();
            }
        }

       
        if (overrideWarningPanel != null)
        {
            overrideWarningPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        
        if (overrideWarningPanel != null) overrideWarningPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

   
    public void OnClickSaveButton()
    {
        if (DataPersistenceManager.Instance != null)
        {
            
            if (DataPersistenceManager.Instance.CurrentSlotHasSaveFile())
            {
                
                if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

              
                if (overrideWarningPanel != null)
                {
                    overrideWarningPanel.SetActive(true);
                }
                
            }
            else
            {
               
                ExecuteSavePipeline();
            }
        }
        else
        {
            Debug.LogWarning("[Save System] Cannot save! DataPersistenceManager is missing from the scene.");
        }
    }

   
    public void ClickedConfirmOverrideYes()
    {
        if (overrideWarningPanel != null) overrideWarningPanel.SetActive(false);

       
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

       
        ExecuteSavePipeline();
    }

    public void ClickedCancelOverrideNo()
    {
        if (overrideWarningPanel != null) overrideWarningPanel.SetActive(false);

       
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

       
    }

   
    private void ExecuteSavePipeline()
    {
        if (DataPersistenceManager.Instance != null)
        {
           
            DataPersistenceManager.Instance.SaveGame();
            Debug.Log("<color=green>[Save System]</color> Game saved successfully!");
        }
    }

    public void MainMenu()
    {
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.SaveGame();
            Debug.Log("[Pause Menu] Auto-saving progress prior to exiting to Main Menu.");
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Quit()
    {
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.SaveGame();
        }

        Time.timeScale = 1f;
        Application.Quit();
        SceneManager.LoadScene(mainMenuScene);
        Debug.Log("Quit");
    }
}
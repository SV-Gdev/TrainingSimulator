using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IDataPersistence
{
    public static GameManager Instance { get; private set; }

    [Header("Level Progress Flags")]
    public bool isLevel1Cleared = false;
    public bool isLevel2Cleared = false;

    [Header("Level 1 Object Arrays")]
    public GameObject[] level1Villagers;
    public GameObject[] level1Enemies;

    [Header("Level 2 Object Arrays")]
    public GameObject[] level2Villagers;
    public GameObject[] level2Enemies;

    [Header("Level 2 Blocking Collider")]
    public Collider level2Collider;

    [Header("UI & Level Cleared Panel")]
    public TMP_Text statusText;
    public GameObject levelClearedPanel;
    public Button saveButton;
    public Button dontSaveButton;

    [Header("Victory Panel UI")]
    public GameObject victoryPanel;
    public Button returnToMainMenuButton;
    [SerializeField] private string mainSceneName = "Main Menu Scene";

    [Header("Warehouse Interactions")]
    public Transform playerTransform;
    public WeaponSwap playerWeaponSwap;
    public Animator level1WarehouseAnim;
    public Animator level2WarehouseAnim;

    public Collider level1WarehouseDoorCollider;
    public Collider level2WarehouseDoorCollider;

    public float interactDistance = 15f;

    private bool isLevel1WarehouseOpen = false;
    private bool isLevel2WarehouseOpen = false;
    private bool hasLoadedData = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (DataPersistenceManager.Instance != null)
            DataPersistenceManager.Instance.RegisterDataPersistence(this);
    }

    private void OnDisable()
    {
        if (DataPersistenceManager.Instance != null)
            DataPersistenceManager.Instance.UnregisterDataPersistence(this);
    }

    public void LoadData(GameData data)
    {
        this.isLevel1Cleared = data.isLevel1Cleared;
        this.isLevel2Cleared = data.isLevel2Cleared;

        Debug.Log($"<color=cyan>[GameManager] LoadData called. isLevel1Cleared: {isLevel1Cleared} | isLevel2Cleared: {isLevel2Cleared}</color>");

        if (isLevel1Cleared)
        {
            if (level1Enemies != null) foreach (var e in level1Enemies) if (e != null) e.SetActive(false);
            if (level1Villagers != null) foreach (var v in level1Villagers) if (v != null) v.SetActive(false);
        }

        if (isLevel2Cleared)
        {
            if (level2Enemies != null) foreach (var e in level2Enemies) if (e != null) e.SetActive(false);
            if (level2Villagers != null) foreach (var v in level2Villagers) if (v != null) v.SetActive(false);
        }

        UpdateLevelState();
        hasLoadedData = true;
    }

    public void SaveData(ref GameData data)
    {
        data.isLevel1Cleared = this.isLevel1Cleared;
        data.isLevel2Cleared = this.isLevel2Cleared;

        int l1Killed = CountInactiveOrDestroyedObjects(level1Enemies);
        int l1Total = level1Enemies != null ? level1Enemies.Length : 0;
        int l2Killed = CountInactiveOrDestroyedObjects(level2Enemies);
        int l2Total = level2Enemies != null ? level2Enemies.Length : 0;

        data.remainingEnemiesCount = (l1Total - l1Killed) + (l2Total - l2Killed);

        Debug.Log($"<color=magenta>[GameManager] SaveData called. Saving isLevel1Cleared: {isLevel1Cleared}</color>");
    }

    private void Start()
    {
        UpdateLevelState();

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveGameClicked);
        }
        if (dontSaveButton != null)
        {
            dontSaveButton.onClick.AddListener(OnDontSaveClicked);
        }
        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (levelClearedPanel != null)
        {
            levelClearedPanel.SetActive(false);
        }
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    public void OnSaveGameClicked()
    {
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.SaveGame();
            Debug.Log("[GameManager] Save button clicked and executed via DataPersistenceManager.");
        }

        CloseLevelClearedPanel();
    }

    public void OnDontSaveClicked()
    {
        Debug.Log("[GameManager] Progress not saved.");
        CloseLevelClearedPanel();
    }

    private void CloseLevelClearedPanel()
    {
        if (levelClearedPanel != null)
        {
            levelClearedPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(mainSceneName);
    }

    private void Update()
    {
        if (!hasLoadedData) return;

        if (!isLevel1Cleared)
        {
            CheckLevel1Progress();
        }
        else if (!isLevel2Cleared)
        {
            CheckLevel2Progress();
        }

        UpdateLevelState();
        CheckWarehouseInteractions();
    }

    private void CheckWarehouseInteractions()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (playerTransform == null || playerWeaponSwap == null) return;

            if (level1WarehouseAnim != null)
            {
                float distToW1 = Vector3.Distance(playerTransform.position, level1WarehouseAnim.transform.position);
                if (distToW1 <= interactDistance)
                {
                    if (IsPlayerHoldingKey()) ToggleWarehouse1();
                    else Debug.Log("You need to equip the Key to open the Level 1 Warehouse!");
                }
            }

            if (level2WarehouseAnim != null)
            {
                float distToW2 = Vector3.Distance(playerTransform.position, level2WarehouseAnim.transform.position);
                if (distToW2 <= interactDistance)
                {
                    if (IsPlayerHoldingKey()) ToggleWarehouse2();
                    else Debug.Log("You need to equip the Key to open the Level 2 Warehouse!");
                }
            }
        }
    }

    private bool IsPlayerHoldingKey()
    {
        foreach (GameObject item in playerWeaponSwap.equippedWeapons)
        {
            if (item != null && item.activeInHierarchy && item.name.Contains("Key"))
            {
                return true;
            }
        }
        return false;
    }

    private void ToggleWarehouse1()
    {
        isLevel1WarehouseOpen = !isLevel1WarehouseOpen;
        if (isLevel1WarehouseOpen)
        {
            level1WarehouseAnim.Play("WareHOUSEOPENNN");
            if (level1WarehouseDoorCollider != null) level1WarehouseDoorCollider.enabled = false;
        }
        else
        {
            level1WarehouseAnim.Play("WareHOUSECLOSEEE");
            if (level1WarehouseDoorCollider != null) level1WarehouseDoorCollider.enabled = true;
        }
    }

    private void ToggleWarehouse2()
    {
        isLevel2WarehouseOpen = !isLevel2WarehouseOpen;
        if (isLevel2WarehouseOpen)
        {
            level2WarehouseAnim.Play("WareHOUSEOPENNN");
            if (level2WarehouseDoorCollider != null) level2WarehouseDoorCollider.enabled = false;
        }
        else
        {
            level2WarehouseAnim.Play("WareHOUSECLOSEEE");
            if (level2WarehouseDoorCollider != null) level2WarehouseDoorCollider.enabled = true;
        }
    }

    private void CheckLevel1Progress()
    {
        int killedEnemies = CountInactiveOrDestroyedObjects(level1Enemies);
        int savedVillagers = CountInactiveOrDestroyedObjects(level1Villagers);

        bool enemiesExist = level1Enemies != null && level1Enemies.Length > 0;
        bool villagersExist = level1Villagers != null && level1Villagers.Length > 0;

        bool enemiesCleared = !enemiesExist || killedEnemies >= level1Enemies.Length;
        bool villagersCleared = !villagersExist || savedVillagers >= level1Villagers.Length;

        if (enemiesCleared && villagersCleared && (enemiesExist || villagersExist))
        {
            isLevel1Cleared = true;
            UpdateLevelState();

            if (levelClearedPanel != null)
            {
                levelClearedPanel.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        UpdateUILevel1(killedEnemies, savedVillagers);
    }

    private void CheckLevel2Progress()
    {
        int killedEnemies = CountInactiveOrDestroyedObjects(level2Enemies);
        int savedVillagers = CountInactiveOrDestroyedObjects(level2Villagers);

        bool enemiesExist = level2Enemies != null && level2Enemies.Length > 0;
        bool villagersExist = level2Villagers != null && level2Villagers.Length > 0;

        bool enemiesCleared = !enemiesExist || killedEnemies >= level2Enemies.Length;
        bool villagersCleared = !villagersExist || savedVillagers >= level2Villagers.Length;

        if (enemiesCleared && villagersCleared && (enemiesExist || villagersExist))
        {
            isLevel2Cleared = true;
            UpdateUILevel2(killedEnemies, savedVillagers);

            
            if (victoryPanel != null && !victoryPanel.activeSelf)
            {
                victoryPanel.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            return;
        }

        UpdateUILevel2(killedEnemies, savedVillagers);
    }

    private int CountInactiveOrDestroyedObjects(GameObject[] array)
    {
        if (array == null || array.Length == 0) return 0;

        int inactiveCount = 0;
        foreach (GameObject obj in array)
        {
            if (obj == null || !obj.activeSelf)
            {
                inactiveCount++;
            }
        }
        return inactiveCount;
    }

    private void UpdateLevelState()
    {
        if (level2Collider != null)
        {
            level2Collider.enabled = !isLevel1Cleared;
        }
    }

    private void UpdateUILevel1(int killedEnemies, int savedVillagers)
    {
        if (statusText != null)
        {
            string textContent = "<b>[LEVEL 1]</b>";
            if (level1Enemies != null && level1Enemies.Length > 0)
                textContent += $"\nEnemies Killed: {killedEnemies}/{level1Enemies.Length}";
            if (level1Villagers != null && level1Villagers.Length > 0)
                textContent += $"\nVillagers Saved: {savedVillagers}/{level1Villagers.Length}";
            statusText.text = textContent;
        }
    }

    public void InflictLaserDamage()
    {
        if (playerTransform != null)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeContinuousDamage(10);
                Debug.Log("[GameManager] Player hit by security laser!");
            }
        }
    }

    private void UpdateUILevel2(int killedEnemies, int savedVillagers)
    {
        if (statusText != null)
        {
            if (isLevel2Cleared)
            {
                statusText.text = "<b>VICTORY!</b>\nAll Levels Cleared!";
            }
            else
            {
                string textContent = "<b>[LEVEL 2 UNLOCKED]</b>";
                if (level2Enemies != null && level2Enemies.Length > 0)
                    textContent += $"\nEnemies Killed: {killedEnemies}/{level2Enemies.Length}";
                if (level2Villagers != null && level2Villagers.Length > 0)
                    textContent += $"\nVillagers Saved: {savedVillagers}/{level2Villagers.Length}";
                statusText.text = textContent;
            }
        }
    }
}
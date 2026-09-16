using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
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

    [Header("UI Display")]
    public TMP_Text statusText;

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

    private void Start()
    {
        UpdateLevelState();
    }

    private void Update()
    {
        if (!isLevel1Cleared)
        {
            CheckLevel1Progress();
        }
        else if (!isLevel2Cleared)
        {
            CheckLevel2Progress();
        }
    }

    private void CheckLevel1Progress()
    {
        int killedEnemies = CountInactiveOrDestroyedObjects(level1Enemies);
        int savedVillagers = CountInactiveOrDestroyedObjects(level1Villagers);

        // Clears automatically once all enemies are killed and all villagers are saved
        if (killedEnemies >= level1Enemies.Length && level1Enemies.Length > 0 &&
            savedVillagers >= level1Villagers.Length && level1Villagers.Length > 0)
        {
            isLevel1Cleared = true;
            UpdateLevelState();
        }

        UpdateUILevel1(killedEnemies, savedVillagers);
    }

    private void CheckLevel2Progress()
    {
        int killedEnemies = CountInactiveOrDestroyedObjects(level2Enemies);
        int savedVillagers = CountInactiveOrDestroyedObjects(level2Villagers);

        if (killedEnemies >= level2Enemies.Length && level2Enemies.Length > 0 &&
            savedVillagers >= level2Villagers.Length && level2Villagers.Length > 0)
        {
            isLevel2Cleared = true;
            UpdateUILevel2(killedEnemies, savedVillagers);
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
            // Counts as killed/saved if the object is destroyed (null) or deactivated
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
            level2Collider.isTrigger = isLevel1Cleared;
        }
    }

    private void UpdateUILevel1(int killedEnemies, int savedVillagers)
    {
        if (statusText != null)
        {
            statusText.text = $"<b>[LEVEL 1]</b>\n" +
                              $"Enemies Killed: {killedEnemies}/{level1Enemies.Length}\n" +
                              $"Villagers Saved: {savedVillagers}/{level1Villagers.Length}";
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
                statusText.text = $"<b>[LEVEL 2 UNLOCKED]</b>\n" +
                                  $"Enemies Killed: {killedEnemies}/{level2Enemies.Length}\n" +
                                  $"Villagers Saved: {savedVillagers}/{level2Villagers.Length}";
            }
        }
    }
}
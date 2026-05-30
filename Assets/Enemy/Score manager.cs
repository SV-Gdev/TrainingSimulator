using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour, IDataPersistence
{
    [Header("UI Reference")]
    [Tooltip("Drag your 'New Text' UI GameObject into this slot.")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score Data")]
    private int totalScore = 0;

    private EnemyHealth enemyHealthSource;
    private EnemyArmor1 armoredEnemySource;

    void Start()
    {
        UpdateScoreUI();

        enemyHealthSource = FindObjectOfType<EnemyHealth>();
        if (enemyHealthSource != null)
        {
            enemyHealthSource.OnEnemyDeath += HandleEnemyDeathEvent;
        }

        armoredEnemySource = FindObjectOfType<EnemyArmor1>();
        if (armoredEnemySource != null)
        {
            armoredEnemySource.OnEnemyArmorDeath += HandleArmoredEnemyDeath;
        }
    }

    
    public void LoadData(GameData data)
    {
        this.totalScore = data.totalScore;
        UpdateScoreUI(); 
    }

    public void SaveData(ref GameData data)
    {
        data.totalScore = this.totalScore;
    }

    private void HandleEnemyDeathEvent(GameObject deadEnemy)
    {
        totalScore += 10;
        UpdateScoreUI();
        Debug.Log($"[Event Received] Base enemy died. Score is now: {totalScore}");
    }

    private void HandleArmoredEnemyDeath(int pointsGained)
    {
        totalScore += pointsGained;
        UpdateScoreUI();
        Debug.Log($"[Event Received] Armored enemy died. Score is now: {totalScore}");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore;
        }
        else
        {
            Debug.LogWarning("[ScoreManager] Missing UI Text component reference in the Inspector!");
        }
    }

    void OnDestroy()
    {
        if (enemyHealthSource != null)
        {
            enemyHealthSource.OnEnemyDeath -= HandleEnemyDeathEvent;
        }

        if (armoredEnemySource != null)
        {
            armoredEnemySource.OnEnemyArmorDeath -= HandleArmoredEnemyDeath;
        }
    }
}
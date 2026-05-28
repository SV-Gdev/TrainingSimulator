using UnityEngine;
using TMPro; // REQUIRED for TextMeshPro components. (Change to 'using UnityEngine.UI;' if using legacy Text)

public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Drag your 'New Text' UI GameObject into this slot.")]
    [SerializeField] private TextMeshProUGUI scoreText; // Change to 'private Text scoreText;' if using old UI Text

    [Header("Score Data")]
    private int totalScore = 0;

    private EnemyHealth enemyHealthSource;
    private EnemyArmor1 armoredEnemySource;

    void Start()
    {
        // Set initial score text display safely
        UpdateScoreUI();

        // Subscribe to base enemy death event
        enemyHealthSource = FindObjectOfType<EnemyHealth>();
        if (enemyHealthSource != null)
        {
            enemyHealthSource.OnEnemyDeath += HandleEnemyDeathEvent;
        }

        // Subscribe to armored enemy death event
        armoredEnemySource = FindObjectOfType<EnemyArmor1>();
        if (armoredEnemySource != null)
        {
            armoredEnemySource.OnEnemyArmorDeath += HandleArmoredEnemyDeath;
        }
    }

    // =======================================================================
    // OBSERVER CALLBACKS: Triggered automatically when death events fire
    // =======================================================================
    private void HandleEnemyDeathEvent(GameObject deadEnemy)
    {
        // For the base enemy, you can pass a flat value directly here, 
        // or add logic if you decide to pass a specific value from EnemyHealth later.
        totalScore += 10;

        UpdateScoreUI();
        Debug.Log($"[Event Received] Base enemy died. Score is now: {totalScore}");
    }

    private void HandleArmoredEnemyDeath(int pointsGained)
    {
        // Uses the exact modular score value passed directly from the armored enemy script
        totalScore += pointsGained;

        UpdateScoreUI();
        Debug.Log($"[Event Received] Armored enemy died. Score is now: {totalScore}");
    }

    // =======================================================================
    // UI REFRESH: Updates the text component on your screen layout
    // =======================================================================
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
        // Unsubscribe safely to protect memory performance layout
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
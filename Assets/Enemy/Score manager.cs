using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour, IDataPersistence
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private TextMeshProUGUI scoreText; // Optional UI reference

    private void Awake()
    {
        // Simple singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Subscribe to the static event on EnemyHealthManager
        EnemyHealthManager.OnEnemyKilled += AddScore;
    }

    private void Start()
    {
        // Register in Start() to guarantee DataPersistenceManager.Instance is fully initialized!
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.RegisterDataPersistence(this);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled to prevent memory leaks
        EnemyHealthManager.OnEnemyKilled -= AddScore;

        // Unregister from data persistence
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.UnregisterDataPersistence(this);
        }
    }

    public void LoadData(GameData data)
    {
        // Load the saved score from GameData when loading a slot or starting a new game
        this.currentScore = data.totalScore;
        UpdateUI();
    }

    public void SaveData(ref GameData data)
    {
        // Write the current score into GameData whenever the game saves
        data.totalScore = this.currentScore;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateUI();
        Debug.Log($"[ScoreManager] Score added: {amount}. Current Score: {currentScore}");
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    public int GetScore() => currentScore;
}
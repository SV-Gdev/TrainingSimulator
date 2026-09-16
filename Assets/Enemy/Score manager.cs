using UnityEngine;
using TMPro; // Include if using TextMeshPro for UI

public class ScoreManager : MonoBehaviour
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
        // Subscribe to the static event on EnemyHealthManager!
        // No Find calls or Inspector references required.
        EnemyHealthManager.OnEnemyKilled += AddScore;
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled to prevent memory leaks
        EnemyHealthManager.OnEnemyKilled -= AddScore;
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
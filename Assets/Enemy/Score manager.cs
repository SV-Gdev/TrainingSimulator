using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;

    [SerializeField] TextMeshProUGUI scoreText; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateScoreUI(); 
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI(); 
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }
}
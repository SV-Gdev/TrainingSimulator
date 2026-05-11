using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject deathPanel;    
    public Button restartButton; 

    bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;

        
        if (deathPanel != null) deathPanel.SetActive(false);

       
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthSlider.value = currentHealth;

        Debug.Log($"[Player] Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

       
        PlayerInput input = new PlayerInput();
        input.Onfoot.Disable();
        input.OnInventory.Disable();

       
        if (deathPanel != null)
            deathPanel.SetActive(true);

       
        Time.timeScale = 0f;
    }

    void RestartGame()
    {
       
        Time.timeScale = 1f;

       
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDataPersistence
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject deathPanel;
    public Button restartButton;

    private bool isDead = false;

    // =======================================================================
    // FIXED: Added Self-Registration lifecycle methods
    // =======================================================================
    private void OnEnable()
    {
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.RegisterDataPersistence(this);
        }
    }

    private void OnDisable()
    {
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.UnregisterDataPersistence(this);
        }
    }

    public void LoadData(GameData data)
    {
        // Pulls the saved health safely from your JSON profile
        this.currentHealth = (int)data.activeRunHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = this.currentHealth;
        }
    }

    public void SaveData(ref GameData data)
    {
        data.activeRunHealth = this.currentHealth;
    }

    void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }

        // If it's a completely fresh game slot, saved data will be 0.
        // In that specific case, initialize them to max health!
        if (this.currentHealth <= 0 && !isDead)
        {
            currentHealth = maxHealth;
        }

        if (healthSlider != null)
        {
            healthSlider.value = this.currentHealth;
        }

        if (deathPanel != null) deathPanel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

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
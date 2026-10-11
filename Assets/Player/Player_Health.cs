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

    [Header("Instant Death Hazard")]
    [SerializeField] private BoxCollider instantDeathCollider; // Drag your hazard's Box Collider here

    private bool isDead = false;

    // Reduced delay for standard hits
    private float damageCooldown = 0.5f;
    private float lastDamageTime;

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

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // Triggers instant death if the player touches the referenced box collider
        if (instantDeathCollider != null && other == instantDeathCollider)
        {
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // Collides instant death if the player touches the referenced box collider
        if (instantDeathCollider != null && collision.collider == instantDeathCollider)
        {
            Die();
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;

        ApplyDamage(amount);
    }

    // Dedicated method for continuous environmental hazards (like lasers) that bypasses standard hit cooldowns
    public void TakeContinuousDamage(int amount)
    {
        if (isDead) return;
        ApplyDamage(amount);
    }

    private void ApplyDamage(int amount)
    {
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

        if (deathPanel != null)
            deathPanel.SetActive(true);

        Time.timeScale = 0f;

        // Unlock and show cursor so the Restart button can be clicked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
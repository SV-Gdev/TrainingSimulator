using UnityEngine;
using UnityEngine.UI;

public class VillagerSaveInteract : MonoBehaviour
{
    [Header("Save Settings")]
    public float holdTimeToSave = 2f; 
    private float currentHoldTime = 0f;
    private bool isPlayerInRange = false;

    [Header("UI References")]
    public GameObject pressEText;
    public GameObject loadingScreenUI; 
    public Slider loadingSlider;        

    private void Start()
    {
        
        if (pressEText != null)
            pressEText.SetActive(false);

        if (loadingScreenUI != null)
            loadingScreenUI.SetActive(false);

        if (loadingSlider != null)
        {
            loadingSlider.value = 0f;
            loadingSlider.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInRange)
        {
            if (Input.GetKey(KeyCode.E))
            {
               
                if (pressEText != null) pressEText.SetActive(false);
                if (loadingScreenUI != null) loadingScreenUI.SetActive(true);
                if (loadingSlider != null) loadingSlider.gameObject.SetActive(true);

                currentHoldTime += Time.deltaTime;

                if (loadingSlider != null)
                {
                    loadingSlider.value = currentHoldTime / holdTimeToSave;
                }

                if (currentHoldTime >= holdTimeToSave)
                {
                    SaveVillager();
                }
            }
            else
            {
               
                if (currentHoldTime > 0)
                {
                    ResetProgress();
                    if (pressEText != null) pressEText.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (pressEText != null) pressEText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ResetProgress();
            if (pressEText != null) pressEText.SetActive(false);
        }
    }

    private void ResetProgress()
    {
        currentHoldTime = 0f;

       
        if (loadingScreenUI != null) loadingScreenUI.SetActive(false);
        if (loadingSlider != null)
        {
            loadingSlider.value = 0f;
            loadingSlider.gameObject.SetActive(false);
        }
    }

    private void SaveVillager()
    {
        ResetProgress();
        isPlayerInRange = false;

        gameObject.SetActive(false);
    }
}
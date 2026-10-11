using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class SaveSlotUIController : MonoBehaviour
{
    [Header("Menu Interface Panels")]
    [SerializeField] private GameObject mainMenuButtonsGroup;
    [SerializeField] private GameObject slotsPanelContainer;

    [Header("Slot UI Text Component Connections")]
    [SerializeField] private TextMeshProUGUI slot1DetailsText;
    [SerializeField] private TextMeshProUGUI slot2DetailsText;
    [SerializeField] private TextMeshProUGUI slot3DetailsText;

    [Header("Scene Configuration")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    
    private bool isLoadingGame = true;

    private void OnEnable()
    {
        RefreshSlotDisplays();
    }

    public void OnClickNewGameMenuButton()
    {
        isLoadingGame = false;
        OpenSlotsMenu();
    }

    public void OnClickLoadGameMenuButton()
    {
        isLoadingGame = true;
        OpenSlotsMenu();
    }

    private void OpenSlotsMenu()
    {
        if (mainMenuButtonsGroup != null) mainMenuButtonsGroup.SetActive(false);
        if (slotsPanelContainer != null) slotsPanelContainer.SetActive(true);
        RefreshSlotDisplays();
    }

    public void CloseSlotsMenu()
    {
        if (slotsPanelContainer != null) slotsPanelContainer.SetActive(false);
        if (mainMenuButtonsGroup != null) mainMenuButtonsGroup.SetActive(true);
    }

    public void RefreshSlotDisplays()
    {
        UpdateSingleSlotUI(1, slot1DetailsText);
        UpdateSingleSlotUI(2, slot2DetailsText);
        UpdateSingleSlotUI(3, slot3DetailsText);
    }

    private void UpdateSingleSlotUI(int slotID, TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        string filePath = Path.Combine(Application.persistentDataPath, $"slot{slotID}.json");

        if (!File.Exists(filePath))
        {
            textComponent.text = $"<b>SLOT {slotID}</b>\n<color=#888888>Empty Save Profile</color>";
            return;
        }

        if (DataPersistenceManager.Instance != null)
        {
            GameData slotData = DataPersistenceManager.Instance.GetGameDataForSlot(slotID);

            if (slotData == null)
            {
                textComponent.text = $"<b>SLOT {slotID}</b>\n<color=#888888>Empty Save Profile</color>";
            }
            else
            {
                string levelDisplay = "Level 1";
                if (slotData.isLevel2Cleared)
                {
                    levelDisplay = "Level 2 (Victory)";
                }
                else if (slotData.isLevel1Cleared)
                {
                    levelDisplay = "Level 2";
                }

                textComponent.text = $"<b>SLOT {slotID} — {levelDisplay}</b>\n" +
                                     $"<color=#FF5555>HP: {slotData.activeRunHealth}</color> | <color=#55FF55>Score: {slotData.totalScore}</color>\n" +
                                     $"Enemies Remaining: {slotData.remainingEnemiesCount}";
            }
        }
    }

    public void ClickedSelectSlot(int slotID)
    {
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.ChangeSelectedProfileSlot(slotID);
            string filePath = Path.Combine(Application.persistentDataPath, $"slot{slotID}.json");

            // If a save file exists, load it directly unless explicit new game sequence was forced
            if (File.Exists(filePath) && isLoadingGame)
            {
                Debug.Log($"[Slots UI] Loading saved progress on Slot {slotID}.");
                DataPersistenceManager.Instance.LoadGame();
            }
            else
            {
                Debug.Log($"[Slots UI] Starting a new run on Slot {slotID}.");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                DataPersistenceManager.Instance.NewGame();
            }

            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
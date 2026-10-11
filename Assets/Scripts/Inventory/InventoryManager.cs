using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDataPersistence
{
    [SerializeField] GameObject[] slots = new GameObject[20];
    [SerializeField] GameObject inventoryParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Camera cam;

    [Header("Save System")]
    [SerializeField] ItemSO[] allItems;

    GameObject draggedObject;
    GameObject lastItemSlot;

    bool isInventoryOpened;

    static readonly float[] RestorePassTimes = { 0.3f, 1.0f, 2.0f };
    const float MaxWaitForSwapper = 5f;

    bool pendingLoad = false;
    List<string> pendingItemNames = new List<string>();
    string pendingEquippedWeapon = "";
    float restoreElapsed = 0f;
    int restorePass = 0;

    void Start()
    {
    }

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

    void Update()
    {
        if (pendingLoad)
        {
            TickRestore();
        }

        inventoryParent.SetActive(isInventoryOpened);

        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isInventoryOpened)
            {
                Cursor.lockState = CursorLockMode.Locked;
                isInventoryOpened = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                isInventoryOpened = true;
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        List<string> names = new List<string>();

        for (int i = 0; i < slots.Length; i++)
        {
            string itemKey = "";

            if (slots[i] != null)
            {
                InventorySlot slot = slots[i].GetComponent<InventorySlot>();
                if (slot != null)
                {
                    GameObject held = slot.HeldItem;

                    if (held == null && draggedObject != null && slots[i] == lastItemSlot)
                    {
                        held = draggedObject;
                    }

                    if (held != null)
                    {
                        InventoryItem uiItem = held.GetComponent<InventoryItem>();
                        if (uiItem != null && uiItem.ItemScriptableObject != null)
                        {
                            itemKey = uiItem.ItemScriptableObject.name;
                        }
                    }
                }
            }

            names.Add(itemKey);
        }

        MergeCarriedWeaponsIntoList(names);

        data.inventoryItemNames = names;
    }

    void MergeCarriedWeaponsIntoList(List<string> names)
    {
        WeaponSwap swapper = FindObjectOfType<WeaponSwap>(true);
        if (swapper == null) return;

        if (allItems == null || allItems.Length == 0) return;

        foreach (ItemSO item in allItems)
        {
            if (item == null || item.equippedWeaponPrefab == null) continue;

            int inHand = 0;
            foreach (GameObject w in swapper.equippedWeapons)
            {
                if (w != null && CleanName(w.name) == item.equippedWeaponPrefab.name) inHand++;
            }

            int inList = names.Count(n => n == item.name);

            for (int k = inList; k < inHand; k++)
            {
                int emptyIndex = names.IndexOf("");
                if (emptyIndex >= 0) names[emptyIndex] = item.name;
                else names.Add(item.name);
            }
        }
    }

    string CleanName(string objectName)
    {
        return objectName.Replace("(Clone)", "").Trim();
    }

    public void LoadData(GameData data)
    {
        pendingItemNames = data.inventoryItemNames != null
            ? new List<string>(data.inventoryItemNames)
            : new List<string>();

        pendingEquippedWeapon = data.equippedWeaponName ?? "";

        restoreElapsed = 0f;
        restorePass = 0;
        pendingLoad = true;
    }

    void TickRestore()
    {
        restoreElapsed += Time.unscaledDeltaTime;

        if (restorePass >= RestorePassTimes.Length)
        {
            pendingLoad = false;
            return;
        }

        if (restoreElapsed < RestorePassTimes[restorePass]) return;

        WeaponSwap swapper = FindObjectOfType<WeaponSwap>(true);
        bool swapperReady = swapper != null && swapper.isActiveAndEnabled;

        if (!swapperReady)
        {
            if (restoreElapsed < MaxWaitForSwapper) return;

            RestoreAll(null);
            pendingLoad = false;
            return;
        }

        if (restorePass == 0)
        {
            RestoreAll(swapper);

            if (!string.IsNullOrEmpty(pendingEquippedWeapon))
            {
                swapper.EquipWeaponByName(pendingEquippedWeapon);
            }
        }
        else
        {
            int missing = CountMissingWeapons(swapper);
            if (missing > 0)
            {
                RestoreAll(swapper);

                if (!string.IsNullOrEmpty(pendingEquippedWeapon))
                {
                    swapper.EquipWeaponByName(pendingEquippedWeapon);
                }
            }
        }

        restorePass++;
    }

    public bool HasItem(ItemSO itemData)
    {
        if (itemData == null) return false;

        foreach (GameObject slotObj in slots)
        {
            if (slotObj == null) continue;
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot != null && slot.HeldItem != null)
            {
                InventoryItem uiItem = slot.HeldItem.GetComponent<InventoryItem>();
                if (uiItem != null && uiItem.ItemScriptableObject != null)
                {
                    if (uiItem.ItemScriptableObject.name == itemData.name ||
                        uiItem.ItemScriptableObject.itemName == itemData.itemName)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    bool IsItemAlreadyInInventory(ItemSO itemData)
    {
        return HasItem(itemData);
    }

    void RestoreAll(WeaponSwap swapper)
    {
        int savedCount = pendingItemNames.Count(n => !string.IsNullOrEmpty(n));
        ClearInventoryUI();

        Dictionary<string, int> seenCounts = new Dictionary<string, int>();
        HashSet<string> restoredItems = new HashSet<string>();
        int restored = 0;

        for (int i = 0; i < pendingItemNames.Count && i < slots.Length; i++)
        {
            string itemName = pendingItemNames[i];
            if (string.IsNullOrEmpty(itemName)) continue;
            if (slots[i] == null) continue;

            ItemSO itemData = FindItemByName(itemName);
            if (itemData == null) continue;

            if (restoredItems.Contains(itemData.name) || IsItemAlreadyInInventory(itemData)) continue;

            InventorySlot slot = slots[i].GetComponent<InventorySlot>();
            if (slot == null || slot.HeldItem != null) continue;

            GameObject newItem = Instantiate(itemPrefab, inventoryParent.transform);
            newItem.GetComponent<InventoryItem>().Initialize(itemData);
            slot.SetHeldItem(newItem);

            restoredItems.Add(itemData.name);
            EnsureWeaponInSwapper(swapper, itemData, seenCounts);
            restored++;
        }
    }

    int CountMissingWeapons(WeaponSwap swapper)
    {
        Dictionary<string, int> needed = new Dictionary<string, int>();

        foreach (string n in pendingItemNames)
        {
            if (string.IsNullOrEmpty(n)) continue;

            ItemSO item = FindItemByName(n);
            if (item == null || item.equippedWeaponPrefab == null) continue;

            string key = item.equippedWeaponPrefab.name;
            needed.TryGetValue(key, out int count);
            needed[key] = count + 1;
        }

        int missing = 0;
        foreach (KeyValuePair<string, int> pair in needed)
        {
            int have = swapper.equippedWeapons.Count(w => w != null && CleanName(w.name) == pair.Key);
            if (have < pair.Value) missing += pair.Value - have;
        }
        return missing;
    }

    void ClearInventoryUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            InventorySlot slot = slots[i].GetComponent<InventorySlot>();
            if (slot == null || slot.HeldItem == null) continue;

            Destroy(slot.HeldItem);
            slot.HeldItem = null;
        }
    }

    ItemSO FindItemByName(string searchKey)
    {
        if (allItems == null) return null;

        foreach (ItemSO item in allItems)
        {
            if (item != null && (item.name == searchKey || item.itemName == searchKey))
            {
                return item;
            }
        }
        return null;
    }

    void EnsureWeaponInSwapper(WeaponSwap swapper, ItemSO itemData, Dictionary<string, int> seenCounts)
    {
        if (swapper == null || itemData.equippedWeaponPrefab == null) return;

        string prefabName = itemData.equippedWeaponPrefab.name;

        seenCounts.TryGetValue(prefabName, out int seen);
        seenCounts[prefabName] = seen + 1;

        int existing = 0;
        foreach (GameObject w in swapper.equippedWeapons)
        {
            if (w != null && CleanName(w.name) == prefabName) existing++;
        }

        if (existing > seen) return;

        try
        {
            swapper.AddWeaponToInventory(itemData.equippedWeaponPrefab);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Inventory] Failed to give '{prefabName}' to WeaponSwap: {e}");
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Fill Item Database From Project")]
    void FillItemDatabase()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemSO");
        allItems = guids
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<ItemSO>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .Where(item => item != null)
            .ToArray();

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            if (clickedObject == null) return;

            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();
            if (slot != null && slot.HeldItem != null)
            {
                draggedObject = slot.HeldItem;
                slot.HeldItem = null;
                lastItemSlot = clickedObject;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (draggedObject != null && eventData.pointerCurrentRaycast.gameObject != null
            && eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;

            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();
            if (slot == null)
                slot = clickedObject.GetComponentInParent<InventorySlot>();

            if (slot != null && slot.HeldItem == null)
            {
                slot.SetHeldItem(draggedObject);
            }
            else if (slot != null && slot.HeldItem != null)
            {
                lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(slot.HeldItem);
                slot.SetHeldItem(draggedObject);
            }
            else if (clickedObject.name == "DropItem")
            {
                ItemSO itemData = draggedObject.GetComponent<InventoryItem>().ItemScriptableObject;

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 spawnPosition;

                if (Physics.Raycast(ray, out RaycastHit hit, 20f))
                    spawnPosition = hit.point + Vector3.up * 0.1f;
                else
                    spawnPosition = cam.transform.position + cam.transform.forward * 2f;

                GameObject newItem = Instantiate(itemData.equippedWeaponPrefab, spawnPosition, Quaternion.identity);
                newItem.GetComponent<ItemPickable>().itemScriptableObject = itemData;

                lastItemSlot.GetComponent<InventorySlot>().HeldItem = null;
                Destroy(draggedObject);
            }
            else
            {
                lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
            }

            draggedObject = null;
        }
    }

    public void ItemPicked(GameObject Pickeditem)
    {
        ItemPickable pickable = Pickeditem.GetComponent<ItemPickable>();
        if (pickable == null || pickable.itemScriptableObject == null) return;

        ItemSO itemData = pickable.itemScriptableObject;

        if (HasItem(itemData))
        {
            return;
        }

        GameObject emptySlot = null;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            InventorySlot slot = slots[i].GetComponent<InventorySlot>();

            if (slot != null && slot.HeldItem == null)
            {
                emptySlot = slots[i];
                break;
            }
        }

        if (emptySlot != null)
        {
            GameObject newItem = Instantiate(itemPrefab, inventoryParent.transform);
            newItem.GetComponent<InventoryItem>().Initialize(itemData);
            emptySlot.GetComponent<InventorySlot>().SetHeldItem(newItem);

            WeaponSwap swapper = FindObjectOfType<WeaponSwap>();
            if (swapper != null && itemData.equippedWeaponPrefab != null)
            {
                swapper.AddWeaponToInventory(itemData.equippedWeaponPrefab);
            }

            Destroy(Pickeditem);
        }
    }

    public void UpdateInventoryUI()
    {
        WeaponSwap swapper = FindObjectOfType<WeaponSwap>();
        if (swapper == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            InventorySlot slotScript = slots[i].GetComponent<InventorySlot>();
            if (slotScript == null || slotScript.HeldItem == null) continue;

            InventoryItem uiItem = slotScript.HeldItem.GetComponent<InventoryItem>();
            if (uiItem != null && uiItem.ItemScriptableObject != null)
            {
                ItemSO itemData = uiItem.ItemScriptableObject;

                if (itemData.isWeapon && itemData.equippedWeaponPrefab != null)
                {
                    string targetName = itemData.equippedWeaponPrefab.name;
                    bool stillExistsInBackend = false;

                    foreach (GameObject weapon in swapper.equippedWeapons)
                    {
                        if (weapon != null)
                        {
                            string cleanWName = weapon.name.Replace("(Clone)", "").Trim();
                            if (cleanWName.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                            {
                                stillExistsInBackend = true;
                                break;
                            }
                        }
                    }

                    if (!stillExistsInBackend)
                    {
                        Destroy(slotScript.HeldItem);
                        slotScript.HeldItem = null;
                    }
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] GameObject[] slots = new GameObject[20];
    [SerializeField] GameObject inventoryParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Camera cam;

    GameObject draggedObject;
    GameObject lastItemSlot;

    bool isInventoryOpened;

    void Start()
    {

    }

    void Update()
    {
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
                Cursor.lockState = CursorLockMode.None; // Fixed compiler bitwise assignment error
                isInventoryOpened = true;
            }
        }
    }

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
            newItem.GetComponent<InventoryItem>().Initialize(Pickeditem.GetComponent<ItemPickable>().itemScriptableObject);

            WeaponSwap swapper = FindObjectOfType<WeaponSwap>();
            if (swapper != null)
            {
                swapper.AddWeaponToInventory(Pickeditem.GetComponent<ItemPickable>().itemScriptableObject.equippedWeaponPrefab);
            }

            emptySlot.GetComponent<InventorySlot>().SetHeldItem(newItem);
            Destroy(Pickeditem);
        }
    }

    // =======================================================================
    // ADDED METHOD: Syncs and destroys UI items when thrown or discarded!
    // =======================================================================
    public void UpdateInventoryUI()
    {
        WeaponSwap swapper = FindObjectOfType<WeaponSwap>();
        if (swapper == null) return;

        // Loop through all UI slot boxes
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            InventorySlot slotScript = slots[i].GetComponent<InventorySlot>();
            if (slotScript == null || slotScript.HeldItem == null) continue;

            InventoryItem uiItem = slotScript.HeldItem.GetComponent<InventoryItem>();
            if (uiItem != null && uiItem.ItemScriptableObject != null)
            {
                // Check if this specific item type still exists anywhere inside backend array
                bool stillExistsInBackend = false;
                foreach (GameObject weapon in swapper.equippedWeapons)
                {
                    if (weapon != null && weapon.name.ToLower().Contains(uiItem.ItemScriptableObject.itemName.ToLower()))
                    {
                        stillExistsInBackend = true;
                        break;
                    }
                }

                // If the backend weapon asset list doesn't have it anymore, delete its UI box layout!
                if (!stillExistsInBackend)
                {
                    Destroy(slotScript.HeldItem);
                    slotScript.HeldItem = null;
                }
            }
        }
    }
}
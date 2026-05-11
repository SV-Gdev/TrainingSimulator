using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

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

    // Update is called once per frame
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
                Cursor.lockState &= CursorLockMode.None;
                isInventoryOpened = true;
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();
            if (slot != null && slot.HeldItem !=null)
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

            // Check parent too in case raycast hits the item icon child instead of slot
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

                GameObject newItem = Instantiate(itemData.prefab, spawnPosition, Quaternion.identity);
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
            InventorySlot slot = slots[i].GetComponent<InventorySlot>();

            if(slot.HeldItem == null)
            {
                emptySlot = slots[i];
                break;
            }
        }
        if (emptySlot != null)
        {
            GameObject newItem = Instantiate(itemPrefab,inventoryParent.transform);
            newItem.GetComponent<InventoryItem>().Initialize  (Pickeditem.GetComponent<ItemPickable>().itemScriptableObject);
            
            emptySlot.GetComponent<InventorySlot>().SetHeldItem(newItem);
            Destroy(Pickeditem);
        }
    }
}


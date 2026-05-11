using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    public GameObject HeldItem;
    
    public void SetHeldItem(GameObject item)
    {
        HeldItem = item;
        HeldItem.transform.position = transform.position;
    }
}

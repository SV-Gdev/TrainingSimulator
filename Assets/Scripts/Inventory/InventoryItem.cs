using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public ItemSO ItemScriptableObject;
    [SerializeField] Image iconImage;

    public void Initialize(ItemSO item)
    {
        ItemScriptableObject = item;

        if (ItemScriptableObject != null && ItemScriptableObject.icon != null)
        {
            iconImage.sprite = ItemScriptableObject.icon;
            iconImage.enabled = true;
            iconImage.color = Color.white; // Ensure full visibility for the sprite
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false; // Hides the image so it doesn't show a black box!
        }
    }
}
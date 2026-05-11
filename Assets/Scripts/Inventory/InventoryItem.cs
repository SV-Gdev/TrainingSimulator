using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
   public ItemSO ItemScriptableObject;

    [SerializeField] Image iconImage;
    

    // Update is called once per frame
     public void Initialize(ItemSO item)
    {
        ItemScriptableObject = item;
        iconImage.sprite = ItemScriptableObject.icon;
    }
    
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class weaponAmmo : MonoBehaviour
{
    public int clipsize;
    public int ExtraAmmo;
    public int currentammo;

    public TextMeshProUGUI ammodisplay;

    void Start()
    {
        if (ammodisplay == null)
            ammodisplay = FindObjectOfType<TextMeshProUGUI>();

        currentammo = clipsize;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammodisplay == null)
        {
            Debug.LogError("Ammo UI not assigned on: " + gameObject.name);
            return;
        }

        ammodisplay.text = currentammo + " / " + ExtraAmmo;
    }

    public void UseAmmo(int amount)
    {
        currentammo -= amount;
        if (currentammo < 0) currentammo = 0;

        UpdateAmmoUI();
    }

    public void Reload()
    {
        Debug.Log("Reloading...");

        if (currentammo == clipsize || ExtraAmmo == 0) return;

        int ammotoreload = Mathf.Min(clipsize - currentammo, ExtraAmmo);

        currentammo += ammotoreload;
        ExtraAmmo -= ammotoreload;

        UpdateAmmoUI();
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class weaponAmmo : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int clipSize = 30;
    public int extraAmmo = 90;
    public int currentAmmo;

    [Header("UI")]
    public TextMeshProUGUI ammoDisplay;

    void Start()
    {
        currentAmmo = clipSize;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoDisplay == null)
        {
            Debug.LogError($"[weaponAmmo] ammoDisplay not assigned on {gameObject.name}");
            return;
        }
        ammoDisplay.text = $"{currentAmmo} / {extraAmmo}";
    }

    public void UseAmmo(int amount)
    {
        if (currentAmmo <= 0) return;
        currentAmmo -= amount;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, clipSize);
        UpdateAmmoUI();
    }

    public bool CanShoot() => currentAmmo > 0;

    public void Reload()
    {
        if (currentAmmo == clipSize || extraAmmo == 0)
        {
            Debug.Log("[Ammo] Nothing to reload");
            return;
        }

        int needed = clipSize - currentAmmo;
        int toReload = Mathf.Min(needed, extraAmmo);
        currentAmmo += toReload;
        extraAmmo -= toReload;

        Debug.Log($"[Ammo] Reloaded {toReload} — current={currentAmmo} extra={extraAmmo}");
        UpdateAmmoUI();
    }

    public void AddAmmo(int amount)
    {
        extraAmmo += amount;
        UpdateAmmoUI();
    }
}
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

        if (gameObject.activeInHierarchy)
        {
            UpdateAmmoUI();
        }
        else
        {
            ClearAmmoUI();
        }
    }

    private void OnEnable()
    {
        TryFindUI();
    }

    private void TryFindUI()
    {
        if (ammoDisplay != null) return;

        // =======================================================================
        // STRICTOR SEARCH: Find the Canvas first, then look specifically inside AmmoDisplay
        // This stops it from accidentally grabbing the Score Manager!
        // =======================================================================
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            // Search inside the Canvas for a child object named exactly "AmmoDisplay"
            Transform ammoDisplayTransform = canvasObj.transform.Find("AmmoDisplay");

            if (ammoDisplayTransform != null)
            {
                // Grab the text component off that specific object or its children
                ammoDisplay = ammoDisplayTransform.GetComponent<TextMeshProUGUI>();
                if (ammoDisplay == null)
                {
                    ammoDisplay = ammoDisplayTransform.GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }
        }

        // Emergency backup: ONLY look for objects explicitly named "AmmoDisplay"
        if (ammoDisplay == null)
        {
            TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var txt in allTexts)
            {
                if (txt.gameObject.name == "AmmoDisplay")
                {
                    ammoDisplay = txt;
                    break;
                }
            }
        }

        UpdateAmmoUI();
    }

    public void UpdateAmmoUI()
    {
        if (ammoDisplay == null)
        {
            TryFindUI();
        }

        if (ammoDisplay == null || !gameObject.activeInHierarchy) return;

        // Double check to make sure we didn't accidentally steal the Score text box
        if (ammoDisplay.gameObject.name == "Score")
        {
            ammoDisplay = null;
            TryFindUI();
            return;
        }

        ammoDisplay.text = $"{currentAmmo} / {extraAmmo}";
    }

    public void ClearAmmoUI()
    {
        if (ammoDisplay != null && ammoDisplay.gameObject.name != "Score")
        {
            ammoDisplay.text = "";
        }
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
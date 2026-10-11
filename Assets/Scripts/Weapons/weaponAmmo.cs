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
    [SerializeField] private TextMeshProUGUI ammoDisplay;

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

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
           
            Transform ammoDisplayTransform = canvasObj.transform.Find("AmmoDisplay");
            if (ammoDisplayTransform != null)
            {
                ammoDisplay = ammoDisplayTransform.GetComponent<TextMeshProUGUI>();
                if (ammoDisplay == null)
                {
                    ammoDisplay = ammoDisplayTransform.GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }

           
            if (ammoDisplay == null)
            {
                TextMeshProUGUI[] canvasTexts = canvasObj.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var txt in canvasTexts)
                {
                    if (txt.gameObject.name == "AmmoDisplay")
                    {
                        ammoDisplay = txt;
                        break;
                    }
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
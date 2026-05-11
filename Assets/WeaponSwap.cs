using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour
{
    [SerializeField] GameObject[] weapons;
    int currentWeaponIndex = 0;

    void Start()
    {
        UpdateWeapon();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            currentWeaponIndex++;
            if (currentWeaponIndex >= weapons.Length)
                currentWeaponIndex = 0;
            UpdateWeapon();
        }
        else if (scroll < 0f)
        {
            currentWeaponIndex--;
            if (currentWeaponIndex < 0)
                currentWeaponIndex = weapons.Length - 1;
            UpdateWeapon();
        }
    }

    void UpdateWeapon()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == currentWeaponIndex);
        }
        Debug.Log("Weapon Swapped to: " + weapons[currentWeaponIndex].name);
    }
}
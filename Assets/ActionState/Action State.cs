using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionState : MonoBehaviour
{
    ActionBase currentState;

    public ReloadState Reload = new ReloadState();
   public  DefaultState Default = new DefaultState();
    
  

    [HideInInspector]public WeaponManager currentWeapon;
    [HideInInspector]public weaponAmmo Ammo;
    [HideInInspector] public Animator anim;
    // Start is called before the first frame update
    void Start()
    {

        
        anim = GetComponent<Animator>();

        SwitchState(Default);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(this);
    }
    public void SwitchState(ActionBase state)
    {
        currentState = state;
        currentState.EnterState(this);
    }
    public void WeaponReloaded()
    {
       
        Ammo.Reload();
        SwitchState(Default);
    }

    public void SetWeapon(WeaponManager weapon)
    {
        currentWeapon = weapon;
        Ammo = weapon.Ammo;
    }
    
    }


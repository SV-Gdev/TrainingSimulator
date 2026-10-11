using UnityEngine;
using UnityEngine.Animations.Rigging;

public class NonWeaponHold : MonoBehaviour
{
    private Animator anim;
    private Rig handRig;
    private GameObject ammoPanel;

    private void LateUpdate()
    {
     
        if (anim == null)
        {
            anim = GetComponentInParent<Animator>();
        }

        if (handRig == null)
        {
            handRig = GetComponentInParent<Rig>();
        }

        if (ammoPanel == null)
        {
            GameObject uiObj = GameObject.Find("CurrentWeaponUI");
            if (uiObj != null) ammoPanel = uiObj;
        }

        
        if (anim != null)
        {
            anim.SetBool("IsArmed", false);

            int shootingLayerIndex = anim.GetLayerIndex("Shooting");
            if (shootingLayerIndex != -1)
            {
                anim.SetLayerWeight(shootingLayerIndex, 0f);
            }
        }

       
        if (handRig != null)
        {
            handRig.weight = 0f;
        }

       
        if (ammoPanel != null)
        {
            ammoPanel.SetActive(false);
        }
    }
}
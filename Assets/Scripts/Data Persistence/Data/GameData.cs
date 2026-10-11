using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public bool isLevel1Cleared;
    public bool isLevel2Cleared;
    public Vector3 playerPosition;
    public float activeRunHealth;
    public int totalScore;
    public int remainingEnemiesCount;
    public string equippedWeaponName;

   
    public List<string> inventoryItemNames;

    public GameData()
    {
        isLevel1Cleared = false;
        isLevel2Cleared = false;

       
        playerPosition = new Vector3(6.282702e-06f, 0.4096925f, -31.47f);

        activeRunHealth = 100f;
        totalScore = 0;
        remainingEnemiesCount = 0;
        equippedWeaponName = "";

        inventoryItemNames = new List<string>();
    }
}
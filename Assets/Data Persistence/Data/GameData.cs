using UnityEngine;

[System.Serializable]
public class GameData
{
    public int totalScore;

   
    public float activeRunHealth;

    public Vector3 playerPosition;
    public string equippedWeaponName;
    public int remainingEnemiesCount;

    public GameData()
    {
        this.totalScore = 0;
        this.activeRunHealth = 100f; 
        this.playerPosition = new Vector3(0f, 1f, 0f);
        this.equippedWeaponName = "";
        this.remainingEnemiesCount = 0;
    }
}
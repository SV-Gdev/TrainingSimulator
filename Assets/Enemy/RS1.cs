using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RS1 : MonoBehaviour
{
    public static RS1 Instance;
    void Awake()
    {
        Instance = this;
    }
    public void StartRespawn(GameObject obj, Vector3 pos, Quaternion rot, float delay, float health, float armor, EnemyArmor1 ea)
    {
        StartCoroutine(RespawnCoroutine(obj, pos, rot, delay, health, armor, ea));
    }
    IEnumerator RespawnCoroutine(GameObject obj, Vector3 pos, Quaternion rot, float delay, float health, float armor, EnemyArmor1 ea)
    {
        yield return new WaitForSeconds(delay);
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        ea.ResetHealth(health, armor);
        obj.SetActive(true);
    }
}

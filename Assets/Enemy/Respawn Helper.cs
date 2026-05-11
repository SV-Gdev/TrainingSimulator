using System.Collections;
using UnityEngine;

public class RespawnHelper : MonoBehaviour
{
    public static RespawnHelper Instance;

    void Awake()
    {
        Instance = this;
    }

    public void Respawn(GameObject obj, Vector3 pos, Quaternion rot, float delay, float health, EnemyHealth eh)
    {
        StartCoroutine(RespawnCoroutine(obj, pos, rot, delay, health, eh));
    }

    IEnumerator RespawnCoroutine(GameObject obj, Vector3 pos, Quaternion rot, float delay, float health, EnemyHealth eh)
    {
        yield return new WaitForSeconds(delay);

        obj.transform.position = pos;
        obj.transform.rotation = rot;
        eh.ResetHealth(health);
       

        obj.SetActive(true);
    }

}
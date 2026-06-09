using System.Collections;
using UnityEngine;

public class PlayerPersistenceController : MonoBehaviour, IDataPersistence
{
    private float currentHealth = 100f;

    
    public void LoadData(GameData data)
    {
        StartCoroutine(LoadPositionRoutine(data.playerPosition));
    }

    private IEnumerator LoadPositionRoutine(Vector3 savedPosition)
    {
        yield return new WaitForEndOfFrame();

        
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        CharacterController cc = GetComponent<CharacterController>();

        if (agent != null) agent.enabled = false;
        if (cc != null) cc.enabled = false;

       
        this.transform.position = savedPosition;
        Debug.Log($"<color=cyan>[Player Load]</color> Restored position : {savedPosition}");

        if (agent != null) agent.enabled = true;
        if (cc != null) cc.enabled = true;
    }

   
    public void SaveData(ref GameData data)
    {
       
        data.playerPosition = transform.position;

        
    }
}
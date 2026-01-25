using UnityEngine;
using System.Collections;

public class PlayerMotherShip : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 PointA;
    public Vector3 PointB;
    public float MoveSpeed = 3f;
    
    [Header("Player Spawn")]
    public GameObject PlayerPrefab;
    public float RespawnDelay = 3f;
    
    private bool isRespawning = false;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        MoveBetweenPoints();
        CheckPlayerStatus();
    }
    
    void MoveBetweenPoints()
    {
        if (PointA == null || PointB == null) return;
        
        float distance = Vector3.Distance(PointA, PointB);
        float progress = (Mathf.PingPong(Time.time * MoveSpeed, distance) / distance);
        
        transform.position = Vector3.Lerp(PointA, PointB, progress);
    }
    
        
    void CheckPlayerStatus()
    {
        if (isRespawning) return;
        
        CellController player = FindFirstObjectByType<CellController>();
        if (player == null)
        {
            StartCoroutine(RespawnPlayer());
        }
    }
    
    IEnumerator RespawnPlayer()
    {
        isRespawning = true;
        yield return new WaitForSeconds(RespawnDelay);
        
        if (PlayerPrefab != null)
        {
            GameObject newPlayer = Instantiate(PlayerPrefab, transform.position + Vector3.forward * 2, Quaternion.identity);
        }
        
        isRespawning = false;
    }
}

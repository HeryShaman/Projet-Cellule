using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class GameManager : MonoBehaviour
{
    public List<Vector3> CellSpawns;
    public int CellNumbersPerSpawn = 1;

    public GameObject CellPrefab;
    public GameObject PlayerPrefab;
    public GameObject PlayerMotherShipPrefab;

    private void Start()
    {
        SpawnCells();
        AudioManager.Instance?.PlayGameplayMusic();
    }

    private void Update()
    {
        HandleDebugControls();
    }


    public void SpawnCells()
    {
        if (CellPrefab == null)
        {
            Debug.LogWarning("SpawnManager : CellPrefab non assigné !");
            return;
        }

        foreach (var point in CellSpawns)
        {
            for (int i = 0; i < CellNumbersPerSpawn; i++)
            {
                Vector3 spawnPos = point;

                // Instanciation de la cellule
                CellEntity cell = Instantiate(CellPrefab, spawnPos, Quaternion.identity).GetComponent<CellEntity>();
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Spawn des cellules
        if (CellSpawns != null)
        {
            Gizmos.color = Color.green; // Couleur du draw = green
            foreach (Vector3 pos in CellSpawns)
            {
                Gizmos.DrawWireSphere(pos, 3f);
            }
        }
    }
    
    private void HandleDebugControls()
    {
        if (Input.GetKeyDown(KeyCode.O)) foreach (CellEntity cell in FindObjectsByType<CellEntity>(FindObjectsSortMode.None)) cell.CurrentState = CellEntity.States.Healthy;
        if (Input.GetKeyDown(KeyCode.I)) foreach (CellEntity cell in FindObjectsByType<CellEntity>(FindObjectsSortMode.None)) cell.CurrentState = CellEntity.States.Infected;
        if (Input.GetKeyDown(KeyCode.M)) AudioListener.volume = AudioListener.volume == 0f ? 1f : 0f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class GameManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int MaxHunter = 20;
    public int MaxMessenger = 10;
    public int MaxWarden = 15;

    public Vector3 PlayerSpawn;

    public List<Vector3> CellSpawns;
    public List<Vector3> WardenNodes;

    public HashSet<ReproducerEntity> InfectedCells = new HashSet<ReproducerEntity>();
    public HashSet<ReproducerEntity> SafeCells = new HashSet<ReproducerEntity>();
    public HashSet<Vector3> OccupiedWardenNodes = new HashSet<Vector3>();

    // Listes des entités vivantes
    public List<HunterEntity> ActiveHunters = new List<HunterEntity>();
    public List<MessengerEntity> ActiveMessengers = new List<MessengerEntity>();
    public List<WardenEntity> ActiveWardens = new List<WardenEntity>();

    public GameObject CellPrefab;
    public GameObject PlayerPrefab;

    [Header("Propagation Settings")]
    public int CellNumbersPerSpawn;
    public float TimeBeforeInfect = 10f;
    private float InfectionTimer = 0f;

    private void Start()
    {
        if (Object.FindAnyObjectByType<PlayerController>() == null)
        {
            SpawnPlayer();
        }
        SpawnCells();

        // Démarre la première infection après 4 secondes
        InvokeRepeating(nameof(InfectCells), TimeBeforeInfect, TimeBeforeInfect);
    }

    private void Update()
    {
        // Vérifie en temps réel l'état des cellules et met à jour les listes
        UpdateCellLists();

        // Gère le timer d'infection
        InfectionTimer += Time.deltaTime;
    }

    private void UpdateCellLists()
    {
        // Parcourt toutes les cellules pour vérifier leur état
        ReproducerEntity[] allCells = FindObjectsByType<ReproducerEntity>(FindObjectsSortMode.None);

        // Vide les listes actuelles
        SafeCells.Clear();
        InfectedCells.Clear();

        // Reclassifie les cellules selon leur état actuel
        foreach (ReproducerEntity cell in allCells)
        {
            if (cell.CurrentState == ReproducerEntity.States.Healthy)
            {
                SafeCells.Add(cell);
            }
            else if (cell.CurrentState == ReproducerEntity.States.Infected)
            {
                InfectedCells.Add(cell);
            }
        }
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
                ReproducerEntity cell = Instantiate(CellPrefab, spawnPos, Quaternion.identity).GetComponent<ReproducerEntity>();
                SafeCells.Add(cell);
            }
        }
    }

    public void InfectCells()
    {
        // Si aucune cellule infectée, infecte aléatoirement une cellule saine
        if (InfectedCells.Count == 0 && SafeCells.Count > 0)
        {
            // Prend une cellule saine au hasard
            ReproducerEntity[] safeCellsArray = new ReproducerEntity[SafeCells.Count];
            SafeCells.CopyTo(safeCellsArray);

            int randomIndex = Random.Range(0, safeCellsArray.Length);
            ReproducerEntity cellToInfect = safeCellsArray[randomIndex];

            if (cellToInfect != null)
            {
                cellToInfect.CurrentState = ReproducerEntity.States.Infected;
                Debug.Log($"Infection de la cellule à {cellToInfect.transform.position}");
            }
        }
    }

    public GameObject SpawnPlayer()
    {
        if (PlayerPrefab == null)
        {
            Debug.LogWarning("SpawnManager : PlayerPrefab non assigné !");
            return null;
        }

        // Instancie le joueur
        GameObject player = Instantiate(PlayerPrefab, PlayerSpawn, Quaternion.identity);
        player.name = "Player";

        // Assigne automatiquement la caméra au joueur
        CameraController cam = Object.FindAnyObjectByType<CameraController>();
        if (cam != null)
        {
            cam.Target = player.transform;
        }

        // Assigne aussi la référence de la caméra au PlayerController si besoin
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null && cam != null)
        {
            pc.Cam = cam;
        }

        return player;
    }

    private void OnDrawGizmos()
    {
        // Spawn du joueur
        Gizmos.color = Color.blue; // Couleur du draw = blue
        Gizmos.DrawWireSphere(PlayerSpawn, 1f);

        // Spawn des cellules
        if (CellSpawns != null)
        {
            Gizmos.color = Color.green; // Couleur du draw = green
            foreach (Vector3 pos in CellSpawns)
            {
                Gizmos.DrawWireSphere(pos, 1f);
            }
        }

        // Nodes des Gardiens
        if (WardenNodes != null)
        {
            Gizmos.color = Color.red; // Couleur du draw = red
            foreach (Vector3 pos in WardenNodes)
            {
                Gizmos.DrawWireSphere(pos, 8f); // sphère de 0.3 unité
            }
        }
    }
}

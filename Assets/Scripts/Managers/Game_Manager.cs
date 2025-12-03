using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<Transform> spawnPoints; // points d'apparition éditables
    public GameObject reproducerPrefab; // prefab de cellule reproductrice
    public int spawnCountPerPoint = 1; // nombre de cellules par point

    [Header("Infection Settings")]
    public float maxNoInfectionTime = 5f; // si aucun infecté > X secondes, en infecte un
    private float timeSinceLastInfected = 0f;

    private List<EnemyReproducer> allReproducers = new List<EnemyReproducer>();

    void Start()
    {
        SpawnAllCells();
    }

    void Update()
    {
        // Vérifie si au moins une cellule est infectée
        bool anyInfected = false;
        foreach (var cell in allReproducers)
        {
            if (cell != null && cell.currentState == EnemyState.Infected)
            {
                anyInfected = true;
                break;
            }
        }

        if (anyInfected)
        {
            timeSinceLastInfected = 0f; // reset timer
        }
        else
        {
            timeSinceLastInfected += Time.deltaTime;

            // Si > maxNoInfectionTime, infecte une cellule neutre aléatoire
            if (timeSinceLastInfected >= maxNoInfectionTime)
            {
                InfectRandomCell();
                timeSinceLastInfected = 0f;
            }
        }
    }

    void SpawnAllCells()
    {
        allReproducers.Clear();

        foreach (var point in spawnPoints)
        {
            for (int i = 0; i < spawnCountPerPoint; i++)
            {
                Vector3 spawnPos = point.position + Random.insideUnitSphere * 0.5f;
                spawnPos.y = point.position.y;

                GameObject cellObj = Instantiate(reproducerPrefab, spawnPos, Quaternion.identity);
                EnemyReproducer cell = cellObj.GetComponent<EnemyReproducer>();
                if (cell != null)
                {
                    allReproducers.Add(cell);

                    // On peut définir ici si la cellule démarre infectée ou neutre
                    cell.SetInfected(false); // toutes neutres au départ
                }
            }
        }
    }

    void InfectRandomCell()
    {
        // Récupère toutes les cellules neutres
        List<EnemyReproducer> neutres = allReproducers.FindAll(c => c != null && c.currentState == EnemyState.Neutral);

        if (neutres.Count > 0)
        {
            int index = Random.Range(0, neutres.Count);
            neutres[index].SetInfected(true);
            Debug.Log("Infecté automatiquement une cellule neutre !");
        }
    }
}

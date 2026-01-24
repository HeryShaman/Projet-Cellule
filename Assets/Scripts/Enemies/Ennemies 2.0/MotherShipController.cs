using UnityEngine;
using UnityEngine.AI;

public class EnemyMotherShip : MonoBehaviour
{
    [Header("Enemy Mother Ship Settings")]
    public Vector3 PointA;
    public Vector3 PointB;
    public float MoveSpeed = 3f;
    
    [Header("Messenger Spawning")]
    public GameObject MessengerPrefab;
    public float MessengerSpawnInterval = 5f;
    public int MessengersPerWave = 3;
    public float MessengerSpawnRadius = 3f;
    public int MaxMessengers = 10;
    
    [Header("Game State Detection")]
    public float GameStateCheckInterval = 2f;
    
    private float messengerSpawnTimer;
    private float gameStateCheckTimer;
    
    public enum GameState
    {
        Dominant,
        Advantage,
        Neutral,
        Disadvantage,
        Dominated
    }
    
    public GameState CurrentGameState = GameState.Neutral;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        HandleMovement();
        HandleMessengerSpawning();
        UpdateGameState();
    }
    
    void HandleMovement()
    {
        float distance = Vector3.Distance(PointA, PointB);
        float progress = (Mathf.PingPong(Time.time * MoveSpeed, distance) / distance);
        
        transform.position = Vector3.Lerp(PointA, PointB, progress);
    }
    
    
    void HandleMessengerSpawning()
    {
        messengerSpawnTimer += Time.deltaTime;
        
        if (messengerSpawnTimer >= MessengerSpawnInterval)
        {
            messengerSpawnTimer = 0f;
            
            // Compter les messagers existants
            MessengerEntity[] existingMessengers = FindObjectsByType<MessengerEntity>(FindObjectsSortMode.None);
            
            if (existingMessengers.Length < MaxMessengers)
            {
                SpawnMessengerWave();
            }
        }
    }
    
    void SpawnMessengerWave()
    {
        if (MessengerPrefab == null) return;
        
        MessengerEntity[] existingMessengers = FindObjectsByType<MessengerEntity>(FindObjectsSortMode.None);
        int messengersToSpawn = Mathf.Min(MessengersPerWave, MaxMessengers - existingMessengers.Length);
        
        for (int i = 0; i < messengersToSpawn; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * MessengerSpawnRadius;
            spawnPosition.y = transform.position.y;
            
            Instantiate(MessengerPrefab, spawnPosition, Quaternion.identity);
        }
        
        Debug.Log($"Vague de {messengersToSpawn} messagers spawnée par le vaisseau mère");
    }
    
    void UpdateGameState()
    {
        gameStateCheckTimer += Time.deltaTime;
        
        if (gameStateCheckTimer >= GameStateCheckInterval)
        {
            gameStateCheckTimer = 0f;
            
            // Compter les cellules de chaque type
            ReproducerEntity[] cells = FindObjectsByType<ReproducerEntity>(FindObjectsSortMode.None);
            
            int healthyCells = 0;
            int infectedCells = 0;
            int neutralCells = 0;
            
            foreach (var cell in cells)
            {
                if (cell.CurrentState == ReproducerEntity.States.Healthy)
                    healthyCells++;
                else if (cell.CurrentState == ReproducerEntity.States.Infected)
                    infectedCells++;
                else if (cell.CurrentState == ReproducerEntity.States.Neutral)
                    neutralCells++;
            }
            
            int totalCells = cells.Length;
            
            // Déterminer l'état du jeu selon les 5 états demandés
            if (infectedCells == totalCells && totalCells > 0)
                CurrentGameState = GameState.Dominant;
            else if (infectedCells > totalCells * 0.6f)
                CurrentGameState = GameState.Advantage;
            else if (infectedCells == healthyCells || (infectedCells == 0 && healthyCells == 0))
                CurrentGameState = GameState.Neutral;
            else if (healthyCells > infectedCells)
                CurrentGameState = GameState.Disadvantage;
            else
                CurrentGameState = GameState.Dominated;
            
            // Ajuster le spawn de messagers selon l'état
            switch (CurrentGameState)
            {
                case GameState.Dominant:
                    MessengerSpawnInterval = 999f; // Plus de spawn
                    MessengersPerWave = 0;
                    break;
                case GameState.Advantage:
                    MessengerSpawnInterval = 10f; // Très lent
                    MessengersPerWave = 2;
                    break;
                case GameState.Neutral:
                    MessengerSpawnInterval = 5f; // Normal
                    MessengersPerWave = 3;
                    break;
                case GameState.Disadvantage:
                    MessengerSpawnInterval = 4f; // Régulier
                    MessengersPerWave = 3;
                    break;
                case GameState.Dominated:
                    MessengerSpawnInterval = 2f; // Très régulier
                    MessengersPerWave = 3;
                    break;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(PointA, PointB);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(PointA, 0.5f);
        Gizmos.DrawSphere(PointB, 0.5f);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, MessengerSpawnRadius);
    }
}

using UnityEngine;

public enum EnemyState
{
    Neutral,
    Infected
}

public class EnemyReproducer : EnemyBase
{
    [Header("Reproduction Settings")]
    public GameObject transporterPrefab;
    public GameObject hunterPrefab;

    public float RegenRate = 1f;
    public int MaxSpawnCount = 10;
    public int MinSpawnPerCycle = 2;
    public int MaxSpawnPerCycle = 2;
    public float HealthCostPerSpawn = 25f;
    public float SpawnInterval = 3f;

    [Header("Type-specific Limits")]
    public int MaxTransporterSpawn = 2;
    public int MaxHunterSpawn = 4;

    private int currentSpawned = 0;
    private int currentTransporterSpawned = 0;
    private int currentHunterSpawned = 0;
    private float nextSpawnTime = 0f;

    [Header("Infection Settings")]
    public EnemyState currentState = EnemyState.Neutral;
    public Material neutralMaterial;
    public Material infectedMaterial;
    private Renderer rend;

    protected override void Start()
    {
        base.CurrentHealth = 1;
        base.Start();
        rend = GetComponentInChildren<Renderer>();
        UpdateVisualState();
    }

    protected override void Update()
    {
        base.Update();
        Regenerate();

        if (currentState == EnemyState.Infected &&
            Time.time >= nextSpawnTime &&
            CurrentHealth >= HealthCostPerSpawn &&
            currentSpawned < MaxSpawnCount)
        {
            TrySpawnEnemies();
            nextSpawnTime = Time.time + SpawnInterval;
        }
    }

    private void Regenerate()
    {
        if (CurrentHealth < MaxHealth)
            CurrentHealth += RegenRate * Time.deltaTime;
    }

    private void TrySpawnEnemies()
    {
        int enemiesToSpawn = Random.Range(MinSpawnPerCycle, MaxSpawnPerCycle + 1);
        enemiesToSpawn = Mathf.Min(enemiesToSpawn, MaxSpawnCount - currentSpawned);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (CurrentHealth < HealthCostPerSpawn)
                break;

            GameObject prefabToSpawn = Random.value > 0.5f ? transporterPrefab : hunterPrefab;

            // Vérification des limites par type
            if (prefabToSpawn == transporterPrefab && currentTransporterSpawned >= MaxTransporterSpawn)
                continue;
            if (prefabToSpawn == hunterPrefab && currentHunterSpawned >= MaxHunterSpawn)
                continue;

            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 1f;
            spawnPos.y = transform.position.y;

            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            CurrentHealth -= HealthCostPerSpawn;
            currentSpawned++;

            if (prefabToSpawn == transporterPrefab)
                currentTransporterSpawned++;
            else
                currentHunterSpawned++;
        }
    }

    public void SetInfected(bool infected)
    {
        currentState = infected ? EnemyState.Infected : EnemyState.Neutral;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (rend == null) return;

        if (currentState == EnemyState.Infected && infectedMaterial != null)
            rend.material = infectedMaterial;
        else if (neutralMaterial != null)
            rend.material = neutralMaterial;
    }

    public override void TakeDamage(float amount)
    {
        if (currentState == EnemyState.Neutral)
            return;

        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            HealCell();
            return;
        }

        UpdateTargetScale();
    }

    private void HealCell()
    {
        SetInfected(false);
        CurrentHealth = MaxHealth;
        currentSpawned = 0;
        currentTransporterSpawned = 0;
        currentHunterSpawned = 0;
        Debug.Log($"{name} a été soignée par le joueur !");
    }

    protected override void Die()
    {
        if (currentState == EnemyState.Infected)
        {
            HealCell();
        }
        else
        {
            base.Die();
        }
    }
}

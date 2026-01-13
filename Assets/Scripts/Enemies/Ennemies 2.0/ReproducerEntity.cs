using UnityEngine;

public class ReproducerEntity : Entity
{
    [Header("Spawner Stats")]
    public float SpawnHealthCost;
    public float SpawnInterval;
    public int SpawnHunterLimit = 5;
    public int SpawnMessengerLimit = 3;

    [SerializeField] private float HealthySpawnCooldown = 5.0f;
    [SerializeField] private float InfectedSpawnCooldown = 3.0f;
    [SerializeField] private float SpawnTimer;

    private int SpawnedHunters = 0;
    private int SpawnedMessengers = 0;
    private int CurrentHunters = 0;
    private int CurrentMessengers = 0;

    [Header("References")]
    public GameObject HunterEntity;
    public GameObject MessengerEntity;
    public GameObject WardenEntity;

    private GameManager Spawner;


    [Header("Entity Graphics")]
    public Material HealthyMaterial;
    public Material InfectedMaterial;
    private Renderer Render;

    public enum States
    {
        Healthy,
        Infected
    }

    [Header("Entity States")]
    public States CurrentState = States.Healthy;

    void Start()
    {
        base.CurrentHealth = base.MaxHealth;
        Render = GetComponentInChildren<Renderer>();
        UpdateMaterial();
        Spawner = FindAnyObjectByType<GameManager>();
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            if (CurrentState == States.Infected)
            {
                float DrainDamage = player.CurrentStamina;
                TakeDamage(DrainDamage * Time.deltaTime);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        base.Regenerate();

        SpawnTimer += Time.deltaTime;

        // Mettre à jour les compteurs actuels depuis le GameManager
        if (Spawner != null)
        {
            CurrentHunters = Spawner.ActiveHunters.Count;
            CurrentMessengers = Spawner.ActiveMessengers.Count;
        }

        // Logique d'Apparition Saine
        if (CurrentState == States.Healthy)
        {
            HealthySpawnLogic();
        }

        // Logique d'Apparition Infecté
        else if (CurrentState == States.Infected)
        {
            InfectedSpawnLogic();
        }

        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (Render == null)
            return;

        switch (CurrentState)
        {
            case States.Healthy:
                Render.material = HealthyMaterial;
                break;
            case States.Infected:
                Render.material = InfectedMaterial;
                break;
        }
    }

    void InfectedSpawnLogic()
    {
        if (SpawnTimer < InfectedSpawnCooldown)
            return;

        if (CurrentHealth < SpawnHealthCost)
            return;

        CurrentHealth -= SpawnHealthCost;

        GameObject entityToSpawn;
        int limit;

        switch (Random.Range(0, 2))
        {
            case 0:
                entityToSpawn = HunterEntity;
                limit = SpawnHunterLimit;
                break;
            case 1:
                entityToSpawn = MessengerEntity;
                limit = SpawnMessengerLimit;
                break;
            default:
                return;
        }

        if ((entityToSpawn == HunterEntity && CurrentHunters < limit) ||
            (entityToSpawn == MessengerEntity && CurrentMessengers < limit))
        {
            Instantiate(entityToSpawn, transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), Quaternion.identity);

            if (entityToSpawn == HunterEntity)
                SpawnedHunters++;
            else
                SpawnedMessengers++;
        }

        SpawnTimer = 0f;
    }

    void HealthySpawnLogic()
    {
        if (SpawnTimer < HealthySpawnCooldown)
            return;

        if (CurrentHealth < SpawnHealthCost)
            return;

        if (Spawner.OccupiedWardenNodes.Count >= Spawner.WardenNodes.Count)
            return;

        if(Spawner.MaxWarden <= Spawner.ActiveWardens.Count)
            return;

        CurrentHealth -= SpawnHealthCost;

        Instantiate(WardenEntity, transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), Quaternion.identity);

        SpawnTimer = 0f;
    }

    public override void TakeDamage(float amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            if (CurrentState == States.Infected)
                CurrentState = States.Healthy;
            else if (CurrentState == States.Healthy)
                CurrentState = States.Infected;
        }
    }

    protected override void Die()
    {
        if (CurrentState == States.Infected)
            CurrentState = States.Healthy;
        else if (CurrentState == States.Healthy)
            CurrentState = States.Infected;
    }
}

using UnityEngine;

public class CellEntity : Entity
{
    [Header("Spawner Stats")]
    public float SpawnHealthCost = 5.0f;
    public float HealthySpawnCooldown = 5.0f;
    public float InfectedSpawnCooldown = 3.0f;
    public int MaxHunters = 3;
    public int MaxWardens = 3;
    
    private int spawnedHunters = 0;
    private int spawnedWardens = 0;
    
    [Header("Entity Graphics")]
    public Material HealthyMaterial;
    public Material InfectedMaterial;
    public Material NeutralMaterial;
    
    [Header("References")]
    public GameObject HunterEntity;
    public GameObject WardenEntity;

    private Renderer Render;
    private float SpawnTimer;

    public enum States
    {
        Neutral,
        Healthy,
        Infected
    }

    public States CurrentState = States.Neutral;

    void Start()
    {
        base.CurrentHealth = base.MaxHealth;
        Render = GetComponentInChildren<Renderer>();
        UpdateMaterial();
    }

    private void OnTriggerStay(Collider other)
    {
        CellController player = other.GetComponent<CellController>();
        MessengerEntity messenger = other.GetComponent<MessengerEntity>();

        // Interaction avec le joueur : si la cellule est contaminée, le joueur peut la soigner
        if (player != null && player.IsDashing)
        {
            if (CurrentState == States.Infected)
            {
                CurrentState = States.Healthy;
                UpdateMaterial();
            }
            else if (CurrentState == States.Neutral)
            {
                CurrentState = States.Healthy;
                UpdateMaterial();
            }
        }

        // Interaction avec le messager : contamination
        if (messenger != null && (CurrentState == States.Neutral || CurrentState == States.Healthy))
        {
            CurrentState = States.Infected;
            UpdateMaterial();
            Debug.Log("Cellule contaminée par un messager");
        }
    }

    protected override void Update()
    {
        base.Update();
        base.Regenerate();

        SpawnTimer += Time.deltaTime;

        // Logique d'Apparition Neutre (ne spawn rien)
        if (CurrentState == States.Neutral)
        {
            // Les cellules neutres ne spawn rien
        }

        // Logique d'Apparition Saine
        else if (CurrentState == States.Healthy)
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
            case States.Neutral:
                if (NeutralMaterial != null)
                    Render.material = NeutralMaterial;
                break;
            case States.Healthy:
                if (HealthyMaterial != null)
                    Render.material = HealthyMaterial;
                break;
            case States.Infected:
                if (InfectedMaterial != null)
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

        // Vérifier le nombre d'entités locales
        if (spawnedHunters >= MaxHunters)
            return;

        CurrentHealth -= SpawnHealthCost;

        // Spawn uniquement des Hunters
        if (HunterEntity != null)
        {
            Instantiate(HunterEntity, transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), Quaternion.identity);
            spawnedHunters++;
            Debug.Log("Spawn Hunter depuis cellule infectée");
        }

        SpawnTimer = 0f;
    }

    void HealthySpawnLogic()
    {
        if (SpawnTimer < HealthySpawnCooldown)
            return;

        if (CurrentHealth < SpawnHealthCost)
            return;

        // Vérifier le nombre d'entités locales
        if (spawnedWardens >= MaxWardens)
            return;

        CurrentHealth -= SpawnHealthCost;

        if (WardenEntity != null)
        {
            Instantiate(WardenEntity, transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), Quaternion.identity);
            spawnedWardens++;
            Debug.Log("Spawn Warden depuis cellule saine");
        }

        SpawnTimer = 0f;
    }

    public override void TakeDamage(float amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            if (CurrentState == States.Infected)
            {
                CurrentState = States.Healthy;
                // Réinitialiser les compteurs quand on passe de infecté à sain
                spawnedHunters = 0;
                spawnedWardens = 0;
            }
            else if (CurrentState == States.Healthy)
            {
                CurrentState = States.Infected;
                // Réinitialiser les compteurs quand on passe de sain à infecté
                spawnedHunters = 0;
                spawnedWardens = 0;
            }
            // Les cellules neutres ne changent pas d'état quand elles meurent
        }
    }

    protected override void Die()
    {
        if (CurrentState == States.Infected)
        {
            CurrentState = States.Healthy;
            // Réinitialiser les compteurs quand on passe de infecté à sain
            spawnedHunters = 0;
            spawnedWardens = 0;
        }
        else if (CurrentState == States.Healthy)
        {
            CurrentState = States.Infected;
            // Réinitialiser les compteurs quand on passe de sain à infecté
            spawnedHunters = 0;
            spawnedWardens = 0;
        }
        // Les cellules neutres ne changent pas d'état quand elles meurent
    }
}

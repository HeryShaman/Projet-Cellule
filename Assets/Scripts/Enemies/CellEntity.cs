using UnityEngine;
using System.Collections.Generic;



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

    private List<GameObject> spawnedHunterObjects = new List<GameObject>();

    private List<GameObject> spawnedWardenObjects = new List<GameObject>();

    

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



        if (player != null && player.IsDashing)

        {

            if (CurrentState == States.Infected)

            {

                CurrentState = States.Healthy;

                UpdateMaterial();



                // 🔊 Son de purification

                AudioManager.Instance?.PlayCellPurify();

            }

            else if (CurrentState == States.Neutral)

            {

                CurrentState = States.Healthy;

                UpdateMaterial();



                // 🔊 Son de purification

                AudioManager.Instance?.PlayCellPurify();

            }

        }



        // Interaction avec le messager : contamination

        if (messenger != null && (CurrentState == States.Neutral || CurrentState == States.Healthy))

        {

            CurrentState = States.Infected;

            UpdateMaterial();

            Debug.Log("Cellule contaminée par un messager");



            // 🔊 Son d’infection

            AudioManager.Instance?.PlayCellInfect();

        }

    }



    protected override void Update()

    {

        base.Update();

        base.Regenerate();



        // Nettoyer les listes des entités détruites

        spawnedHunterObjects.RemoveAll(obj => obj == null);

        spawnedWardenObjects.RemoveAll(obj => obj == null);

        spawnedHunters = spawnedHunterObjects.Count;

        spawnedWardens = spawnedWardenObjects.Count;



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



        // Vérifier le nombre de hunters spawnés par cette cellule

        if (spawnedHunters >= MaxHunters)

            return;



        CurrentHealth -= SpawnHealthCost;



        // Spawn uniquement des Hunters

        if (HunterEntity != null)

        {

            GameObject newHunter = Instantiate(HunterEntity, transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), Quaternion.identity);

            spawnedHunterObjects.Add(newHunter);

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



        // Vérifier le nombre de wardens spawnés par cette cellule

        if (spawnedWardens >= MaxWardens)

            return;



        CurrentHealth -= SpawnHealthCost;



        if (WardenEntity != null)

        {

            GameObject newWarden = Instantiate(WardenEntity, transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), Quaternion.identity);

            spawnedWardenObjects.Add(newWarden);

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

            }

            else if (CurrentState == States.Healthy)

            {

                CurrentState = States.Infected;

            }

            // Les cellules neutres ne changent pas d'état quand elles meurent

        }

    }



    protected override void Die()

    {

        if (CurrentState == States.Infected)

        {

            CurrentState = States.Healthy;

        }

        else if (CurrentState == States.Healthy)

        {

            CurrentState = States.Infected;

        }

        // Les cellules neutres ne changent pas d'état quand elles meurent

    }

}


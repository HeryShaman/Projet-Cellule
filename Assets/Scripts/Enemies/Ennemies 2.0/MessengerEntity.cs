using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MessengerEntity : Entity
{
    [Header("Messenger Settings")]
    public float DrainDamage = 0.1f;
    public float RegenerateRadius = 3f;
    public int MaxHunterHost = 5;

    [Header("References")]
    private Vector3 Target;
    private NavMeshAgent Agent;
    private GameManager Spawner;

    public enum States
    {
        Regenerate,
        GoToInfect,
        GoToRetreat
    }

    public States CurrentState = States.GoToInfect;
    public int CurrentHunters = 0;

    private void Start()
    {
        base.CurrentHealth = base.MaxHealth;
        Agent = GetComponent<NavMeshAgent>();
        Spawner = FindAnyObjectByType<GameManager>();
    }

    protected override void Update()
    {
        base.Update();

        switch (CurrentState)
        {
            case States.Regenerate:
                HandleRegenerate();
                break;

            case States.GoToInfect:
                GoToInfect();
                break;

            case States.GoToRetreat:
                GoToRetreat();
                break;
        }

    }


    void HandleRegenerate()
    {
        base.Regenerate();

        // Vérifie si à côté d'une cellule infectée pour se régénérer
        foreach (var cell in Spawner.InfectedCells)
        {
            if (cell.CurrentState == ReproducerEntity.States.Infected)
            {
                float distance = Vector3.Distance(transform.position, cell.transform.position);
                if (distance <= RegenerateRadius)
                {
                    // Régénération accélérée près des cellules infectées
                    CurrentHealth += RegenRateHealth * 2 * Time.deltaTime;
                    break;
                }
            }
        }

        // Retour à l'infection si vie au max
        if (CurrentHealth >= MaxHealth)
        {
            CurrentState = States.GoToInfect;
        }
    }

    void GoToInfect()
    {
        ReproducerEntity targetCell = FindNearestSafeCell();
        
        if (targetCell != null)
        {
            Target = targetCell.transform.position;
            Agent.SetDestination(Target);
        }
        else
        {
            // Plus de cellules saines, se meurt
            TakeDamage(0.1f);
        }
    }

    void GoToRetreat()
    {
        ReproducerEntity targetCell = FindNearestInfectedCell();
        
        if (targetCell != null)
        {
            Target = targetCell.transform.position;
            Agent.SetDestination(Target);
        }
    }

    ReproducerEntity FindNearestSafeCell()
    {
        ReproducerEntity nearest = null;
        float minDist = float.MaxValue;

        foreach (var cell in Spawner.SafeCells)
        {
            if (cell.CurrentState == ReproducerEntity.States.Healthy)
            {
                float dist = Vector3.Distance(transform.position, cell.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = cell;
                }
            }
        }
        return nearest;
    }

    ReproducerEntity FindNearestInfectedCell()
    {
        ReproducerEntity nearest = null;
        float minDist = float.MaxValue;

        foreach (var cell in Spawner.InfectedCells)
        {
            if (cell.CurrentState == ReproducerEntity.States.Infected)
            {
                float dist = Vector3.Distance(transform.position, cell.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = cell;
                }
            }
        }
        return nearest;
    }

    public void AddHunter()
    {
        if (CurrentHunters < MaxHunterHost)
        {
            CurrentHunters++;
            Debug.Log("Hunter ajouté: " + CurrentHunters + "/" + MaxHunterHost);
        }
    }

    public void RemoveHunter()
    {
        if (CurrentHunters > 0)
        {
            CurrentHunters--;
            Debug.Log("Hunter retiré: " + CurrentHunters + "/" + MaxHunterHost);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        WardenEntity warden = other.GetComponent<WardenEntity>();
        ReproducerEntity reproducer = other.GetComponent<ReproducerEntity>();

        // Interaction avec le joueur : dégâts mutuels
        if (player != null)
        {
            player.ReceiveDamage(DrainDamage);
            TakeDamage(DrainDamage);
        }

        // Interaction avec le warden : le messager perd de la vie
        if (warden != null)
        {
            TakeDamage(DrainDamage);
        }

        // Interaction avec la cellule : infection
        if (reproducer != null && reproducer.CurrentState == ReproducerEntity.States.Healthy)
        {
            reproducer.TakeDamage(DrainDamage);
            TakeDamage(DrainDamage);
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class WardenEntity : Entity
{
    public float MaxSpeed = 5f;
    private NavMeshAgent Agent;
    private CellEntity OriginCell;
    private Vector3 GuardPosition;

    [Header("Warden Settings")]
    public float DrainDamage = 0.1f;
    public float MaxDistanceFromCell = 15f;
    public float GuardRadius = 10f;
    public float ChaseSpeedMultiplier = 2f;
    public float PatrolRadius = 8f;

    public enum States
    {
        Guarding,
        Surveillance
    }

    public States CurrentState = States.Guarding;
    private Transform currentTarget;

    private void Start()
    {
        base.CurrentHealth = base.MaxHealth;
        Agent = GetComponent<NavMeshAgent>();

        // Trouver la cellule saine la plus proche comme origine
        FindOriginCell();

        if (OriginCell != null)
        {
            GuardPosition = OriginCell.transform.position;
            CurrentState = States.Guarding;
        }
    }

    protected override void Update()
    {
        base.Update();

        // Le Warden ne peut pas mourir
        // if (CurrentHealth <= 0)
        // {
        //     Die();
        //     return;
        // }

        switch (CurrentState)
        {
            case States.Guarding:
                GuardArea();
                break;
            case States.Surveillance:
                SurveillanceArea();
                break;
        }
    }

    void FindOriginCell()
    {
        CellEntity[] cells = FindObjectsByType<CellEntity>(FindObjectsSortMode.None);
        float minDist = float.MaxValue;

        foreach (var cell in cells)
        {
            if (cell.CurrentState == CellEntity.States.Healthy)
            {
                float dist = Vector3.Distance(transform.position, cell.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    OriginCell = cell;
                }
            }
        }
    }

    void GuardArea()
    {
        if (OriginCell == null)
        {
            FindOriginCell();
            // if (OriginCell == null)
            // {
            //     // Plus de cellule saine à garder, le warden meurt
            //     TakeDamage(MaxHealth);
            //     return;
            // }
        }

        GuardPosition = OriginCell.transform.position;

        // Vérifier la distance par rapport à la cellule d'origine
        float distanceFromCell = Vector3.Distance(transform.position, GuardPosition);

        if (distanceFromCell > MaxDistanceFromCell)
        {
            // Retourner vers la cellule
            Agent.SetDestination(GuardPosition);
            return;
        }

        // Chercher des messagers et hunters dans le rayon de garde
        Collider[] targets = Physics.OverlapSphere(GuardPosition, GuardRadius);
        MessengerEntity messengerTarget = null;

        foreach (Collider target in targets)
        {
            HunterEntity hunter = target.GetComponent<HunterEntity>();
            if (hunter != null)
            {
                currentTarget = hunter.transform;
                CurrentState = States.Surveillance;
                Agent.speed = MaxSpeed * ChaseSpeedMultiplier;
                return;
            }
            
            MessengerEntity messenger = target.GetComponent<MessengerEntity>();
            if (messenger != null)
            {
                messengerTarget = messenger;
            }
        }

        if (messengerTarget != null)
        {
            currentTarget = messengerTarget.transform;
            CurrentState = States.Surveillance;
            Agent.speed = MaxSpeed;
        }
        else
        {
            // Patrouiller autour de la cellule
            PatrolAroundCell();
        }
    }

    void PatrolAroundCell()
    {
        if (Agent.remainingDistance < 1f)
        {
            Vector3 randomPoint = GuardPosition + Random.insideUnitSphere * PatrolRadius;
            randomPoint.y = GuardPosition.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, PatrolRadius, NavMesh.AllAreas))
            {
                Agent.SetDestination(hit.position);
            }
        }
    }

    void SurveillanceArea()
    {
        if (currentTarget == null)
        {
            CurrentState = States.Guarding;
            return;
        }

        MessengerEntity messenger = currentTarget.GetComponent<MessengerEntity>();
        HunterEntity hunter = currentTarget.GetComponent<HunterEntity>();
        
        if (messenger == null && hunter == null)
        {
            CurrentState = States.Guarding;
            currentTarget = null;
            return;
        }

        // Vérifier si le messager est encore dans le rayon de garde
        float distanceFromGuard = Vector3.Distance(currentTarget.position, GuardPosition);
        float distanceFromCell = Vector3.Distance(transform.position, GuardPosition);

        if (distanceFromGuard > GuardRadius || distanceFromCell > MaxDistanceFromCell)
        {
            // Le messager est hors de portée ou on est trop loin de la cellule
            CurrentState = States.Guarding;
            currentTarget = null;
            return;
        }

        if (hunter != null)
        {
            Agent.SetDestination(currentTarget.position);
        }
        else
        {
            // Tourner autour du messager et infliger des dégâts
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 perpendicularDirection = Vector3.Cross(directionToTarget, Vector3.up).normalized;
            
            Vector3 circlePosition = currentTarget.position + perpendicularDirection * 3f;
            Agent.SetDestination(circlePosition);

            // Infliger des dégâts au messager s'il est assez proche
            if (Vector3.Distance(transform.position, currentTarget.position) < 2f)
            {
                messenger.TakeDamage(10f * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (OriginCell != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(OriginCell.transform.position, GuardRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(OriginCell.transform.position, MaxDistanceFromCell);
        }

        if (CurrentState == States.Surveillance && currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}

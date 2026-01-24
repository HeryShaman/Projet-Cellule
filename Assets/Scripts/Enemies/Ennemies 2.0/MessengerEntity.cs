using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MessengerEntity : Entity
{
    [Header("Messenger Settings")]
    public float DrainDamage = 0.1f;
    public float DetectionRadius = 8f;
    public float WardenAvoidRadius = 1f;
    public float PathRecalculationInterval = 0.5f;
    public int MaxHunters = 3;
    
    [Header("References")]
    private Vector3 Target;
    private NavMeshAgent Agent;
    private Vector3 lastTargetPosition;
    private float lastPathRecalculation;
    private ReproducerEntity[] allCells;
    
    public int CurrentHunters = 0;

    public enum States
    {
        Contaminating,
        Fleeing
    }

    public States CurrentState = States.Contaminating;

    private void Start()
    {
        base.CurrentHealth = base.MaxHealth;
        Agent = GetComponent<NavMeshAgent>();
        // Récupérer toutes les cellules au démarrage
        allCells = FindObjectsByType<ReproducerEntity>(FindObjectsSortMode.None);
    }

    protected override void Update()
    {
        base.Update();

        // Vérifier si le chemin doit être recalculé
        if (Time.time - lastPathRecalculation > PathRecalculationInterval)
        {
            CheckForObstaclesAndRecalculate();
            lastPathRecalculation = Time.time;
        }

        switch (CurrentState)
        {
            case States.Contaminating:
                Contaminating();
                break;

            case States.Fleeing:
                Fleeing();
                break;
        }
    }

    void Contaminating()
    {
        // Vérifier si player est à proximité
        Collider[] threats = Physics.OverlapSphere(transform.position, DetectionRadius);
        foreach (Collider threat in threats)
        {
            PlayerController player = threat.GetComponent<PlayerController>();
            
            if (player != null)
            {
                CurrentState = States.Fleeing;
                return;
            }
        }
        
        // Vérifier si warden est à proximité (rayon plus petit)
        Collider[] wardens = Physics.OverlapSphere(transform.position, WardenAvoidRadius);
        foreach (Collider wardenCollider in wardens)
        {
            WardenEntity warden = wardenCollider.GetComponent<WardenEntity>();
            
            if (warden != null)
            {
                CurrentState = States.Fleeing;
                return;
            }
        }

        // Aller vers la cellule neutre ou healthy la plus proche
        ReproducerEntity targetCell = FindNearestTargetCell();
        
        if (targetCell != null)
        {
            Target = targetCell.transform.position;
            Agent.SetDestination(Target);
        }
        else
        {
            // Plus de cibles, se meurt
            TakeDamage(0.1f);
        }
    }

    void Fleeing()
    {
        // Trouver la direction opposée aux menaces
        Vector3 fleeDirection = Vector3.zero;
        
        // Éviter les joueurs
        Collider[] threats = Physics.OverlapSphere(transform.position, DetectionRadius);
        foreach (Collider threat in threats)
        {
            PlayerController player = threat.GetComponent<PlayerController>();
            
            if (player != null)
            {
                fleeDirection += (transform.position - player.transform.position).normalized;
            }
        }
        
        // Éviter les wardens (rayon plus petit)
        Collider[] wardens = Physics.OverlapSphere(transform.position, WardenAvoidRadius);
        foreach (Collider wardenCollider in wardens)
        {
            WardenEntity warden = wardenCollider.GetComponent<WardenEntity>();
            
            if (warden != null)
            {
                fleeDirection += (transform.position - warden.transform.position).normalized;
            }
        }

        if (fleeDirection.magnitude > 0.1f)
        {
            Vector3 fleePosition = transform.position + fleeDirection * 10f;
            Agent.SetDestination(fleePosition);
        }
        else
        {
            // Plus de menaces, retour à la contamination
            CurrentState = States.Contaminating;
        }
    }

    ReproducerEntity FindNearestTargetCell()
    {
        ReproducerEntity nearest = null;
        float minDist = float.MaxValue;

        // Parcourir toutes les cellules connues
        foreach (ReproducerEntity cell in allCells)
        {
            if (cell != null && (cell.CurrentState == ReproducerEntity.States.Healthy || cell.CurrentState == ReproducerEntity.States.Neutral))
            {
                // Vérifier combien de messagers visent déjà cette cellule
                MessengerEntity[] allMessengers = FindObjectsByType<MessengerEntity>(FindObjectsSortMode.None);
                int messengersTargetingThisCell = 0;
                
                foreach (MessengerEntity messenger in allMessengers)
                {
                    if (messenger != this && Vector3.Distance(messenger.Target, cell.transform.position) < 5f)
                    {
                        messengersTargetingThisCell++;
                    }
                }
                
                // Prioriser les cellules moins ciblées
                float dist = Vector3.Distance(transform.position, cell.transform.position);
                float priorityScore = dist + (messengersTargetingThisCell * 10f); // Pénalité de 10 unités par messager
                
                if (priorityScore < minDist)
                {
                    minDist = priorityScore;
                    nearest = cell;
                }
            }
        }
        return nearest;
    }

    private void OnTriggerStay(Collider other)
    {
        ReproducerEntity reproducer = other.GetComponent<ReproducerEntity>();

        // Infection des cellules neutres ou saines - la messagère meurt et contamine la cellule
        if (reproducer != null && (reproducer.CurrentState == ReproducerEntity.States.Healthy || reproducer.CurrentState == ReproducerEntity.States.Neutral))
        {
            reproducer.CurrentState = ReproducerEntity.States.Infected;
            TakeDamage(MaxHealth); // La messagère meurt
            Debug.Log($"Messagère a contaminé une cellule et est morte");
        }
        
        // Gestion des chasseuses
        HunterEntity hunter = other.GetComponent<HunterEntity>();
        if (hunter != null && CurrentHunters < MaxHunters)
        {
            hunter.SetMessengerHost(this);
            CurrentHunters++;
        }
    }
    
    void CheckForObstaclesAndRecalculate()
    {
        if (Vector3.Distance(Target, lastTargetPosition) > 1f)
        {
            lastTargetPosition = Target;
            Agent.SetDestination(Target);
        }
    }
    
    public void RemoveHunter()
    {
        if (CurrentHunters > 0)
        {
            CurrentHunters--;
        }
    }
}

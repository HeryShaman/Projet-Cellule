using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HunterEntity : Entity
{
    [Header("Settings")]
    public float DrainDamage = 0.1f;
    public float SearchRadius = 6f;
    public float PatrolRadius = 8f;

    [Header("Références")]
    [SerializeField] private GameObject RippleParticlePrefab;

    private Transform Target;
    private NavMeshAgent Agent;
    private CellEntity OriginInfectedCell;
    private MessengerEntity MessengerHost;
    private float lastRippleTime;

    public enum States
    {
        SearchInfectedCell,
        PatrolAroundCell,
        AttackEntity
    }

    public States CurrentState = States.SearchInfectedCell;
    private Vector3 patrolCenter;

    private void Start()
    {
        CurrentHealth = MaxHealth;
        Agent = GetComponent<NavMeshAgent>();
        CurrentState = States.SearchInfectedCell;
    }

    public void SetMessengerHost(MessengerEntity messenger)
    {
        MessengerHost = messenger;
    }

    protected override void Update()
    {
        base.Update();

        switch (CurrentState)
        {
            case States.SearchInfectedCell:
                SearchInfectedCell();
                break;

            case States.PatrolAroundCell:
                PatrolAroundCell();
                break;

            case States.AttackEntity:
                AttackEntity();
                break;
        }
        
        RippleParticles();
    }

    void SearchInfectedCell()
    {
        // Cherche toutes les cellules infectées dans la scène
        CellEntity[] allCells = FindObjectsByType<CellEntity>(FindObjectsSortMode.None);
        CellEntity[] infectedCells = System.Array.FindAll(allCells, cell => cell.CurrentState == CellEntity.States.Infected);
        
        // Meurt si aucune cellule infectée
        if (infectedCells.Length == 0)
        {
            TakeDamage(MaxHealth);
            return;
        }

        // Vérifie d'abord si un joueur ou warden est à proximité (priorité absolue)
        Collider[] targets = Physics.OverlapSphere(transform.position, SearchRadius);
        CellController player = null;
        WardenEntity warden = null;

        foreach (Collider target in targets)
        {
            player = target.GetComponent<CellController>();
            warden = target.GetComponent<WardenEntity>();
            if (player != null || warden != null)
            {
                break;
            }
        }

        if (player != null)
        {
            Target = player.transform;
            CurrentState = States.AttackEntity;
            return;
        }
        else if (warden != null)
        {
            Target = warden.transform;
            CurrentState = States.AttackEntity;
            return;
        }

        // Sinon cherche la cellule infectée la plus proche
        CellEntity closestInfected = null;
        float minDist = float.MaxValue;

        foreach (CellEntity infectedCell in infectedCells)
        {
            float dist = Vector3.Distance(transform.position, infectedCell.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestInfected = infectedCell;
            }
        }

        if (closestInfected != null)
        {
            OriginInfectedCell = closestInfected;
            patrolCenter = closestInfected.transform.position;
            CurrentState = States.PatrolAroundCell;
        }
    }

    void PatrolAroundCell()
    {
        if (OriginInfectedCell == null || OriginInfectedCell.CurrentState != CellEntity.States.Infected)
        {
            CurrentState = States.SearchInfectedCell;
            return;
        }

        // Vérifie d'abord si un joueur ou warden est à proximité (priorité absolue)
        Collider[] targets = Physics.OverlapSphere(transform.position, SearchRadius);
        CellController player = null;
        WardenEntity warden = null;

        foreach (Collider target in targets)
        {
            player = target.GetComponent<CellController>();
            warden = target.GetComponent<WardenEntity>();
            if (player != null || warden != null)
            {
                break;
            }
        }

        if (player != null)
        {
            Target = player.transform;
            CurrentState = States.AttackEntity;
            return;
        }
        else if (warden != null)
        {
            Target = warden.transform;
            CurrentState = States.AttackEntity;
            return;
        }

        // Patrouiller autour de la cellule infectée
        if (Agent.remainingDistance < 1f)
        {
            Vector3 randomPoint = patrolCenter + Random.insideUnitSphere * PatrolRadius;
            randomPoint.y = patrolCenter.y;
            
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, PatrolRadius, NavMesh.AllAreas))
            {
                Agent.SetDestination(hit.position);
            }
        }
    }

    void AttackEntity()
    {
        if (Target == null)
        {
            CurrentState = States.PatrolAroundCell;
            return;
        }

        Agent.SetDestination(Target.position);

        float dist = Vector3.Distance(transform.position, Target.position);
        if (dist > SearchRadius * 2f)
        {
            CurrentState = States.PatrolAroundCell;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        CellController player = other.GetComponent<CellController>();
        WardenEntity warden = other.GetComponent<WardenEntity>();

        if (player != null)
        {
            player.ReceiveDamage(DrainDamage);
        }

        if (warden != null)
        {
            warden.TakeDamage(DrainDamage);
            TakeDamage(DrainDamage);
        }
    }
    
    void RippleParticles()
    {
        if (RippleParticlePrefab == null || Agent == null) return;
        
        if (Agent.velocity.magnitude > 0.5f && Time.time - lastRippleTime > 0.1f)
        {
            Vector3 ripplePosition = transform.position + Vector3.down * 0.5f;
            GameObject ripple = Instantiate(RippleParticlePrefab, ripplePosition, Quaternion.identity);
            Destroy(ripple, 2f);
            lastRippleTime = Time.time;
        }
    }

    protected override void Die()
    {
        // Chasseuses : mort silencieuse, aucun son
        Destroy(gameObject);
    }
}
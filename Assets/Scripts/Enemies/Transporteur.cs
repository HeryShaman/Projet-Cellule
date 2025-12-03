using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyTransporter : EnemyBase
{
    [Header("Transporter Settings")]
    public float detectionPlayerRadius = 7f;
    public float infectDistance = 2f;
    public float fleeSpeedMultiplier = 1.5f;
    public float searchInterval = 2f;
    public float disperseRadius = 5f;

    [Header("Regeneration Settings")]
    public float regenerationRate = 1f;
    public float stayRadius = 3f;

    // --- SUIVI DES HUNTERS ---
    [HideInInspector] public List<EnemyHunter> followers = new List<EnemyHunter>();
    public int maxFollowers = 2;

    private NavMeshAgent agent;
    private Transform player;
    private EnemyReproducer targetReproducer;
    private float nextSearchTime;

    private bool isDispersing = false;
    private Vector3 disperseTarget;

    protected override void Start()
    {
        base.Start();
        base.CurrentHealth = 1;

        agent = GetComponent<NavMeshAgent>();
        player = FindFirstObjectByType<PlayerController>().transform;
    }

    protected override void Update()
    {
        base.Update();

        // --- MODE REGENERATION ---
        if (CurrentHealth < MaxHealth)
        {
            RegenerateBehavior();
            return;
        }

        // --- MODE NORMAL ---
        if (Time.time >= nextSearchTime)
        {
            UpdateTarget();
            nextSearchTime = Time.time + searchInterval;
        }

        HandleBehavior();
    }

    // --------------------------------------------------------------------------
    // -------------------------- REGENERATION ----------------------------------
    // --------------------------------------------------------------------------

    void RegenerateBehavior()
    {
        CurrentHealth = Mathf.Min(CurrentHealth + regenerationRate * Time.deltaTime, MaxHealth);

        if (targetReproducer == null || targetReproducer.currentState != EnemyState.Infected)
        {
            targetReproducer = FindNearestInfected();
        }

        if (targetReproducer == null)
        {
            agent.SetDestination(transform.position);
            return;
        }

        float dist = Vector3.Distance(transform.position, targetReproducer.transform.position);

        if (dist > stayRadius)
            agent.SetDestination(targetReproducer.transform.position);
        else
            agent.SetDestination(transform.position);
    }

    EnemyReproducer FindNearestInfected()
    {
        EnemyReproducer[] reproducers = FindObjectsByType<EnemyReproducer>(FindObjectsSortMode.None);
        EnemyReproducer nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var r in reproducers)
        {
            if (r.currentState != EnemyState.Infected) continue;

            float dist = Vector3.Distance(transform.position, r.transform.position);

            if (dist < minDist)
            {
                nearest = r;
                minDist = dist;
            }
        }

        return nearest;
    }

    // --------------------------------------------------------------------------
    // -------------------------- CELLULE NEUTRE LA PLUS SAFE -------------------
    // --------------------------------------------------------------------------

    EnemyReproducer FindSafestNeutral()
    {
        EnemyReproducer[] reproducers = FindObjectsByType<EnemyReproducer>(FindObjectsSortMode.None);

        EnemyReproducer safest = null;
        float maxDist = -Mathf.Infinity;

        foreach (var r in reproducers)
        {
            if (r.currentState != EnemyState.Neutral) continue;

            float distPlayer = Vector3.Distance(r.transform.position, player.position);

            if (distPlayer > maxDist)
            {
                maxDist = distPlayer;
                safest = r;
            }
        }

        return safest;
    }

    // Direction perpendiculaire au joueur pour esquiver
    Vector3 GetEvadeDirection()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 evadeDir = Vector3.Cross(toPlayer, Vector3.up).normalized;

        if (Random.value > 0.5f)
            evadeDir = -evadeDir;

        return evadeDir;
    }

    // --------------------------------------------------------------------------
    // ---------------------------- MISE À JOUR DES CIBLES -----------------------
    // --------------------------------------------------------------------------

    void UpdateTarget()
    {
        EnemyReproducer[] reproducers = FindObjectsByType<EnemyReproducer>(FindObjectsSortMode.None);

        EnemyReproducer nearestNeutral = null;
        float minNeutralDist = Mathf.Infinity;

        EnemyReproducer nearestInfected = null;
        float minInfectedDist = Mathf.Infinity;

        foreach (var r in reproducers)
        {
            if (r == null) continue;

            float dist = Vector3.Distance(transform.position, r.transform.position);

            if (r.currentState == EnemyState.Neutral && dist < minNeutralDist)
            {
                nearestNeutral = r;
                minNeutralDist = dist;
            }
            else if (r.currentState == EnemyState.Infected && dist < minInfectedDist)
            {
                nearestInfected = r;
                minInfectedDist = dist;
            }
        }

        float playerDist = Vector3.Distance(transform.position, player.position);

        // ----------------------------------------------------------------------
        // -------------------------- ESQUIVE DU JOUEUR -------------------------
        // ----------------------------------------------------------------------

        if (playerDist <= detectionPlayerRadius)
        {
            isDispersing = false;

            EnemyReproducer safest = FindSafestNeutral();

            if (safest == null)
            {
                Vector3 evadeOnly = transform.position + GetEvadeDirection() * 5f;
                agent.SetDestination(evadeOnly);
            }
            else
            {
                Vector3 evade = GetEvadeDirection() * 3f;
                Vector3 targetPos = safest.transform.position + evade;
                agent.SetDestination(targetPos);
            }

            agent.speed = MaxSpeed * fleeSpeedMultiplier;
            targetReproducer = null;
            return;
        }

        // ----------------------------------------------------------------------
        // -------------------------- COMPORTEMENT NORMAL ------------------------
        // ----------------------------------------------------------------------

        if (nearestNeutral != null)
        {
            targetReproducer = nearestNeutral;
            isDispersing = false;
            agent.speed = MaxSpeed;
        }
        else if (nearestInfected != null)
        {
            targetReproducer = null;
            isDispersing = true;

            Vector3 randomOffset = Random.insideUnitSphere * disperseRadius;
            randomOffset.y = 0;
            disperseTarget = nearestInfected.transform.position + randomOffset;

            if (NavMesh.SamplePosition(disperseTarget, out NavMeshHit hit, disperseRadius, NavMesh.AllAreas))
                disperseTarget = hit.position;

            agent.speed = MaxSpeed * 0.8f;
        }
    }

    // --------------------------------------------------------------------------
    // ------------------------------ EXÉCUTION ---------------------------------
    // --------------------------------------------------------------------------

    void HandleBehavior()
    {
        if (isDispersing)
        {
            agent.SetDestination(disperseTarget);
            return;
        }

        if (targetReproducer == null) return;

        agent.SetDestination(targetReproducer.transform.position);

        float dist = Vector3.Distance(transform.position, targetReproducer.transform.position);

        if (targetReproducer.currentState == EnemyState.Neutral && dist <= infectDistance)
        {
            targetReproducer.SetInfected(true);
            Debug.Log($"{name} a infecté {targetReproducer.name} !");
        }
    }

    // --------------------------------------------------------------------------
    // -------------------------- HUNTER FOLLOW HANDLING -------------------------
    // --------------------------------------------------------------------------

    public bool TryAddHunterFollower(EnemyHunter hunter)
    {
        if (followers.Count >= maxFollowers)
            return false;

        if (!followers.Contains(hunter))
            followers.Add(hunter);

        return true;
    }

    public void RemoveHunterFollower(EnemyHunter hunter)
    {
        if (followers.Contains(hunter))
            followers.Remove(hunter);
    }
}

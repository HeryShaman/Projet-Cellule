using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyHunter : EnemyBase
{
    [Header("Hunter Settings")]
    public float rangeAroundReproducer = 5f;
    public float rangeAroundTransporter = 2f;
    public float attackRange = 2f;

    public float detectionTransporterRange = 6f; // distance pour se mettre en mode suivi
    public float followDistanceBehind = 2f;      // distance derrière le transporter

    private NavMeshAgent agent;
    private Transform player;
    private EnemyReproducer nearestReproducer;
    private EnemyTransporter nearestTransporter;
    private float wanderTimer = 2f;
    private float timer;

    // Suivi du transporter
    private EnemyTransporter currentFollowTarget = null;

    protected override void Start()
    {
        base.Start();
        base.CurrentHealth = MaxHealth;
        agent = GetComponent<NavMeshAgent>();
        player = FindFirstObjectByType<PlayerController>().transform;
        timer = wanderTimer;
    }

    protected override void Update()
    {
        base.Update();

        float playerDist = Vector3.Distance(transform.position, player.position);

        // Mode attaque
        if (playerDist <= attackRange)
        {
            AttackPlayer();
            ReleaseTransporter();
            return;
        }

        // Si on suit un transporter, continuer ce comportement
        if (currentFollowTarget != null)
        {
            FollowTransporter();
            return;
        }

        // Sinon, vérifier si un transporter peut être suivi près de nous
        LookForTransporterToFollow();

        // Si aucun suivi → comportement normal
        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            UpdateTargets();
            WanderAroundTargets();
            timer = 0f;
        }
    }

    // --------------------------------------------------------------------------
    // ----------------------------- SUIVI DU TRANSPORTEUR -----------------------
    // --------------------------------------------------------------------------

    void LookForTransporterToFollow()
    {
        EnemyTransporter[] transporters = FindObjectsByType<EnemyTransporter>(FindObjectsSortMode.None);

        EnemyTransporter nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var t in transporters)
        {
            float d = Vector3.Distance(transform.position, t.transform.position);

            if (d < detectionTransporterRange && d < minDist)
            {
                nearest = t;
                minDist = d;
            }
        }

        if (nearest == null) return;

        // Vérifier s'il reste une place (max 2 hunters)
        if (nearest.TryAddHunterFollower(this))
        {
            currentFollowTarget = nearest;
        }
    }

    void FollowTransporter()
    {
        if (currentFollowTarget == null)
            return;

        // Si trop loin → abandonner le suivi
        float dist = Vector3.Distance(transform.position, currentFollowTarget.transform.position);
        if (dist > detectionTransporterRange * 2f)
        {
            ReleaseTransporter();
            return;
        }

        // Position derrière le transporter
        Vector3 behindPos = currentFollowTarget.transform.position
                          - currentFollowTarget.transform.forward * followDistanceBehind;

        agent.SetDestination(behindPos);
    }

    void ReleaseTransporter()
    {
        if (currentFollowTarget != null)
        {
            currentFollowTarget.RemoveHunterFollower(this);
            currentFollowTarget = null;
        }
    }

    // --------------------------------------------------------------------------
    // ----------------------------- COMPORTEMENT NORMAL -------------------------
    // --------------------------------------------------------------------------

    void UpdateTargets()
    {
        nearestReproducer = FindNearest<EnemyReproducer>();
        nearestTransporter = FindNearest<EnemyTransporter>();
    }

    void WanderAroundTargets()
    {
        Vector3 destination = transform.position;

        if (nearestReproducer != null)
            destination = RandomNavSphere(nearestReproducer.transform.position, rangeAroundReproducer);
        else if (nearestTransporter != null)
            destination = RandomNavSphere(nearestTransporter.transform.position, rangeAroundTransporter);

        agent.SetDestination(destination);
    }

    void AttackPlayer()
    {
        agent.SetDestination(player.position);
    }

    T FindNearest<T>() where T : EnemyBase
    {
        T[] enemies = FindObjectsByType<T>(FindObjectsSortMode.None);
        T nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }

        return nearest;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist + origin;
        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, NavMesh.AllAreas);
        return navHit.position;
    }
}

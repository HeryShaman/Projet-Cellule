using UnityEngine;
using UnityEngine.AI;

public class WardenEntity : Entity
{
    private NavMeshAgent Agent;
    private GameManager Spawner;
    private Vector3 Target;

    public enum States
    {
        Travel,
        Growing
    }

    public States CurrentState = States.Travel;


    private void Start()
    {
        base.CurrentHealth = base.MaxHealth / base.MaxHealth;
        Agent = GetComponent<NavMeshAgent>();
        Spawner = FindAnyObjectByType<GameManager>();

        AssignNearestPoint();
    }

    protected override void Update()
    {
        base.Update();

        // Vérifie si le Warden doit mourir
        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        if (Target == Vector3.zero)
            AssignNearestPoint();

        if (CurrentState == States.Travel)
            GoToPoint();

        if (CurrentState == States.Travel && Vector3.Distance(transform.position, Target) < 1f)
            CurrentState = States.Growing;

        if (CurrentState == States.Growing)
            base.Regenerate();
    }

    public Vector3 GetNearestFreeNode(Vector3 position)
    {
        Vector3 nearest = Vector3.zero;
        float minDist = float.MaxValue;

        foreach (var node in Spawner.WardenNodes)
        {
            if (Spawner.OccupiedWardenNodes.Contains(node)) continue;

            float dist = Vector3.Distance(position, node);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }

        if (nearest != Vector3.zero)
            Spawner.OccupiedWardenNodes.Add(nearest);

        return nearest;
    }

    void AssignNearestPoint()
    {
        Target = GetNearestFreeNode(transform.position);

        if (Target == Vector3.zero)
            base.TakeDamage(0.1f);
    }

    void GoToPoint()
    {
        if (Target != Vector3.zero && Agent.isOnNavMesh)
            Agent.SetDestination(Target);
    }
}

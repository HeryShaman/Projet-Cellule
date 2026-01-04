using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HunterEntity : Entity
{
    [Header("Settings")]
    public float DrainDamage = 0.1f;
    public float SearchRadius = 6f;

    private Transform Target;
    private NavMeshAgent Agent;
    private GameManager Spawner;

    public enum States
    {
        SearchInfectedCell,
        SearchMessengerNearCell,
        FollowMessenger,
        AttackEntity
    }

    public States CurrentState = States.SearchInfectedCell;

    private void Start()
    {
        CurrentHealth = MaxHealth;
        Agent = GetComponent<NavMeshAgent>();
        Spawner = FindAnyObjectByType<GameManager>();
        CurrentState = States.SearchInfectedCell;
    }

    protected override void Update()
    {
        base.Update();

        // Vérifie si le Hunter doit mourir
        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        switch (CurrentState)
        {
            case States.SearchInfectedCell:
                SearchInfectedCell();
                break;

            case States.SearchMessengerNearCell:
                SearchMessengerNearCell();
                break;

            case States.FollowMessenger:
                FollowMessenger();
                break;

            case States.AttackEntity:
                AttackEntity();
                break;
        }
    }

    void SearchInfectedCell()
    {
        // Meurt si aucune cellule infectée
        if (Spawner.InfectedCells.Count == 0)
        {
            TakeDamage(MaxHealth);
            return;
        }

        // Vérifie d'abord si un joueur ou warden est à proximité (priorité absolue)
        Collider[] targets = Physics.OverlapSphere(transform.position, SearchRadius);
        PlayerController player = null;
        WardenEntity warden = null;

        foreach (Collider target in targets)
        {
            player = target.GetComponent<PlayerController>();
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
        ReproducerEntity closestInfected = null;
        float minDist = float.MaxValue;

        foreach (ReproducerEntity infectedCell in Spawner.InfectedCells)
        {
            if (infectedCell.CurrentState == ReproducerEntity.States.Infected)
            {
                float dist = Vector3.Distance(transform.position, infectedCell.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestInfected = infectedCell;
                }
            }
        }

        if (closestInfected != null)
        {
            Target = closestInfected.transform;
            Agent.SetDestination(Target.position);
            CurrentState = States.SearchMessengerNearCell;
        }
    }

    void SearchMessengerNearCell()
    {
        // Vérifie d'abord si un joueur ou warden est à proximité (priorité absolue)
        Collider[] targets = Physics.OverlapSphere(transform.position, SearchRadius);
        PlayerController player = null;
        WardenEntity warden = null;

        foreach (Collider target in targets)
        {
            player = target.GetComponent<PlayerController>();
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

        // Cherche un messager dans le rayon de SearchRadius autour de la cellule infectée ciblée
        if (Target == null)
        {
            CurrentState = States.SearchInfectedCell;
            return;
        }

        Collider[] messengers = Physics.OverlapSphere(Target.position, SearchRadius);
        MessengerEntity closestMessenger = null;
        float minDist = float.MaxValue;

        foreach (Collider messenger in messengers)
        {
            MessengerEntity entity = messenger.GetComponent<MessengerEntity>();
            if (entity != null)
            {
                float distToCell = Vector3.Distance(entity.transform.position, Target.position);
                if (distToCell <= SearchRadius && distToCell < minDist)
                {
                    minDist = distToCell;
                    closestMessenger = entity;
                }
            }
        }

        if (closestMessenger != null)
        {
            Target = closestMessenger.transform;
            CurrentState = States.FollowMessenger;
        }
        else
        {
            // Mouvement aléatoire autour de la cellule infectée pour un côté plus vivant
            Vector3 randomOffset = new Vector3(
                Random.Range(-SearchRadius, SearchRadius),
                0,
                Random.Range(-SearchRadius, SearchRadius)
            );
            Vector3 randomPosition = Target.position + randomOffset;
            
            Agent.SetDestination(randomPosition);
        }
    }

    void FollowMessenger()
    {
        if (Target == null)
        {
            CurrentState = States.SearchMessengerNearCell;
            return;
        }

        Agent.SetDestination(Target.position);

        // Vérifie si un joueur ou warden est dans le rayon d'attaque
        Collider[] targets = Physics.OverlapSphere(transform.position, SearchRadius);
        PlayerController player = null;
        WardenEntity warden = null;

        foreach (Collider target in targets)
        {
            player = target.GetComponent<PlayerController>();
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
        }
        else if (warden != null)
        {
            Target = warden.transform;
            CurrentState = States.AttackEntity;
        }
    }

    void AttackEntity()
    {
        if (Target == null)
        {
            CurrentState = States.SearchMessengerNearCell;
            return;
        }

        Agent.SetDestination(Target.position);

        float dist = Vector3.Distance(transform.position, Target.position);
        if (dist > SearchRadius)
        {
            CurrentState = States.SearchMessengerNearCell;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        WardenEntity warden = other.GetComponent<WardenEntity>();

        if (player != null)
        {
            player.ReceiveDamage(DrainDamage);
            TakeDamage(DrainDamage);
        }

        if (warden != null)
        {
            warden.TakeDamage(DrainDamage);
            TakeDamage(DrainDamage);
        }
    }
}
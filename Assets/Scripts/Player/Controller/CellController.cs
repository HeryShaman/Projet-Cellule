using UnityEngine;
using System.Collections;

public class CellController : MonoBehaviour
{
    [Header("Movement")]
    public float MaxSpeed = 5f;
    public float Accel = 5f;
    public float Friction = 0.5f;
    public float Gravity = 9f;

    [Header("Dash")]
    public float DashSpeed = 10f;
    public float DashTime = 0.2f;
    public float DashCooldown = 0.2f;
    [SerializeField] public bool IsDashing;

    [Header("Health")]
    public float MaxHealth = 10f;
    public float HealthRegenRate = 5f;

    public float CurrentHealth;

    [SerializeField] private float DamageRegenDelay = 2.0f;
    [SerializeField] private float LastDamageTimer;

    public Vector2 Wishvel; // Input direction
    public Vector3 Velocity;     
    private Vector3 DashVelocity;
    private Coroutine currentDashCoroutine;

    private bool accelBoostActive = false; // Small boost when starting movement
    private float accelBoostTimer = 0f;
    public bool BounceLock = false;    // Used during wallbounce


    [Header("Références")]
    [SerializeField] private CharacterController cc;
    [SerializeField] private InputReader input;

    void Start()
    {
        CurrentHealth = MaxHealth;
        cc = GetComponent<CharacterController>();
    }

    void ProcessInput()
    {
        Wishvel = Vector2.zero;
        Wishvel = input.MoveDirection;
    }


    void Update()
    {
        ProcessInput();
        if (!accelBoostActive && Wishvel.magnitude > 0.1f && Velocity.magnitude < 0.1f)
        {
            accelBoostActive = true;
            accelBoostTimer = 0.1f;
        }

        RegenerateHealth();
        
        // DASH MOVEMENT
        if (IsDashing)
        {
            cc.Move(Velocity * Time.deltaTime);
        }
        else
        {
            ApplyGravity();
            Move();
            cc.Move(Velocity * Time.deltaTime);

            // Start dash if input held and conditions met
            if (input != null && input.DashHeld && Time.time - LastDamageTimer >= DamageRegenDelay)
                currentDashCoroutine = StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        IsDashing = true;
        BounceLock = false;

        DashVelocity = Wishvel.magnitude > 0.1f
            ? new Vector3(Wishvel.x, 0, Wishvel.y).normalized
            : transform.forward;

        // Small slow-motion effect
        Time.timeScale = 0.75f;
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1f;

        float timer = 0f;

        // Dash Move Loop
        while (timer < DashTime)
        {
            // Dash direction
            Vector3 DashDir = new Vector3(Wishvel.x, 0, Wishvel.y).normalized;
            // Dash Movement

            if (Wishvel.magnitude > 0.1f)
            {
                DashVelocity = Vector3.Lerp(DashVelocity, DashDir, 0.12f); // 0.12f
            }

            Velocity = DashVelocity * DashSpeed;

            timer += Time.deltaTime;
            yield return null;
        }
        Velocity *= Friction;
        yield return new WaitForSeconds(DashCooldown);
        IsDashing = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsDashing) return;

        // Bounce only on walls (not floor)
        if (hit.normal.y < 0.5f)
        {
            Vector3 bounce = Vector3.Reflect(Velocity.normalized, hit.normal);
            bounce.y = 0f;

            float bounceStrength = DashSpeed * 1.2f;
            Velocity = bounce.normalized * bounceStrength;

            DashVelocity = bounce.normalized;
            BounceLock = true;
        }
    }


    void Move()
    {
        // Direction
        Vector3 Dir = new Vector3(Wishvel.x, 0f, Wishvel.y);

        // Movement
        if (Wishvel.magnitude > 0.1f)
            Velocity = Vector3.Lerp(Velocity, Dir * MaxSpeed, Accel * Time.deltaTime);
        else
            Velocity *= Friction * Time.deltaTime; 
    }


    void ApplyGravity()
    {
        if (!cc.isGrounded)
            Velocity.y -= Gravity * Time.deltaTime;
        else
            Velocity.y = -1f;
    }

    void RegenerateHealth()
    {
        if (Time.time - LastDamageTimer >= DamageRegenDelay)
        {
            CurrentHealth   = Mathf.MoveTowards(CurrentHealth, MaxHealth, Time.deltaTime * HealthRegenRate);
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        }
    }

    public void ReceiveDamage(float amount)
    {
        CurrentHealth -= amount;
        LastDamageTimer = Time.time;
        
        // Vérifier si le joueur est mort
        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        // Détruire le joueur
        Destroy(gameObject);
    }
}
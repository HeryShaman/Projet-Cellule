using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float MaxSpeed = 5f;
    public float Accel = 5f;
    public float Friction = 0.5f;
    public float Gravity = 9f;

    [Header("Dash")]
    public float DashSpeed;
    public float DashTime;

    public float DashCooldown = 0.5f;

    [Header("Charge")]
    public float ChargeRate = 0.1f;
    public float MaxCharge = 1f;

    [Header("Stamina")]
    public float RateStamina = 5f;
    public float MaxStamina = 100f;


    [SerializeField] private bool IsCharging;
    [SerializeField] private bool IsDashing;
    [SerializeField] private float DamageRegenDelay = 0.1f;
    [SerializeField] private float LastDamageTime;
    [SerializeField] private bool IsInsideWardenCollider;

    [Header("Dash Enhancment")]
    public float WardenSpeedMultiplier = 1.3f;

    [Header("Graphics")]
    public Transform PlayerModel;
    public float RotationModel;

    public float MinScale = 0.5f;
    public float MaxScale = 1.5f;

    public AudioClip[] clips;

    public float CurrentCharge;
    public float CurrentStamina;

    private Vector3 velocity;
    private Vector3 DashDir;
    private Vector2 wishvel;

    [Header("Références")]
    [SerializeField] private CharacterController cc; // cc = character controller
    [SerializeField] private InputReader input; // ir = input reader
    [SerializeField] public CameraController Cam;
    [SerializeField] private ProceduralPlayerAnim Anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        CurrentStamina = 0;
    }

    void ProcessInput()
    {
        Cam.TiltDir = wishvel.normalized;
        wishvel = Vector2.zero;

        wishvel = input.MoveDirection;

        // Empêcher charge/dash pendant un dash actif
        if (input.DashPressedThisFrame && !IsDashing)
            IsCharging = true;

        if (input.DashReleasedThisFrame && !IsDashing)
            IsCharging = false;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();

        ModelScale();


        #region Gestion Dash/ Charge
        if (IsCharging)
        {

            Cam.CameraZoom(Cam.OriginalFov + 8f, 5f);
            Cam.CameraShaking(0.04f, 1f);
            ChargeDash();
            Anim.MoveAnim(wishvel);
        }
        else
        {
            RegenerateStamina();
        }

        // Quand la touche est relâchée, on effectue le dash
        if (CurrentCharge > 0.2f && !IsCharging && !IsDashing)
        {
            StartCoroutine(Dash(CurrentCharge));
            CurrentStamina = Mathf.Clamp(CurrentStamina - CurrentCharge, 0, MaxStamina);
            CurrentCharge = 0.0f;
        }
        #endregion

        #region Movement Logic

        if (cc.isGrounded && !IsCharging && !IsDashing)
        {
            Move();
            Cam.CameraZoom(Cam.OriginalFov, 10f);
            Anim.MoveAnim(wishvel);
        }
        else
        {
            ApplyGravity();
        }

        #endregion

        //Debug.Log("Current Stamina:" + CurrentStamina);
        Debug.Log(IsInsideWardenCollider);


        cc.Move(velocity * Time.deltaTime);
    }

    void Move()
    {
    // Direction
        Vector3 Dir = transform.TransformDirection(new Vector3(wishvel.x, 0f, wishvel.y));
        
        // Vitesse augmentée si dans un warden (détecté par collider)
        float currentMaxSpeed = IsInsideWardenCollider ? MaxSpeed * WardenSpeedMultiplier : MaxSpeed;

    // Acceleration
        if (wishvel.magnitude > 0.1f)
        {
            velocity.x = Mathf.Lerp(velocity.x, Dir.x * currentMaxSpeed, Accel * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, Dir.z * currentMaxSpeed, Accel * Time.deltaTime);
        }
    // Friction
        else
        {
            velocity *= Friction;
        }

    // Clamp Axe Y
        if (velocity.y <= 0)
        {
            velocity.y = 0;
        }
    }

    void ApplyGravity()
    {
        velocity.y -= Gravity * Time.deltaTime;
    }

    void ChargeDash()
    {
        velocity.x = 0;
        velocity.z = 0;

        //Clamp Min
        if (CurrentCharge < 0.2f)
        {
            CurrentCharge = 0.2f;
        }

        // Logique de charge
        CurrentCharge = Mathf.MoveTowards(CurrentCharge, MaxCharge, Time.deltaTime * ChargeRate);
        Debug.Log(CurrentCharge);
    }


    IEnumerator Dash(float DashPeriod)
    {
        IsDashing = true;

        // Récupère la direction du mouvement (si le joueur se déplace)
        if (wishvel.magnitude > 0.1f)
        {
            DashDir = new Vector3(wishvel.x, 0f, wishvel.y).normalized;
        }
        else
        {
            DashDir = transform.forward; // Si le joueur n'est pas en mouvement, dash dans la direction où il regarde
        }

        // Appliquer la vitesse du dash
        float dashTime = 0f;
        while (dashTime < DashPeriod)
        {
            velocity = DashDir * DashSpeed;
            dashTime += Time.deltaTime;
            yield return null;
        }

        // Cooldown entre les dashes
        yield return new WaitForSeconds(DashCooldown);

        IsDashing = false;
    }


    void ModelScale()
    {
        // Clamp et Normalization du scale pour ajuster le scale
        float NormalizedScale = Mathf.Clamp01(CurrentStamina / MaxStamina);
        float TargetScale = Mathf.Lerp(MinScale, MaxScale, NormalizedScale);

        // Application du scale
        PlayerModel.localScale = Vector3.one * TargetScale;
    }


    void RegenerateStamina()
    {
        // Vérifie si le délai après dégâts est écoulé
        if (Time.time - LastDamageTime >= DamageRegenDelay)
        {
            // Recharge Stamina
            CurrentStamina = Mathf.MoveTowards(CurrentStamina, MaxStamina, Time.deltaTime * RateStamina);

            // Clamp min, max
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
        }
    }

    // Détecte l'entrée dans un warden (trigger)
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WardenEntity>() != null)
        {
            IsInsideWardenCollider = true;
        }
    }

    // Détecte la sortie d'un warden (trigger)
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<WardenEntity>() != null)
        {
            IsInsideWardenCollider = false;
        }
    }

    // Détecte en continu si on est dans un warden (plus précis pendant le dash)
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<WardenEntity>() != null)
        {
            IsInsideWardenCollider = true;
        }
    }

    public void ReceiveDamage(float amount)
    {
        CurrentStamina -= amount;
        LastDamageTime = Time.time; // Enregistre l'heure des dégâts
    }
}

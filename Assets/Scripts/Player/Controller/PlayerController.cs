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

    [Header("Graphics")]
    public Transform PlayerModel;
    public float RotationModel;

    public float MinScale = 0.5f;
    public float MaxScale = 1.5f;

    public AudioClip[] clips;

    private float CurrentCharge;
    private float CurrentStamina;

    private Vector3 velocity;
    private Vector3 DashDir;
    private Vector2 wishvel;

    [Header("Références")]
    [SerializeField] private CharacterController cc; // cc = character controller
    [SerializeField] private InputReader input; // ir = input reader
    [SerializeField] private CameraController Cam;
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

        if (input.DashPressedThisFrame)
            IsCharging = true;

        if (input.DashReleasedThisFrame)
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

        Debug.Log("Current Stamina:" + CurrentStamina);
        Debug.Log(CurrentCharge);

        cc.Move(velocity * Time.deltaTime);
    }

    void Move()
    {
    // Direction
        Vector3 Dir = transform.TransformDirection(new Vector3(wishvel.x, 0f, wishvel.y));

    // Acceleration
        if (wishvel.magnitude > 0.1f)
        {
            velocity.x = Mathf.Lerp(velocity.x, Dir.x * MaxSpeed, Accel * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, Dir.z * MaxSpeed, Accel * Time.deltaTime);
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

        // Après le dash, revenir à la vitesse normale (ou appliquer une petite friction)
        velocity *= Friction;

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
        // Recharge Stamina
        CurrentStamina = Mathf.MoveTowards(CurrentStamina, MaxStamina, Time.deltaTime * RateStamina);

        // Clamp min, max
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {

        }
    }
}

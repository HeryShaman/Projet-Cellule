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
    public float DashSpeed = 10f;
    public float DashTime = 0.0f;
    public float DashCooldown = 0.2f;
    public float DashShaderIntensity = 2f;

    [Header("Stamina")]
    public float RateStamina = 5f;
    public float MaxStamina = 100f;

    [SerializeField] private bool IsDashing;
    [SerializeField] private float DamageRegenDelay = 0.1f;
    [SerializeField] private float LastDamageTime;
    [SerializeField] private bool IsInsideWardenCollider;

    [Header("Dash Enhancment")]
    public float WardenSpeedMultiplier = 1.3f;

    [Header("Graphics")]
    public Transform PlayerModel;
    public float RotationModel;
    public Renderer PlayerRenderer;
    public Material DashMaterial;
    public Material OriginalMaterial;
    public float DashShakeIntensity = 0.5f;
    public float DashShakeDuration = 0.3f;
    public float ReproducerShakeIntensity = 0.2f;
    public float ReproducerShakeDuration = 0.2f;

    public float MinScale = 0.5f;
    public float MaxScale = 1.5f;

    public AudioClip[] clips;

    public float CurrentStamina;
    public bool IsDead = false;

    private Vector3 velocity;
    private Vector3 DashDir;
    private Vector2 wishvel;
    private Material playerMaterial;
    private float originalShaderIntensity;
    private bool hasCollidedDuringDash;

    [Header("Références")]
    [SerializeField] private CharacterController cc; // cc = character controller
    [SerializeField] public InputReader input; // ir = input reader
    [SerializeField] private ProceduralPlayerAnim Anim;
    [SerializeField] public ParticleSystem Effects;


[Header("Dash Settings")]
    public float DashDamageRadius = 2f;
    public string[] EnemyTags = {"Enemy"};
    public int DashDamage = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        CurrentStamina = 0;
        
        // Récupérer le material du joueur pour le shader
        if (PlayerRenderer != null)
        {
            playerMaterial = PlayerRenderer.material;
            // Sauvegarder le material original
            OriginalMaterial = PlayerRenderer.material;
            // Sauvegarder l'intensité originale du shader
            if (playerMaterial.HasProperty("_Amplitude"))
            {
                originalShaderIntensity = playerMaterial.GetFloat("_Amplitude");
            }
        }
    }

    void ProcessInput()
    {
        if (input == null)
        {
            Debug.LogWarning("InputReader non assigné au PlayerController !");
            return;
        }
        
        wishvel = Vector2.zero;

        wishvel = input.MoveDirection;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDead) return;
        
        if (input == null)
        {
            Debug.LogWarning("InputReader non assigné au PlayerController !");
            return;
        }
        
        ProcessInput();

        ModelScale();

        // Quand la touche est relâchée, on effectue le dash
        if (input.DashHeld && !IsDashing)
        {
            StartCoroutine(Dash());
        }
        else
        {
            RegenerateStamina();
        }

        #region Movement Logic

        if (cc.isGrounded && !IsDashing)
        {
            Move();
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
        
        // Vérifier si le joueur doit mourir
        if (CurrentStamina <= 0 && !IsDead)
        {
            Die();
        }
    }

    void Move()
    {
        if (input == null)
        {
            Debug.LogWarning("InputReader non assigné au PlayerController !");
            return;
        }
        
    // Direction
        Vector3 Dir = transform.TransformDirection(new Vector3(wishvel.x, 0f, wishvel.y));
        
        // Vitesse augmentée si dans un warden (détecté par collider)
        float currentMaxSpeed = IsInsideWardenCollider ? MaxSpeed * WardenSpeedMultiplier : MaxSpeed;

    // Acceleration
        if (wishvel.magnitude > 0.1f)
        {
            velocity.x = Mathf.Lerp(velocity.x, Dir.x * currentMaxSpeed, Accel * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, Dir.z * currentMaxSpeed, Accel * Time.deltaTime);

            var emission = Effects.emission;
            emission.enabled = true;
        }
    // Friction
        else
        {
            var emission = Effects.emission;
            emission.enabled = false;
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


    IEnumerator Dash()
    {
        if (input == null)
        {
            Debug.LogWarning("InputReader non assigné au PlayerController !");
            yield break;
        }
        
        IsDashing = true;
        hasCollidedDuringDash = false;

        // Appliquer le material blanc du dash
        if (PlayerRenderer != null && DashMaterial != null)
        {
            PlayerRenderer.material = DashMaterial;
        }

        // Récupère la direction du mouvement
        if (wishvel.magnitude > 0.1f)
        {
            DashDir = new Vector3(wishvel.x, 0f, wishvel.y).normalized;
        }
        else
        {
            // Si le joueur n'est pas en mouvement, dash dans la direction où il regarde
            DashDir = transform.forward;
        }

        // Dash minimal plus court
        float dashTime = 0f;
        float shortDashTime = DashTime * 0.3f; // 30% du temps original
        
        while (dashTime < shortDashTime && !hasCollidedDuringDash)
        {
            velocity = DashDir * DashSpeed;
            
            // Orienter le modèle pendant le dash
            Anim.DashAnim(DashDir);
            
            // Vérifier les collisions avec les ennemis pendant le dash
            CheckDashCollisions();
            
            dashTime += Time.deltaTime;
            yield return null;
        }

        // Restaurer le material original
        if (PlayerRenderer != null && OriginalMaterial != null)
        {
            PlayerRenderer.material = OriginalMaterial;
        }

        // Cooldown entre les dashes
        yield return new WaitForSeconds(DashCooldown);

        IsDashing = false;
    }
    
    void CheckDashCollisions()
    {
        // Trouver tous les ennemis dans le rayon de dash en utilisant les tags
        Collider[] allColliders = Physics.OverlapSphere(transform.position, DashDamageRadius);
        
        foreach (Collider collider in allColliders)
        {
            // Vérifier si le collider a un tag d'ennemi
            bool isEnemy = false;
            foreach (string tag in EnemyTags)
            {
                if (collider.CompareTag(tag))
                {
                    isEnemy = true;
                    break;
                }
            }
            
            if (!isEnemy) continue;
            
            hasCollidedDuringDash = true;
            
            // Vérifier si c'est une messagère - kill instantané avec screen shake
            MessengerEntity messenger = collider.GetComponent<MessengerEntity>();
            if (messenger != null)
            {
                messenger.TakeDamage(messenger.MaxHealth);
                ApplyScreenShake(DashShakeIntensity, DashShakeDuration);
                Debug.Log($"Dash kill instantané sur {collider.name}");
                continue;
            }
            
            // Vérifier si c'est une ReproducerEntity - devient healthy avec screen shake léger
            ReproducerEntity reproducer = collider.GetComponent<ReproducerEntity>();
            if (reproducer != null)
            {
                reproducer.CurrentState = ReproducerEntity.States.Healthy;
                ApplyScreenShake(ReproducerShakeIntensity, ReproducerShakeDuration);
                Debug.Log($"Dash sur {collider.name} - devient healthy");
                continue;
            }
            
            // Vérifier si c'est une autre entité
            Entity entity = collider.GetComponent<Entity>();
            if (entity != null)
            {
                // Infliger des dégâts normaux
                entity.TakeDamage(DashDamage);
                Debug.Log($"Dash damage sur {collider.name}");
            }
        }
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
        if (IsDead) return;
        
        CurrentStamina -= amount;
        LastDamageTime = Time.time; // Enregistre l'heure des dégâts
        
        // Clamp pour éviter les valeurs négatives
        CurrentStamina = Mathf.Max(0, CurrentStamina);
    }
    
    public void TakeDamage(int damage)
    {
        CurrentStamina -= damage;
        Debug.Log($"Player took {damage} damage, stamina: {CurrentStamina}");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualiser le rayon de dégâts du dash
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DashDamageRadius);
    }
    
    void ApplyScreenShake(float intensity, float duration)
    {
        // Trouver la caméra principale et appliquer le screen shake
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            StartCoroutine(ScreenShakeFallback(intensity, duration));
        }
    }
    
    IEnumerator ScreenShakeFallback(float intensity, float duration)
    {
        Vector3 originalPosition = Camera.main.transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            float x = UnityEngine.Random.Range(-1f, 1f) * intensity;
            float y = UnityEngine.Random.Range(-1f, 1f) * intensity;
            
            Camera.main.transform.position = originalPosition + new Vector3(x, y, 0);
            
            yield return null;
        }
        
        Camera.main.transform.position = originalPosition;
    }
    
    void Die()
    {
        IsDead = true;
        Debug.Log("Le joueur est mort");
        
        // Désactiver le gameObject du joueur
        gameObject.SetActive(false);
        
        // Le vaisseau mère va gérer le respawn
    }
}

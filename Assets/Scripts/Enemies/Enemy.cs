using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float MaxSpeed;
    public float MaxHealth = 10f;
    public float CurrentHealth;

    public float actionRadius;


    [Header("Visual")]
    public Transform EnemyModel;
    public float MinScale = 0.5f;
    public float MaxScale = 1.5f;
    public float ScaleSpeed = 5f; // vitesse du lerp

    private Vector3 targetScale;

    protected virtual void Start()
    {
        if (EnemyModel != null)
            targetScale = Vector3.one * MaxScale;
    }

    protected virtual void Update()
    {
        // Lerp fluide de la taille
        if (EnemyModel != null)
        {
            Vector3 currentScale = EnemyModel.localScale;
            EnemyModel.localScale = Vector3.Lerp(currentScale, targetScale, ScaleSpeed * Time.deltaTime);
        }
        UpdateTargetScale();
    }

    public virtual void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        UpdateTargetScale();
        if (CurrentHealth <= 0) Die();

    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void UpdateTargetScale()
    {
        if (EnemyModel == null) return;
        float normalizedHealth = Mathf.Clamp01(CurrentHealth / MaxHealth);
        float scale = Mathf.Lerp(MinScale, MaxScale, normalizedHealth);
        targetScale = Vector3.one * scale;
    }
}
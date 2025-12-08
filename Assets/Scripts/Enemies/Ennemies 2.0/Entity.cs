using UnityEngine;



public class Entity : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float CurrentHealth;
    public float MaxHealth;

    [Header("Enemy Graphics")]
    public Transform EnemyModel;
    public float MinScale;
    public float MaxScale;
    public enum EnemyStates
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateModelScale();
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    public virtual void TakeDamage(float amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {

    }

    void UpdateModelScale()
    {
        // Clamp et Normalization du scale pour ajuster le scale
        float NormalizedScale = Mathf.Clamp01(CurrentHealth / MaxHealth);
        float TargetScale = Mathf.Lerp(MinScale, MaxScale, NormalizedScale);

        // Application du scale
        EnemyModel.localScale = Vector3.one * TargetScale;
    }
}

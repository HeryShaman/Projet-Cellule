using UnityEngine;



public class Entity : MonoBehaviour

{

    [Header("Entity Stats")]

    public float CurrentHealth;

    public float MaxHealth;

    public float RegenRateHealth;



    [Header("Entity Graphics")]

    public Transform EnemyModel;

    public Vector3 MinScale;

    public Vector3 MaxScale;







    // Update is called once per frame

    protected virtual void Update()

    {

        UpdateModelScale();

    }



    public virtual void TakeDamage(float amount)

    {

        CurrentHealth -= amount;



        if (CurrentHealth <= 0)

        {

            Die();

        }

    }



    protected virtual void Die()

    {

        Destroy(gameObject);

    }



    void UpdateModelScale()

    {

        // Clamp et Normalization du scale pour ajuster le scale

        float NormalizedScale = Mathf.Clamp01(CurrentHealth / MaxHealth);

        Vector3 TargetScale = Vector3.Lerp(MinScale, MaxScale, NormalizedScale);



        // Application du scale

        EnemyModel.localScale = TargetScale;

    }



    protected virtual void Regenerate()

    {

        if (CurrentHealth < MaxHealth)

            CurrentHealth += RegenRateHealth * Time.deltaTime;

    }

}


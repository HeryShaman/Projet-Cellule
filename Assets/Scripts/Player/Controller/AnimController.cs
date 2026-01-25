using UnityEngine;

public class AnimController : MonoBehaviour
{
    public Vector3[] CoreScale;

    [Header("R�f�rences")]
    [SerializeField] private Transform SphericalCore;
    [SerializeField] private Transform Cytoplasm;

    [SerializeField] private Material[] CoreMaterials;
    [SerializeField] private Material[] CytoplasmMaterials;

    [SerializeField] private TrailRenderer DashTrail;
    [SerializeField] private CellController Anim;
    [SerializeField] private GameObject RippleParticlePrefab;

    private float lastRippleTime;



    void Start()
    {
        Anim = GetComponent<CellController>();
    }

    void Update()
    {
        CoreDirection();
        ModelScale();
        ActiveDashTrail();
        UpdateMaterials();
        RippleParticles();
    }

    // Défini la direction et le mouvement du noyau
    void CoreDirection()
    {
        if (Anim == null || SphericalCore == null) return;
        
        Vector3 velocity = Anim.Velocity;
        Vector3 coreDir = new Vector3(velocity.x, 0f, velocity.z);
        
        if (coreDir.magnitude > 0.1f)
            SphericalCore.localPosition = Vector3.Lerp(SphericalCore.localPosition, coreDir.normalized * 0.1f, Time.deltaTime * 5f);
        else
            SphericalCore.localPosition = Vector3.Lerp(SphericalCore.localPosition, Vector3.zero, Time.deltaTime * 5f);
    }

    void CytoplasmAnimation()
    {

    }

    void UpdateMaterials()
    {
        if (CoreMaterials == null || CytoplasmMaterials == null) return;

        if (CoreMaterials.Length == 0 || CytoplasmMaterials.Length == 0) return;

        if (Anim == null) return;

        if (SphericalCore == null) return;

        if (Anim.IsDashing)
        {
            // Dash material
            SphericalCore.GetComponent<Renderer>().material = CoreMaterials[1];
            Cytoplasm.GetComponent<Renderer>().material = CytoplasmMaterials[1];
        }
        else
        {
            // Normal material
            SphericalCore.GetComponent<Renderer>().material = CoreMaterials[0];
            Cytoplasm.GetComponent<Renderer>().material = CytoplasmMaterials[0];
        }
    }

    void RippleParticles()
    {
        if (RippleParticlePrefab == null || Anim == null) return;
        
        if (Anim.Velocity.magnitude > 0.5f && Time.time - lastRippleTime > 0.1f)
        {
            Vector3 ripplePosition = transform.position + Vector3.down * 0.5f;
            GameObject ripple = Instantiate(RippleParticlePrefab, ripplePosition, Quaternion.identity);
            Destroy(ripple, 2f);
            lastRippleTime = Time.time;
        }
    }

    void ActiveDashTrail()
    {
        if (DashTrail == null) return;
        
        if (Anim.IsDashing)
        {
            DashTrail.emitting = true;
        }
        else
        {
            DashTrail.emitting = false;
        }
    }

    void ModelScale()
    {
        if (SphericalCore == null) return;

        float NormalizedScale = Mathf.Clamp01(Anim.CurrentHealth / Anim.MaxHealth);
        Vector3 TargetScale = Vector3.Lerp(CoreScale[0], CoreScale[1], NormalizedScale);

        SphericalCore.localScale = TargetScale;
    }
}

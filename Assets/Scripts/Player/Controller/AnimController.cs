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



    void Start()
    {
        Anim = GetComponent<CellController>();
    }

    void Update()
    {
        ModelScale();
        CoreDirection();
    }

    // Défini la direction et le mouvement du noyau
    void CoreDirection()
    {
        if (Anim == null || SphericalCore == null) return;
        
        Vector3 velocity = Anim.Velocity;
        Vector3 coreDir = new Vector3(velocity.x, 0f, velocity.z);
        
        if (coreDir.magnitude > 0.25f)
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

        if (Anim.IsDashing)
        {
            CoreMaterials[0].SetFloat("_EmissionGain", 1f);
            CytoplasmMaterials[0].SetFloat("_EmissionGain", 1f);
        }
        else
        {
            CoreMaterials[0].SetFloat("_EmissionGain", 0f);
            CytoplasmMaterials[0].SetFloat("_EmissionGain", 0f);
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

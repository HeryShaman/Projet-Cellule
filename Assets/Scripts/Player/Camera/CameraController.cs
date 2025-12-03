using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    public Transform Target;          // Ce que la caméra regarde (souvent le joueur)

    [Header("Camera Position")]
    public float CamLength;
    public float CamHeight;

    [Header("Camera Offset")]
    public float LerpSpeed = 1f;
    public float FollowDistance = 5f;
    public float TeleportDistanceThreshold = 50f;
    private Vector3 Offset;
    public Quaternion CamRotation;

    [Header("Camera Shake")]
    public float shakeTimer;
    public float shakeMagnitude;

    [Header("Camera Tilt")]
    public float maxOffset = 1.5f;    // distance max que la caméra peut s'éloigner du centre
    public float tiltSmooth = 3f;     // vitesse de lerp

    private Vector3 currentOffset;     // offset courant
    private Vector3 targetOffset;      // offset cible selon direction

    public Vector2 TiltDir;     // direction du joueur normalisée

    [Header("Field of View")]
    public float OriginalFov = 60f;
    public float targetFov;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (Target == null) { Debug.LogWarning("Assignation Manquante"); return; }
    }

    void LateUpdate()
    {
        CameraTracking();
        CameraTilting();
        HandleCameraShake();
    }

    void CameraTracking()
    {
        Vector3 CamInitialPos = new Vector3(0, CamHeight, -CamLength);
        Vector3 TargetPos = Target.position + CamInitialPos;

        transform.position = TargetPos;
        transform.rotation = CamRotation;
        transform.LookAt(Target);
    }

    public void CameraShaking(float magnitude, float duration)
    {
        shakeMagnitude = magnitude;
        shakeTimer = duration;
    }

    void HandleCameraShake()
    {
    if (shakeTimer > 0f)
       {
            transform.position += Random.insideUnitSphere * shakeMagnitude;
            shakeTimer -= Time.deltaTime;
       }
    }

    void CameraTilting()
    {
        // Direction du tilt
        targetOffset = new Vector3(TiltDir.x, 0f, TiltDir.y) * maxOffset;

        // Lissage pour que ce soit doux
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * tiltSmooth);

        // Appliquer l'offset à la position de la caméra
        transform.position += currentOffset;
    }

    public void CameraZoom(float newTargetFov, float zoomSpeed)
    {
        targetFov = newTargetFov;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
    }
}
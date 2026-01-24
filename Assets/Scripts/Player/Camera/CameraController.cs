using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Field of View")]
    public float BaseFov = 75f;
    public float DashFov = 10f;
    public float CamSpeed = 5f;

    private Quaternion CamRotation;
    public Transform[] targets;
    public Vector3[] PosOffsets;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        Transform playerTarget = FindFirstObjectByType<CellController>()?.transform;
        Transform motherShipTarget = FindFirstObjectByType<PlayerMotherShip>()?.transform;

        Transform currentTarget = null;
        bool isPlayerTarget = false;

        if (playerTarget != null)
        {
            currentTarget = playerTarget;
            isPlayerTarget = true;
        }
        else if (motherShipTarget != null)
        {
            currentTarget = motherShipTarget;
            isPlayerTarget = false;
        }

        // Appliquer les behaviours selon la cible actuelle
        if (currentTarget != null)
        {
            if (isPlayerTarget)
            {
                PlayerCameraBehaviour(currentTarget);
            }
            else
            {
                MotherShipCameraBehaviour(currentTarget);
            }
        }
    }

    void PlayerCameraBehaviour(Transform playerTarget)
    {
        // Position + Orientation Caméra
        CameraBehaviour.CamPositionOffset(cam, playerTarget, PosOffsets[1]);
        CameraBehaviour.LookTarget(cam, playerTarget);

        CellController cellController = playerTarget.GetComponent<CellController>();
        if (cellController != null)
        {
            if (cellController.IsDashing)
            {
                CameraBehaviour.CamFOV(cam, BaseFov + DashFov, CamSpeed);
                StartCoroutine(CameraBehaviour.CameraShake(cam, 0.2f, 0.1f));
            }
            else
            {
                CameraBehaviour.CamFOV(cam, BaseFov, CamSpeed);
            }

            if (cellController.BounceLock)
            {
                StartCoroutine(CameraBehaviour.HitPause(0.05f));
            }
            else
            {
                return;
            }
        }
    }

    void MotherShipCameraBehaviour(Transform motherShipTarget)
    {
        CameraBehaviour.CamPositionOffset(cam, motherShipTarget, PosOffsets[0]);
        CameraBehaviour.LookTarget(cam, motherShipTarget);
        CameraBehaviour.CamFOV(cam, BaseFov, CamSpeed);
    }
}
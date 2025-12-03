using Unity.VisualScripting;
using UnityEngine;

public class ProceduralPlayerAnim : MonoBehaviour
{
    [SerializeField] private Transform PlayerModel;

    public float MaxOffset = 0.5f;
    public float MoveSpeed = 1f;
    public float ReturnSpeed = 0.5f;

    private Vector3 InitialPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerModel == null) PlayerModel = transform;
        InitialPos = PlayerModel.localPosition;
    }

    public void MoveAnim(Vector2 Direction)
    {
        // Normalize la direction
        Vector2 Dir = Direction.normalized;

        // Défini le mouvement sur l'axe XZ
        Vector3 TargetLocal = InitialPos + new Vector3(Dir.x, 0f, Dir.y) * MaxOffset;

        // Si input proche de 0
        if (Direction.magnitude < 0.05f)
        {
            PlayerModel.localPosition = Vector3.Lerp(PlayerModel.localPosition, InitialPos, Time.deltaTime * ReturnSpeed);
        }
        else
        {
            // Se déplace dans la direction maintenu
            PlayerModel.localPosition = Vector3.Lerp(PlayerModel.localPosition, TargetLocal, Time.deltaTime * MoveSpeed);
        }        
    }

    void ShakeAnim()
    {

    }
}

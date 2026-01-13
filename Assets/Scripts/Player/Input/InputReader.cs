using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference dash;

    // Move
    public Vector2 MoveDirection => move.action.ReadValue<Vector2>();

    public bool DashHeld => dash.action.IsPressed();

    private void OnEnable()
    {
        move.action.Enable();
        dash.action.Enable();
    }

    private void OnDisable()
    {
        move.action.Disable();
        dash.action.Disable();
    }
}

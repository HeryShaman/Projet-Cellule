using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference dash;

    // Move
    public Vector2 MoveDirection => move.action.ReadValue<Vector2>();

    // Dash
    public bool DashPressedThisFrame => dash.action.WasPressedThisFrame();
    public bool DashHeld => dash.action.IsPressed();
    public bool DashReleasedThisFrame => dash.action.WasReleasedThisFrame();

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

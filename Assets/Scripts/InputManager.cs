using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputActions _input; // 自動生成クラス
    public event Action<Vector2> OnMove;
    public event Action OnAttack;

    private void Awake()
    {
        ServiceLocator.Set(this);
        _input = new InputActions();
    }

    private void OnEnable()
    {
        _input.Enable();

        _input.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        _input.Player.Attack.performed += ctx => OnAttack?.Invoke();
        _input.Player.Attack.canceled += ctx => OnAttack?.Invoke();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    public Vector2 GetMoveInput()
    {
        return _input.Player.Move.ReadValue<Vector2>();
    }
}

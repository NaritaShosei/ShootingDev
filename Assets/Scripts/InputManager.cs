using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputActions _input;
    public event Action<Vector2> OnMove;
    public event Action OnAttack;
    public event Action<Vector2> OnMouseMove; // マウス移動イベント追加

    private void Awake()
    {
        ServiceLocator.Set(this);
        _input = new InputActions();
    }

    private void OnEnable()
    {
        _input.Enable();

        // 移動入力
        _input.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        // 攻撃入力
        _input.Player.Attack.performed += ctx => OnAttack?.Invoke();

        // マウス移動（必要に応じて）
        _input.Player.Look.performed += ctx => OnMouseMove?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    public Vector2 GetMoveInput()
    {
        return _input.Player.Move.ReadValue<Vector2>();
    }

    // マウス位置を取得
    public Vector2 GetMousePosition()
    {
        return Mouse.current.position.ReadValue();
    }

    // 攻撃ボタンが押されているかチェック
    public bool IsAttackPressed()
    {
        return _input.Player.Attack.IsPressed();
    }
}
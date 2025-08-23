using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField, Range(0, 1)] private float _moveLimit = 0.3f;
    private Camera _camera;
    private Vector3 _input;
    private void Start()
    {
        _camera = ServiceLocator.Get<CameraManager>().MainCamera;
        ServiceLocator.Get<InputManager>().OnMove += Movement;
    }
    private void Update()
    {
        transform.position += _input * _moveSpeed * Time.deltaTime;

        // 元の z を保持
        float originalZ = transform.position.z;

        // ビューポート変換
        Vector3 viewPos = _camera.WorldToViewportPoint(transform.position);

        // 画面内にClamp
        viewPos.x = Mathf.Clamp(viewPos.x, _moveLimit, 1f - _moveLimit);
        viewPos.y = Mathf.Clamp(viewPos.y, _moveLimit, 1f - _moveLimit);

        // ワールド座標に戻す
        Vector3 worldPos = _camera.ViewportToWorldPoint(viewPos);

        // z を元に戻す（前後のズレ防止）
        worldPos.z = originalZ;

        transform.position = worldPos;
    }

    private void Movement(Vector2 moveInput)
    {
        _input = moveInput.normalized;
    }
}

// プレイヤーの体力管理インターフェース（参考）
public interface IPlayerHealth
{
    void TakeDamage(float damage);
    void Heal(float amount);
    float Health { get; }
    float MaxHealth { get; }
    bool IsAlive { get; }
}
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

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
    }

    private void Movement(Vector2 moveInput)
    {
        _input = moveInput;
    }
}

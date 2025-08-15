using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [HideInInspector] public Camera MainCamera;
    private void Awake()
    {
        ServiceLocator.Set(this);
        MainCamera = Camera.main;
    }
}

using UnityEngine;

public class DebugPlayer : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;

    [ContextMenu("セーブ")]
    private void Save()
    {
        SaveLoadService.Save(_playerData);
    }
}

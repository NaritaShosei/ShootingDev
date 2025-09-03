using UnityEngine;

public class WeaponSelector : MonoBehaviour
{
    private PlayerData _playerData;

    private void Awake()
    {
        ServiceLocator.Set(this);
        _playerData = SaveLoadService.Load<PlayerData>();
    }

    public void SelectWeapon(int id)
    {
        _playerData.EquipWeapon(id);

        SaveLoadService.Save(_playerData);
    }
}

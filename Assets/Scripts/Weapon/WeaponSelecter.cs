using UnityEngine;

public class WeaponSelector : MonoBehaviour
{
    private PlayerData _playerData = new PlayerData();

    private void Awake()
    {
        ServiceLocator.Set(this);
    }

    public void SelectWeapon(int id)
    {
        _playerData.EquipWeapon(id);

        SaveLoadService.Save(_playerData);
    }
}

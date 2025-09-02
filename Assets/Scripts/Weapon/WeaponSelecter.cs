using UnityEngine;

public class WeaponSelector : MonoBehaviour
{
    private PlayerData _playerData = new PlayerData();

    public void SelectWeapon(int id)
    {
        _playerData.EquipWeapon(id);
    }
}

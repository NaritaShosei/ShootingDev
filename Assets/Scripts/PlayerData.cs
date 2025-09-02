using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField] private PlayerLoadout _currentLoadout = new PlayerLoadout();
    [SerializeField] private List<int> _unlockedWeaponIDs = new List<int>();

    public PlayerLoadout CurrentLoadout => _currentLoadout;
    public List<int> UnlockedWeaponIds => _unlockedWeaponIDs;

    // 武器のアンロック
    public void UnlockWeapon(int weaponId)
    {
        if (!_unlockedWeaponIDs.Contains(weaponId))
        {
            _unlockedWeaponIDs.Add(weaponId);
        }
    }

    // 装備変更
    public bool EquipWeapon(int weaponId)
    {
        // if (!_unlockedWeaponIDs.Contains(weaponId)) return false;

        _currentLoadout.PrimaryWeaponId = weaponId;
        return true;
    }
}


[System.Serializable]
public class PlayerLoadout
{
    [SerializeField] private int _primaryWeaponId;      // メイン武器ID

    public int PrimaryWeaponId
    {
        get => _primaryWeaponId;
        set => _primaryWeaponId = value;
    }
}
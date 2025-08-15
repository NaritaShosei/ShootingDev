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
    public bool EquipWeapon(int weaponId, WeaponSlot slot)
    {
        if (!_unlockedWeaponIDs.Contains(weaponId)) return false;

        switch (slot)
        {
            case WeaponSlot.Primary:
                _currentLoadout.PrimaryWeaponId = weaponId;
                break;
            case WeaponSlot.Secondary:
                _currentLoadout.SecondaryWeaponId = weaponId;
                break;
            case WeaponSlot.Special:
                _currentLoadout.SpecialWeaponId = weaponId;
                break;
        }
        return true;
    }
}

public enum WeaponSlot
{
    Primary,
    Secondary,
    Special
}

[System.Serializable]
public class PlayerLoadout
{
    [SerializeField] private int _primaryWeaponId;      // メイン武器ID
    [SerializeField] private int _secondaryWeaponId;    // サブ武器ID
    [SerializeField] private int _specialWeaponId;      // 特殊武器ID

    public int PrimaryWeaponId
    {
        get => _primaryWeaponId;
        set => _primaryWeaponId = value;
    }
    public int SecondaryWeaponId
    {
        get => _secondaryWeaponId;
        set => _secondaryWeaponId = value;
    }
    public int SpecialWeaponId
    {
        get => _specialWeaponId;
        set => _specialWeaponId = value;
    }
}
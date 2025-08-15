using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "WeaponDatabase", fileName = "WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    [SerializeField] private WeaponData[] _weapons;
    private Dictionary<int, WeaponData> _weaponDict;

    private void OnEnable()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        _weaponDict = new Dictionary<int, WeaponData>();
        foreach (var weapon in _weapons)
        {
            if (weapon != null)
            {
                _weaponDict[weapon.WeaponID] = weapon;
            }
        }
    }

    public WeaponData GetWeapon(int weaponId)
    {
        if (_weaponDict == null) InitializeDictionary();
        return _weaponDict.TryGetValue(weaponId, out var weapon) ? weapon : null;
    }

    public WeaponData[] GetAllWeapons() => _weapons;
}

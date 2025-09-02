using UnityEngine;

public class WeaponSelectView : MonoBehaviour
{
    [SerializeField] private WeaponDatabase _weaponDatabase;
    [SerializeField] private WeaponCell _weaponCell;
    [SerializeField] private Transform _cellParent;

    private void Start()
    {
        SetUI();
    }

    private void SetUI()
    {
        foreach (var data in _weaponDatabase.GetAllWeapons())
        {
            var cell = Instantiate(_weaponCell, _cellParent);
            cell.Initialize(data.WeaponIcon, data.WeaponName, data.AttackPower, data.AttackRate);
        }
    }
}

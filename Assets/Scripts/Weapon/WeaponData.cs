using UnityEngine;

[CreateAssetMenu(menuName = "WeaponData", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private WeaponBase _weapon;
    [SerializeField] private float _attackPower = 1;
    [SerializeField] private float _attackRate = 1;
}

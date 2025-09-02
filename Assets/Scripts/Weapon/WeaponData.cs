using UnityEngine;

[CreateAssetMenu(menuName = "WeaponData", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private int _weaponID;
    [SerializeField] private string _weaponName;
    [SerializeField] private Sprite _weaponIcon;

    [Header("性能")]
    [SerializeField] private float _attackPower = 1;
    [SerializeField] private float _attackRate = 1;
    [SerializeField] private float _range = 10;

    [Header("プレハブ")]
    [SerializeField] private GameObject _weaponPrefab;
    public int WeaponID => _weaponID;
    public string WeaponName => _weaponName;
    public Sprite WeaponIcon => _weaponIcon;
    public float AttackPower => _attackPower;
    public float AttackRate => _attackRate;
    public float Range => _range;
    public GameObject WeaponPrefab => _weaponPrefab;

}

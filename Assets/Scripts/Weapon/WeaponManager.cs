using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponDatabase _weaponDatabase;
    [SerializeField] private Transform _weaponParent;
    private PlayerData _playerData;
    private WeaponBase _currentWeapon;
    public WeaponBase CurrentWeapon => _currentWeapon;

    private void Start()
    {
        ServiceLocator.Set(this);

        // プレイヤーデータを取得（セーブ管理クラスから）
        _playerData = SaveLoadService.Load<PlayerData>();

        // 装備中の武器を生成
        SpawnEquippedWeapons();
    }

    [ContextMenu("再度取得")]
    private void Load()
    {
        _playerData = SaveLoadService.Load<PlayerData>();
    }

    [ContextMenu("武器の反映")]
    private void SpawnEquippedWeapons()
    {
        var loadout = _playerData.CurrentLoadout;

        // メイン武器生成
        if (loadout.PrimaryWeaponId > 0)
        {
            SpawnWeapon(loadout.PrimaryWeaponId);
        }
    }

    private void SpawnWeapon(int weaponId)
    {
        var weaponData = _weaponDatabase.GetWeapon(weaponId);
        if (weaponData?.WeaponPrefab == null) { Debug.LogWarning("プレハブが設定されていません"); return; }

        var weaponObj = Instantiate(weaponData.WeaponPrefab, _weaponParent);
        var weaponComponent = weaponObj.GetComponent<WeaponBase>();

        if (!weaponComponent) { Debug.LogWarning("プレハブにWeaponBaseがアタッチされていません"); return; }

        weaponComponent.Initialize(weaponData);
        _currentWeapon = weaponComponent;

    }

}

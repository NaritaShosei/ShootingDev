using UnityEngine;
using System.Collections;

public class LongRangeAttackWeapon : WeaponBase
{
    [Header("弾丸設定")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _bulletSpeed = 10f;

    [Header("エフェクト")]
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private AudioClip _fireSound;

    private float _lastFireTime;
    private AudioSource _audioSource;

    protected override void OnInitialize()
    {
        // AudioSourceコンポーネントを取得または追加
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // FirePointが設定されていない場合は自身のTransformを使用
        if (_firePoint == null)
        {
            _firePoint = transform;
        }

        Debug.Log($"遠距離武器 {_data.WeaponName} を初期化しました");
        Debug.Log($"攻撃力: {_data.AttackPower}, 攻撃速度: {_data.AttackRate}, 射程: {_data.Range}");
    }

    public override void Attack()
    {
        // 攻撃レートによるクールダウンチェック
        if (!CanFire())
        {
            return;
        }

        FireBullet();
        PlayEffects();

        _lastFireTime = Time.time;
    }

    private bool CanFire()
    {
        // AttackRateが高いほど連射速度が速くなる
        float fireInterval = 1f / _data.AttackRate;
        return Time.time >= _lastFireTime + fireInterval;
    }

    private void FireBullet()
    {
        if (_bulletPrefab == null)
        {
            Debug.LogWarning($"{_data.WeaponName}: 弾丸プレハブが設定されていません");
            return;
        }

        // 弾丸を生成
        GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);

        // 弾丸に武器データを渡す
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Initialize(_data.AttackPower, _bulletSpeed, _data.Range);
        }
        else
        {
            // Bulletコンポーネントがない場合は、Rigidbodyで物理的に飛ばす
            var rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = _firePoint.forward * _bulletSpeed;
            }

            // 射程に基づいて弾丸を削除
            Destroy(bullet, _data.Range / _bulletSpeed);
        }

        Debug.Log($"{_data.WeaponName} が攻撃力 {_data.AttackPower} で発砲");
    }

    private void PlayEffects()
    {
        // マズルフラッシュエフェクト
        if (_muzzleFlash != null)
        {
            _muzzleFlash.Play();
        }

        // 発砲音
        if (_fireSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_fireSound);
        }
    }

    // デバッグ用: 武器の有効射程を可視化
    private void OnDrawGizmosSelected()
    {
        if (_data != null && _firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position, _data.Range);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(_firePoint.position, _firePoint.forward * _data.Range);
        }
    }
}
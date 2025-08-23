using UnityEngine;


// 照準機能付きの遠距離攻撃武器
public class LongRangeAttackWeapon : WeaponBase
{
    [Header("弾丸設定")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _bulletSpeed = 10f;

    [Header("照準設定")]
    [SerializeField] private Transform _weaponModel; // 武器のモデル部分
    [SerializeField] private bool _rotateWeapon = true; // 武器を回転させるか
    [SerializeField] private float _rotationSpeed = 10f; // 回転速度

    [Header("エフェクト")]
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private AudioClip _fireSound;

    private float _lastFireTime;
    private AudioSource _audioSource;
    private CrosshairManager _crosshairManager;

    protected override void OnInitialize()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (_firePoint == null)
        {
            _firePoint = transform;
        }

        if (_weaponModel == null)
        {
            _weaponModel = transform;
        }

        // クロスヘアマネージャーを取得
        _crosshairManager = ServiceLocator.Get<CrosshairManager>();

        Debug.Log($"照準機能付き遠距離武器 {_data.WeaponName} を初期化しました");
    }

    private void Update()
    {
        if (_rotateWeapon && _crosshairManager != null)
        {
            AimAtTarget();
        }
    }

    private void AimAtTarget()
    {
        // クロスヘアの方向を向く
        Vector3 aimDirection = _crosshairManager.AimDirection;

        aimDirection.Normalize();

        if (aimDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            _weaponModel.rotation = Quaternion.Slerp(_weaponModel.rotation, targetRotation,
                                                   _rotationSpeed * Time.deltaTime);
        }
    }

    public override void Attack()
    {
        if (!CanFire())
        {
            return;
        }

        FireBulletAtTarget();
        PlayEffects();

        _lastFireTime = Time.time;
    }

    private bool CanFire()
    {
        float fireInterval = 1f / _data.AttackRate;
        return Time.time >= _lastFireTime + fireInterval;
    }

    private void FireBulletAtTarget()
    {
        if (_bulletPrefab == null || _crosshairManager == null)
        {
            Debug.LogWarning("弾丸プレハブまたはクロスヘアマネージャーが設定されていません");
            return;
        }

        // 敵を照準中の場合は敵の位置を、そうでなければ通常の照準位置を使用
        Vector3 targetPosition = _crosshairManager.IsTargetingEnemy
            ? _crosshairManager.GetEnemyTargetPosition()
            : _crosshairManager.AimPosition;

        Vector3 fireDirection = (targetPosition - _firePoint.position).normalized;

        // 弾丸を生成（照準方向を向ける）
        Quaternion bulletRotation = Quaternion.LookRotation(fireDirection);
        GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, bulletRotation);

        // Bulletタグを設定（CrosshairManagerで無視されるように）
        bullet.tag = "Bullet";

        // 弾丸に武器データと方向を渡す
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Initialize(_data.AttackPower, _bulletSpeed, _data.Range,
                                     targetPosition, _crosshairManager.CurrentTarget);
        }
        else
        {
            // 通常のRigidbodyによる移動
            var rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = fireDirection * _bulletSpeed;
            }

            Destroy(bullet, _data.Range / _bulletSpeed);
        }

        string targetInfo = _crosshairManager.IsTargetingEnemy ? "敵" : "地面";
        Debug.Log($"{_data.WeaponName} が{targetInfo}に向けて発砲");
    }

    private void PlayEffects()
    {
        if (_muzzleFlash != null)
        {
            _muzzleFlash.Play();
        }

        if (_fireSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_fireSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_data != null && _firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position, _data.Range);

            // 照準方向を表示
            if (_crosshairManager != null && Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Vector3 aimDirection = (_crosshairManager.AimPosition - _firePoint.position).normalized;
                Gizmos.DrawRay(_firePoint.position, aimDirection * _data.Range);
            }
        }
    }
}


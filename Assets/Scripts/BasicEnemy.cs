// Z軸移動＋プレイヤー攻撃の基本敵クラス
using UnityEngine;
using System.Collections;

public class BasicEnemy : MonoBehaviour, IEnemy
{
    [Header("敵の基本設定")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _destroyZPosition = -10f; // この位置で削除

    [Header("攻撃設定")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _attackInterval = 2f;
    [SerializeField] private float _bulletSpeed = 8f;
    [SerializeField] private float _attackRange = 15f;
    [SerializeField] private float _attackDamage = 10f;

    [Header("画面判定設定")]
    [SerializeField] private float _screenMargin = 0.1f; // 画面端からのマージン（0.0〜1.0）

    [Header("視覚効果")]
    [SerializeField] private GameObject _deathEffect;
    [SerializeField] private AudioClip _attackSound;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private Color _damageColor = Color.red;

    private float _currentHealth;
    private Transform _player;
    private AudioSource _audioSource;
    private Renderer _renderer;
    private Color _originalColor;
    private float _lastAttackTime;
    private bool _isAlive = true;
    private Camera _mainCamera;

    // IEnemy実装
    public float Health => _currentHealth;
    public bool IsAlive => _isAlive && _currentHealth > 0;

    private void Start()
    {
        InitializeEnemy();
        FindPlayer();
        FindMainCamera();
        StartCoroutine(AttackCoroutine());
    }

    private void InitializeEnemy()
    {
        _currentHealth = _maxHealth;

        // コンポーネント取得
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }

        // FirePointが設定されていない場合は自身を使用
        if (_firePoint == null)
        {
            _firePoint = transform;
        }

        // Enemyタグを確実に設定
        gameObject.tag = "Enemy";

        Debug.Log($"敵 {gameObject.name} を初期化しました（HP: {_maxHealth}）");
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Playerタグのオブジェクトが見つかりません");
        }
    }

    private void FindMainCamera()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            _mainCamera = FindFirstObjectByType<Camera>();
        }

        if (_mainCamera == null)
        {
            Debug.LogWarning("メインカメラが見つかりません。画面判定ができません。");
        }
    }

    private void Update()
    {
        if (!IsAlive) return;

        MoveTowardsPlayer();
        CheckBounds();
    }

    private void MoveTowardsPlayer()
    {
        // Z軸方向（奥から手前）への移動
        Vector3 moveDirection = Vector3.forward; // または -Vector3.back
        transform.position += moveDirection * _moveSpeed * Time.deltaTime;
    }

    private void CheckBounds()
    {
        // 画面外（手前側）に出たら削除
        if (transform.position.z <= _destroyZPosition)
        {
            Debug.Log($"敵 {gameObject.name} が画面外に出ました");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// オブジェクトが画面内に表示されているかどうかをチェック
    /// </summary>
    /// <returns>画面内に表示されている場合はtrue</returns>
    public bool IsVisibleOnScreen()
    {
        if (_mainCamera == null) return true; // カメラがない場合は常に攻撃可能とする

        // ワールド座標をビューポート座標に変換
        Vector3 viewportPosition = _mainCamera.WorldToViewportPoint(transform.position);

        // ビューポート座標は (0,0) が左下、(1,1) が右上
        // マージンを考慮して画面内判定
        bool isInViewport = viewportPosition.x >= -_screenMargin &&
                           viewportPosition.x <= 1f + _screenMargin &&
                           viewportPosition.y >= -_screenMargin &&
                           viewportPosition.y <= 1f + _screenMargin &&
                           viewportPosition.z > 0; // カメラの前方にある

        return isInViewport;
    }

    private IEnumerator AttackCoroutine()
    {
        while (IsAlive)
        {
            yield return new WaitForSeconds(_attackInterval);

            if (IsAlive && CanAttackPlayer())
            {
                AttackPlayer();
            }
        }
    }

    private bool CanAttackPlayer()
    {
        if (_player == null) return false;

        // 画面内にいるかどうかをチェック
        if (!IsVisibleOnScreen())
        {
            return false;
        }

        // プレイヤーとの距離をチェック
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        return distanceToPlayer <= _attackRange;
    }

    private void AttackPlayer()
    {
        if (_bulletPrefab == null || _player == null) return;

        // プレイヤーへの方向を計算
        Vector3 directionToPlayer = (_player.position - _firePoint.position).normalized;

        // 弾丸を生成
        GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position,
                                      Quaternion.LookRotation(directionToPlayer));

        // 敵の弾丸タグを設定
        bullet.tag = "EnemyBullet";

        // 弾丸に設定を適用
        var enemyBullet = bullet.GetComponent<EnemyBullet>();
        if (enemyBullet != null)
        {
            enemyBullet.Initialize(_attackDamage, _bulletSpeed, _attackRange, directionToPlayer);
        }
        else
        {
            // 基本的な物理移動
            var rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = directionToPlayer * _bulletSpeed;
            }

            // 一定時間後に削除
            Destroy(bullet, _attackRange / _bulletSpeed + 2f);
        }

        // 攻撃エフェクト
        PlayAttackEffects();

        Debug.Log($"敵 {gameObject.name} がプレイヤーを攻撃しました（画面内攻撃）");
    }

    private void PlayAttackEffects()
    {
        if (_attackSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_attackSound);
        }
    }

    // IEnemy実装：ダメージを受ける
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);

        Debug.Log($"敵 {gameObject.name} が {damage} ダメージを受けました（残りHP: {_currentHealth}）");

        // ダメージエフェクト
        StartCoroutine(DamageFlash());

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlash()
    {
        if (_renderer != null)
        {
            _renderer.material.color = _damageColor;
            yield return new WaitForSeconds(0.1f);
            _renderer.material.color = _originalColor;
        }
    }

    // IEnemy実装：死亡処理
    public void Die()
    {
        if (!_isAlive) return;
        _isAlive = false;

        Debug.Log($"敵 {gameObject.name} が倒されました");

        // 死亡エフェクト
        if (_deathEffect != null)
        {
            Instantiate(_deathEffect, transform.position, transform.rotation);
        }

        if (_deathSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_deathSound);
        }

        // オブジェクトを削除（音が鳴り終わるまで少し待つ）
        Destroy(gameObject, _deathSound != null ? _deathSound.length : 0.1f);
    }

    // デバッグ用：攻撃範囲を可視化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (_player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _player.position);
        }

        // 画面判定の可視化
        if (_mainCamera != null)
        {
            Vector3 viewportPos = _mainCamera.WorldToViewportPoint(transform.position);
            bool isVisible = IsVisibleOnScreen();

            Gizmos.color = isVisible ? Color.green : Color.gray;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}
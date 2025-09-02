// 照準機能付き弾丸クラス
using UnityEngine;
using System.Collections;
public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private float _range;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Vector3 _direction;
    private GameObject _targetEnemy; // 追尾対象の敵

    public void Initialize(float damage, float speed, float range, Vector3 targetPosition, GameObject targetEnemy = null)
    {
        _damage = damage;
        _speed = speed;
        _range = range;
        _startPosition = transform.position;
        _targetPosition = targetPosition;
        _targetEnemy = targetEnemy;
        _direction = (_targetPosition - _startPosition).normalized;

        // Bulletタグを設定
        gameObject.tag = "Bullet";

        StartCoroutine(MoveBullet());
    }

    private IEnumerator MoveBullet()
    {
        while (Vector3.Distance(_startPosition, transform.position) < _range)
        {
            // 敵を追尾する場合は動的にターゲット位置を更新
            if (_targetEnemy != null)
            {
                var collider = _targetEnemy.GetComponent<Collider>();
                Vector3 currentTargetPos = collider != null ? collider.bounds.center : _targetEnemy.transform.position;
                _direction = (currentTargetPos - transform.position).normalized;
            }

            // ターゲット方向への移動
            transform.position += _direction * _speed * Time.deltaTime;

            // ターゲットに到達した場合も削除
            float distanceToTarget = _targetEnemy != null
                ? Vector3.Distance(transform.position, _targetEnemy.transform.position)
                : Vector3.Distance(transform.position, _targetPosition);

            if (distanceToTarget < 0.5f)
            {
                break;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 敵にヒット
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<IEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }

            Debug.Log($"敵 {other.name} に {_damage} ダメージを与えました");
            Destroy(gameObject);
            return;
        }


        // プレイヤー、武器、他の弾丸は無視
        if (other.CompareTag("Player") || other.CompareTag("Weapon") || other.CompareTag("Bullet"))
        {
            return; // 衝突を無視
        }
    }
}
public interface IEnemy
{
    void TakeDamage(float damage);
}
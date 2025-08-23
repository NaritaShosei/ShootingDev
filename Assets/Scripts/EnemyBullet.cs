// 敵の弾丸クラス
using UnityEngine;
using System.Collections;

public class EnemyBullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private float _range;
    private Vector3 _direction;
    private Vector3 _startPosition;

    public void Initialize(float damage, float speed, float range, Vector3 direction)
    {
        _damage = damage;
        _speed = speed;
        _range = range;
        _direction = direction.normalized;
        _startPosition = transform.position;

        // EnemyBulletタグを確実に設定
        gameObject.tag = "EnemyBullet";

        StartCoroutine(MoveBullet());
    }

    private IEnumerator MoveBullet()
    {
        while (Vector3.Distance(_startPosition, transform.position) < _range)
        {
            transform.position += _direction * _speed * Time.deltaTime;
            yield return null;
        }

        // 射程に達したら削除
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーにヒット
        if (other.TryGetComponent(out IPlayerHealth playerHealth))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage);
                Debug.Log($"プレイヤーに {_damage} ダメージを与えました");
            }

            Destroy(gameObject);
            return;
        }

        // 壁や障害物にヒット
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        // 敵や敵の弾丸は無視
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
        {
            return;
        }
    }
}
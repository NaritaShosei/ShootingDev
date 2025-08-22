using UnityEngine;
using System.Collections;

[System.Serializable]
public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private float _range;
    private Vector3 _startPosition;

    public void Initialize(float damage, float speed, float range)
    {
        _damage = damage;
        _speed = speed;
        _range = range;
        _startPosition = transform.position;

        // 移動開始
        StartCoroutine(MoveBullet());
    }

    private IEnumerator MoveBullet()
    {
        while (Vector3.Distance(_startPosition, transform.position) < _range)
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
            yield return null;
        }

        // 射程に達したら削除
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 敵との衝突判定
        if (other.TryGetComponent(out IEnemy enemy))
        {
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }

            Debug.Log($"敵に {_damage} ダメージを与えました");
            Destroy(gameObject);
        }

        // 障害物との衝突
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
public interface IEnemy
{
    void TakeDamage(float damage);
}
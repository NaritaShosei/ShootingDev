// 敵スポナークラス
using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private int _maxEnemies = 10;
    [SerializeField] private float _spawnZPosition = 20f; // 奥側のスポーン位置

    [Header("スポーンエリア設定 (4つのコーナー)")]
    [SerializeField] private Transform _topLeftCorner;
    [SerializeField] private Transform _topRightCorner;
    [SerializeField] private Transform _bottomLeftCorner;
    [SerializeField] private Transform _bottomRightCorner;

    [Header("デフォルトエリア設定")]
    [SerializeField] private Vector2 _defaultAreaSize = new Vector2(15f, 8f); // 横幅 x 縦幅
    [SerializeField] private Vector3 _defaultAreaCenter = new Vector3(0f, 3f, 20f);

    private int _currentEnemyCount;
    private Vector3[] _cornerPositions = new Vector3[4];

    private void Start()
    {
        SetupSpawnArea();
        StartCoroutine(SpawnCoroutine());
    }

    private void SetupSpawnArea()
    {
        // 4つのコーナーが設定されている場合
        if (_topLeftCorner != null && _topRightCorner != null &&
            _bottomLeftCorner != null && _bottomRightCorner != null)
        {
            _cornerPositions[0] = _topLeftCorner.position;      // 左上
            _cornerPositions[1] = _topRightCorner.position;     // 右上
            _cornerPositions[2] = _bottomLeftCorner.position;   // 左下
            _cornerPositions[3] = _bottomRightCorner.position;  // 右下

            Debug.Log("4つのコーナーポジションを使用してスポーンエリアを設定しました");
        }
        else
        {
            // デフォルトの4つのコーナーを作成
            CreateDefaultSpawnArea();
            Debug.Log("デフォルトスポーンエリアを作成しました");
        }

        LogSpawnArea();
    }

    private void CreateDefaultSpawnArea()
    {
        Vector3 center = _defaultAreaCenter;
        float halfWidth = _defaultAreaSize.x * 0.5f;
        float halfHeight = _defaultAreaSize.y * 0.5f;

        // 4つのコーナー座標を計算
        _cornerPositions[0] = new Vector3(center.x - halfWidth, center.y + halfHeight, _spawnZPosition); // 左上
        _cornerPositions[1] = new Vector3(center.x + halfWidth, center.y + halfHeight, _spawnZPosition); // 右上
        _cornerPositions[2] = new Vector3(center.x - halfWidth, center.y - halfHeight, _spawnZPosition); // 左下
        _cornerPositions[3] = new Vector3(center.x + halfWidth, center.y - halfHeight, _spawnZPosition); // 右下

        // デバッグ用に空のGameObjectを作成（オプション）
        CreateDebugCorners();
    }

    private void CreateDebugCorners()
    {
        GameObject cornerParent = new GameObject("SpawnAreaCorners");
        cornerParent.transform.SetParent(transform);

        string[] cornerNames = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };

        for (int i = 0; i < 4; i++)
        {
            GameObject corner = new GameObject($"Corner_{cornerNames[i]}");
            corner.transform.SetParent(cornerParent.transform);
            corner.transform.position = _cornerPositions[i];
        }
    }

    private void LogSpawnArea()
    {
        Debug.Log("スポーンエリア座標:");
        Debug.Log($"左上: {_cornerPositions[0]}");
        Debug.Log($"右上: {_cornerPositions[1]}");
        Debug.Log($"左下: {_cornerPositions[2]}");
        Debug.Log($"右下: {_cornerPositions[3]}");
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);

            if (_currentEnemyCount < _maxEnemies && _enemyPrefabs.Length > 0)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        // ランダムな敵を選択
        GameObject enemyPrefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];

        // 4つのコーナー内のランダムな位置を生成
        Vector3 randomSpawnPosition = GetRandomPositionInArea();

        GameObject enemy = Instantiate(enemyPrefab, randomSpawnPosition, Quaternion.identity);

        // 敵が削除された時にカウントを減らす
        var enemyComponent = enemy.GetComponent<BasicEnemy>();
        if (enemyComponent != null)
        {
            _currentEnemyCount++;
            StartCoroutine(WaitForEnemyDestruction(enemy));
        }

        Debug.Log($"敵を生成: {enemy.name} at {randomSpawnPosition}");
    }

    private Vector3 GetRandomPositionInArea()
    {
        // 4つのコーナーで囲まれた矩形エリア内のランダムな位置を生成

        // X座標の範囲を計算（左上と右上、または左下と右下から）
        float minX = Mathf.Min(_cornerPositions[0].x, _cornerPositions[2].x); // 左上と左下の最小X
        float maxX = Mathf.Max(_cornerPositions[1].x, _cornerPositions[3].x); // 右上と右下の最大X

        // Y座標の範囲を計算（左上と右上、または左下と右下から）
        float minY = Mathf.Min(_cornerPositions[2].y, _cornerPositions[3].y); // 左下と右下の最小Y
        float maxY = Mathf.Max(_cornerPositions[0].y, _cornerPositions[1].y); // 左上と右上の最大Y

        // Z座標は固定（奥側）
        float spawnZ = _cornerPositions[0].z;

        // ランダムな座標を生成
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector3(randomX, randomY, spawnZ);
    }

    private IEnumerator WaitForEnemyDestruction(GameObject enemy)
    {
        while (enemy != null)
        {
            yield return null;
        }

        _currentEnemyCount--;
    }

    // デバッグ用：スポーンエリアを手動で再設定
    [ContextMenu("スポーンエリアを再設定")]
    private void RefreshSpawnArea()
    {
        SetupSpawnArea();
    }

    // デバッグ用：テスト敵を生成
    [ContextMenu("テスト敵を生成")]
    private void SpawnTestEnemy()
    {
        if (_enemyPrefabs.Length > 0)
        {
            SpawnEnemy();
        }
        else
        {
            Debug.LogWarning("敵プレハブが設定されていません");
        }
    }

    private void OnDrawGizmos()
    {
        // スポーンエリアを可視化
        if (_cornerPositions != null && _cornerPositions.Length == 4)
        {
            // エリアの境界線を描画
            Gizmos.color = Color.green;

            // 矩形の4辺を描画
            Gizmos.DrawLine(_cornerPositions[0], _cornerPositions[1]); // 上辺
            Gizmos.DrawLine(_cornerPositions[1], _cornerPositions[3]); // 右辺
            Gizmos.DrawLine(_cornerPositions[3], _cornerPositions[2]); // 下辺
            Gizmos.DrawLine(_cornerPositions[2], _cornerPositions[0]); // 左辺

            // コーナーポイントを強調
            Gizmos.color = Color.red;
            foreach (var corner in _cornerPositions)
            {
                Gizmos.DrawWireSphere(corner, 0.5f);
            }

            // エリア全体を半透明で表示
            Gizmos.color = new Color(0, 1, 0, 0.2f);

            // 矩形エリアの中心と大きさを計算
            Vector3 center = (_cornerPositions[0] + _cornerPositions[1] + _cornerPositions[2] + _cornerPositions[3]) / 4f;

            float width = Mathf.Abs(_cornerPositions[1].x - _cornerPositions[0].x);
            float height = Mathf.Abs(_cornerPositions[0].y - _cornerPositions[2].y);

            Gizmos.DrawCube(center, new Vector3(width, height, 0.1f));
        }

        // デフォルト設定の場合のプレビュー
        if (Application.isPlaying == false && _cornerPositions.Length == 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = _defaultAreaCenter;
            Gizmos.DrawWireCube(center, new Vector3(_defaultAreaSize.x, _defaultAreaSize.y, 1f));
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

// Canvas UI上の敵専用照準クロスヘアシステム（ロックオン機能付き）
public class CrosshairManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private Image _crosshairImage;
    [SerializeField] private Sprite _normalCrosshair;
    [SerializeField] private Sprite _enemyCrosshair;
    [SerializeField] private Sprite _lockedOnCrosshair; // ロックオン時専用
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _enemyColor = Color.red;
    [SerializeField] private Color _lockedOnColor = Color.yellow; // ロックオン時の色

    [Header("サイズ設定")]
    [SerializeField, Range(16, 256)] private float _normalSize = 32f;
    [SerializeField, Range(16, 256)] private float _enemySize = 40f;
    [SerializeField, Range(16, 256)] private float _lockedOnSize = 48f; // ロックオン時のサイズ
    [SerializeField, Range(0.5f, 2f)] private float _pulseIntensity = 0.1f;
    [SerializeField, Range(1f, 20f)] private float _pulseSpeed = 8f;
    [SerializeField, Range(1f, 10f)] private float _sizeChangeSpeed = 8f;

    [Header("ロックオン設定")]
    [SerializeField, Range(50f, 200f)] private float _lockOnRange = 80f; // ピクセル単位でのロックオン範囲
    [SerializeField, Range(0.1f, 2f)] private float _lockOnTime = 0.5f; // ロックオンに必要な時間
    [SerializeField, Range(0.1f, 5f)] private float _lockOnLoseTime = 1f; // ロックオン解除時間
    [SerializeField] private bool _enableAutoLockOn = true; // 自動ロックオン有効/無効
    [SerializeField] private bool _snapToLockedTarget = true; // ロック時にカーソルを敵に吸着
    [SerializeField, Range(1f, 10f)] private float _snapSpeed = 5f; // 吸着速度

    [Header("照準設定")]
    [SerializeField] private LayerMask _groundLayer = 1;
    [SerializeField] private float _maxDistance = 50f;

    [Header("無視するタグ")]
    [SerializeField] private string[] _ignoreTags = { "Player", "Weapon", "Bullet" };
    [SerializeField] private string _enemyTag = "Enemy";

    [Header("ロックオンエフェクト")]
    [SerializeField] private AudioClip _lockOnSound;
    [SerializeField] private AudioClip _lockOffSound;

    private Camera _camera;
    private Vector3 _aimPosition;
    private GameObject _currentTarget;
    private GameObject _lockedOnTarget; // ロックオン中の敵
    private bool _isTargetingEnemy;
    private bool _isLockedOn;
    private float _lockOnTimer;
    private float _lockOffTimer;
    private Vector3 _lastMousePosition;
    private AudioSource _audioSource;

    // ロックオン候補の敵リスト
    private List<GameObject> _nearbyEnemies = new List<GameObject>();

    public Vector3 AimPosition => _aimPosition;
    public Vector3 AimDirection { get; private set; }
    public GameObject CurrentTarget => _isLockedOn ? _lockedOnTarget : _currentTarget;
    public bool IsTargetingEnemy => _isTargetingEnemy;
    public bool IsLockedOn => _isLockedOn;
    public GameObject LockedOnTarget => _lockedOnTarget;

    private void Awake()
    {
        ServiceLocator.Set(this);
    }

    private bool ShouldIgnoreTag(string tag)
    {
        foreach (string ignoreTag in _ignoreTags)
        {
            if (tag == ignoreTag)
            {
                return true;
            }
        }
        return false;
    }

    private void Start()
    {
        _camera = ServiceLocator.Get<CameraManager>().MainCamera;

        // オーディオソース設定
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // マウスカーソルを非表示
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        // UIクロスヘアのセットアップ
        SetupUICrosshair();

        Debug.Log("Canvas UI クロスヘアシステム（ロックオン機能付き）を初期化しました");
    }

    private void SetupUICrosshair()
    {
        // キャンバスが設定されていない場合は自動作成
        if (_uiCanvas == null)
        {
            _uiCanvas = FindOrCreateCanvas();
        }

        // クロスヘアスプライトが設定されていない場合は自動生成
        if (_normalCrosshair == null)
        {
            int textureSize = Mathf.RoundToInt(_normalSize);
            _normalCrosshair = CrosshairCreator.CreateDotCrosshair(textureSize, 2, 4, Color.white);
        }

        if (_enemyCrosshair == null)
        {
            int textureSize = Mathf.RoundToInt(_enemySize);
            _enemyCrosshair = CrosshairCreator.CreateCircleCrosshair(textureSize, textureSize / 3, 2, Color.red);
        }

        if (_lockedOnCrosshair == null)
        {
            int textureSize = Mathf.RoundToInt(_lockedOnSize);
            _lockedOnCrosshair = CrosshairCreator.CreateLockOnCrosshair(textureSize, textureSize / 2 - 4, 3, Color.yellow);
        }

        // クロスヘアImageが設定されていない場合は自動作成
        if (_crosshairImage == null)
        {
            _crosshairImage = CreateCrosshairImage();
        }

        // 初期設定
        _crosshairImage.sprite = _normalCrosshair;
        _crosshairImage.color = _normalColor;
    }

    private Canvas FindOrCreateCanvas()
    {
        // 既存のCanvasを探す
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return canvas;
        }

        // 新しいCanvasを作成
        GameObject canvasObj = new GameObject("UI Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 最前面に表示

        // CanvasScalerを追加
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // GraphicRaycasterを追加
        canvasObj.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private Image CreateCrosshairImage()
    {
        // クロスヘア用のGameObjectを作成
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(_uiCanvas.transform, false);

        // Imageコンポーネントを追加
        Image image = crosshairObj.AddComponent<Image>();
        image.sprite = _normalCrosshair;
        image.color = _normalColor;
        image.raycastTarget = false; // UIレイキャストを無効化

        // サイズ設定
        RectTransform rectTransform = crosshairObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(_normalSize, _normalSize);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        return image;
    }

    private void Update()
    {
        UpdateNearbyEnemies();
        UpdateLockOnSystem();
        UpdateAimPosition();
        UpdateUICrosshair();
    }

    private void UpdateNearbyEnemies()
    {
        _nearbyEnemies.Clear();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(_enemyTag);
        Vector3 mousePos = Input.mousePosition;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            Vector3 enemyScreenPos = _camera.WorldToScreenPoint(enemy.transform.position);

            // 画面外の敵は除外
            if (enemyScreenPos.z <= 0) continue;

            float distance = Vector2.Distance(mousePos, enemyScreenPos);
            if (distance <= _lockOnRange)
            {
                _nearbyEnemies.Add(enemy);
            }
        }

        // 距離順でソート（近い順）
        _nearbyEnemies.Sort((a, b) =>
        {
            Vector3 aScreenPos = _camera.WorldToScreenPoint(a.transform.position);
            Vector3 bScreenPos = _camera.WorldToScreenPoint(b.transform.position);
            float aDist = Vector2.Distance(mousePos, aScreenPos);
            float bDist = Vector2.Distance(mousePos, bScreenPos);
            return aDist.CompareTo(bDist);
        });
    }

    private void UpdateLockOnSystem()
    {
        if (!_enableAutoLockOn) return;

        // 最も近い敵を取得
        GameObject closestEnemy = _nearbyEnemies.Count > 0 ? _nearbyEnemies[0] : null;

        // ロックオン処理
        if (closestEnemy != null && !_isLockedOn)
        {
            if (closestEnemy == _currentTarget)
            {
                _lockOnTimer += Time.deltaTime;
                if (_lockOnTimer >= _lockOnTime)
                {
                    StartLockOn(closestEnemy);
                }
            }
            else
            {
                _lockOnTimer = 0f;
            }
        }
        // ロックオン解除処理
        else if (_isLockedOn)
        {
            bool shouldLoseTarget = false;

            // 敵が存在しない場合
            if (_lockedOnTarget == null)
            {
                shouldLoseTarget = true;
            }
            // 敵が画面外に出た場合
            else if (!IsEnemyVisibleOnScreen(_lockedOnTarget))
            {
                shouldLoseTarget = true;
                Debug.Log($"ロックオン対象 {_lockedOnTarget.name} が画面外に出ました");
            }
            // 敵がロックオン範囲外に出た場合
            else if (!_nearbyEnemies.Contains(_lockedOnTarget))
            {
                shouldLoseTarget = true;
            }

            if (shouldLoseTarget)
            {
                _lockOffTimer += Time.deltaTime;
                if (_lockOffTimer >= _lockOnLoseTime)
                {
                    EndLockOn();
                }
            }
            else
            {
                _lockOffTimer = 0f;
            }
        }

        // 手動ロックオン解除（右クリック）
        if (Input.GetMouseButtonDown(1) && _isLockedOn)
        {
            EndLockOn();
        }
    }

    private void StartLockOn(GameObject target)
    {
        _isLockedOn = true;
        _lockedOnTarget = target;
        _lockOnTimer = 0f;
        _lockOffTimer = 0f;

        // ロックオン音再生
        if (_lockOnSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_lockOnSound);
        }

        Debug.Log($"敵 {target.name} をロックオンしました");
    }

    private void EndLockOn()
    {
        _isLockedOn = false;
        _lockedOnTarget = null;
        _lockOnTimer = 0f;
        _lockOffTimer = 0f;

        // ロックオン解除音再生
        if (_lockOffSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_lockOffSound);
        }

        Debug.Log("ロックオンを解除しました");
    }

    private void UpdateAimPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        // ロックオン中は敵の位置を追跡
        if (_isLockedOn && _lockedOnTarget != null)
        {
            Vector3 enemyScreenPos = _camera.WorldToScreenPoint(_lockedOnTarget.transform.position);

            if (_snapToLockedTarget)
            {
                // カーソルを敵位置に吸着
                mousePosition = Vector3.Lerp(mousePosition, enemyScreenPos, Time.deltaTime * _snapSpeed);
            }

            // 照準位置を敵の位置に設定
            var collider = _lockedOnTarget.GetComponent<Collider>();
            if (collider != null)
            {
                _aimPosition = collider.bounds.center;
            }
            else
            {
                _aimPosition = _lockedOnTarget.transform.position;
            }

            _currentTarget = _lockedOnTarget;
            _isTargetingEnemy = true;
        }
        else
        {
            // 通常のレイキャスト処理
            Ray ray = _camera.ScreenPointToRay(mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, _maxDistance);

            GameObject targetEnemy = null;
            RaycastHit? groundHit = null;

            // ヒットしたオブジェクトを処理
            foreach (var hit in hits)
            {
                string hitTag = hit.collider.tag;

                // 無視するタグをスキップ
                if (ShouldIgnoreTag(hitTag))
                {
                    continue;
                }

                // 敵を発見
                if (hitTag == _enemyTag)
                {
                    if (targetEnemy == null)
                    {
                        targetEnemy = hit.collider.gameObject;
                        _aimPosition = hit.point;
                    }
                    continue;
                }

                // 地面レイヤー
                if (((1 << hit.collider.gameObject.layer) & _groundLayer) != 0)
                {
                    if (groundHit == null)
                    {
                        groundHit = hit;
                    }
                }
            }

            // 照準対象の決定
            if (targetEnemy != null)
            {
                _currentTarget = targetEnemy;
                _isTargetingEnemy = true;
            }
            else if (groundHit.HasValue)
            {
                _currentTarget = null;
                _isTargetingEnemy = false;
                _aimPosition = groundHit.Value.point;
            }
            else
            {
                _currentTarget = null;
                _isTargetingEnemy = false;
                _aimPosition = ray.GetPoint(_maxDistance);
            }
        }

        // プレイヤーから照準位置への方向を計算
        Vector3 playerPosition = transform.position;
        AimDirection = (_aimPosition - playerPosition).normalized;

        _lastMousePosition = mousePosition;
    }

    private void UpdateUICrosshair()
    {
        if (_crosshairImage == null) return;

        // マウス位置にクロスヘアを配置（ロックオン時は吸着）
        Vector3 targetPos = _lastMousePosition;

        if (_isLockedOn && _lockedOnTarget != null && _snapToLockedTarget)
        {
            targetPos = _camera.WorldToScreenPoint(_lockedOnTarget.transform.position);
        }

        _crosshairImage.rectTransform.position = targetPos;

        // ロックオン状態に応じた視覚的変化
        if (_isLockedOn)
        {
            // ロックオン時
            _crosshairImage.color = _lockedOnColor;
            if (_lockedOnCrosshair != null)
            {
                _crosshairImage.sprite = _lockedOnCrosshair;
            }

            Vector2 targetSize = new Vector2(_lockedOnSize, _lockedOnSize);
            _crosshairImage.rectTransform.sizeDelta = Vector2.Lerp(
                _crosshairImage.rectTransform.sizeDelta,
                targetSize,
                Time.deltaTime * _sizeChangeSpeed
            );

            // 強い脈動効果
            float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed * 1.5f) * (_pulseIntensity * 1.5f);
            _crosshairImage.transform.localScale = Vector3.one * pulse;
        }
        else if (_isTargetingEnemy)
        {
            // 敵照準時
            _crosshairImage.color = _enemyColor;
            if (_enemyCrosshair != null)
            {
                _crosshairImage.sprite = _enemyCrosshair;
            }

            Vector2 targetSize = new Vector2(_enemySize, _enemySize);
            _crosshairImage.rectTransform.sizeDelta = Vector2.Lerp(
                _crosshairImage.rectTransform.sizeDelta,
                targetSize,
                Time.deltaTime * _sizeChangeSpeed
            );

            // 通常の脈動効果
            float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseIntensity;
            _crosshairImage.transform.localScale = Vector3.one * pulse;
        }
        else
        {
            // 通常時
            _crosshairImage.color = _normalColor;
            _crosshairImage.sprite = _normalCrosshair;

            Vector2 targetSize = new Vector2(_normalSize, _normalSize);
            _crosshairImage.rectTransform.sizeDelta = Vector2.Lerp(
                _crosshairImage.rectTransform.sizeDelta,
                targetSize,
                Time.deltaTime * _sizeChangeSpeed
            );

            _crosshairImage.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 敵が画面内に表示されているかどうかをチェック
    /// BasicEnemyのIsVisibleOnScreenメソッドを使用するか、独自に判定
    /// </summary>
    /// <param name="enemy">チェックする敵オブジェクト</param>
    /// <returns>画面内に表示されている場合はtrue</returns>
    private bool IsEnemyVisibleOnScreen(GameObject enemy)
    {
        if (enemy == null || _camera == null) return false;

        // BasicEnemyコンポーネントがある場合はそのメソッドを使用
        var basicEnemy = enemy.GetComponent<BasicEnemy>();
        if (basicEnemy != null)
        {
            return basicEnemy.IsVisibleOnScreen();
        }

        // BasicEnemyがない場合は独自に判定
        Vector3 viewportPosition = _camera.WorldToViewportPoint(enemy.transform.position);

        // ビューポート座標は (0,0) が左下、(1,1) が右上
        // 少しマージンを持たせて判定
        float margin = 0.1f;
        bool isInViewport = viewportPosition.x >= -margin &&
                           viewportPosition.x <= 1f + margin &&
                           viewportPosition.y >= -margin &&
                           viewportPosition.y <= 1f + margin &&
                           viewportPosition.z > 0; // カメラの前方にある

        return isInViewport;
    }

    // 敵の位置を取得
    public Vector3 GetEnemyTargetPosition()
    {
        if (_isLockedOn && _lockedOnTarget != null)
        {
            var collider = _lockedOnTarget.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds.center;
            }
            return _lockedOnTarget.transform.position;
        }
        else if (_isTargetingEnemy && _currentTarget != null)
        {
            var collider = _currentTarget.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds.center;
            }
            return _currentTarget.transform.position;
        }

        return _aimPosition;
    }

    // クロスヘアの表示/非表示切り替え
    public void SetCrosshairVisible(bool visible)
    {
        if (_crosshairImage != null)
        {
            _crosshairImage.gameObject.SetActive(visible);
        }
    }

    // ロックオン設定の変更
    public void SetLockOnSettings(float range, float lockTime, float loseTime)
    {
        _lockOnRange = Mathf.Clamp(range, 50f, 200f);
        _lockOnTime = Mathf.Clamp(lockTime, 0.1f, 2f);
        _lockOnLoseTime = Mathf.Clamp(loseTime, 0.1f, 5f);
    }

    // 手動ロックオン切り替え
    public void ToggleLockOn()
    {
        if (_isLockedOn)
        {
            EndLockOn();
        }
        else if (_nearbyEnemies.Count > 0)
        {
            StartLockOn(_nearbyEnemies[0]);
        }
    }

    // デバッグ情報表示
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        // ロックオン情報
        if (_isLockedOn && _lockedOnTarget != null)
        {
            Vector3 screenPos = _camera.WorldToScreenPoint(_lockedOnTarget.transform.position);
            screenPos.y = Screen.height - screenPos.y; // Y座標を反転

            GUI.color = Color.yellow;
            GUI.Label(new Rect(screenPos.x + 20, screenPos.y - 30, 200, 20),
                     $"LOCKED ON: {_lockedOnTarget.name}");

            // 画面外警告
            if (!IsEnemyVisibleOnScreen(_lockedOnTarget))
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(screenPos.x + 20, screenPos.y - 50, 200, 20),
                         "TARGET OFF-SCREEN!");
            }
        }
        else if (_isTargetingEnemy && _currentTarget != null)
        {
            Vector3 screenPos = _camera.WorldToScreenPoint(_currentTarget.transform.position);
            screenPos.y = Screen.height - screenPos.y; // Y座標を反転

            GUI.color = Color.red;
            GUI.Label(new Rect(screenPos.x + 20, screenPos.y - 10, 200, 20),
                     $"Target: {_currentTarget.name}");

            // ロックオン進行度
            if (_lockOnTimer > 0)
            {
                GUI.color = Color.white;
                float progress = _lockOnTimer / _lockOnTime;
                GUI.Label(new Rect(screenPos.x + 20, screenPos.y + 10, 200, 20),
                         $"Lock-on: {progress:P0}");
            }
        }

        GUI.color = Color.white;

        // ロックオン範囲の表示（デバッグ用）
        if (_nearbyEnemies.Count > 0)
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 50, 200, 20), $"Nearby Enemies: {_nearbyEnemies.Count}");
        }

        // ロックオン解除タイマー表示（デバッグ用）
        if (_isLockedOn && _lockOffTimer > 0)
        {
            GUI.color = Color.cyan;
            float progress = _lockOffTimer / _lockOnLoseTime;
            GUI.Label(new Rect(10, 70, 200, 20), $"Losing Lock: {progress:P0}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 照準線（3D空間）
        if (_isLockedOn)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _aimPosition);
            Gizmos.DrawWireSphere(_aimPosition, 0.5f);
        }
        else if (_isTargetingEnemy)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _aimPosition);
            Gizmos.DrawWireSphere(_aimPosition, 0.3f);
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, _aimPosition);
            Gizmos.DrawWireCube(_aimPosition, Vector3.one * 0.5f);
        }

        // ロックオン範囲の可視化（画面上）
        Vector3 mousePos = Input.mousePosition;
        foreach (GameObject enemy in _nearbyEnemies)
        {
            Vector3 enemyScreenPos = _camera.WorldToScreenPoint(enemy.transform.position);
            Gizmos.color = enemy == _lockedOnTarget ? Color.yellow : Color.green;
            Gizmos.DrawWireSphere(enemy.transform.position, 1f);
        }
    }

    // パブリックメソッド：外部からサイズを変更
    public void SetCrosshairSizes(float normalSize, float enemySize, float lockedOnSize)
    {
        _normalSize = Mathf.Clamp(normalSize, 16f, 256f);
        _enemySize = Mathf.Clamp(enemySize, 16f, 256f);
        _lockedOnSize = Mathf.Clamp(lockedOnSize, 16f, 256f);

        if (_crosshairImage != null)
        {
            Vector2 currentSize;
            if (_isLockedOn)
                currentSize = new Vector2(_lockedOnSize, _lockedOnSize);
            else if (_isTargetingEnemy)
                currentSize = new Vector2(_enemySize, _enemySize);
            else
                currentSize = new Vector2(_normalSize, _normalSize);

            _crosshairImage.rectTransform.sizeDelta = currentSize;
        }
    }

    // 脈動の設定を変更
    public void SetPulseSettings(float intensity, float speed)
    {
        _pulseIntensity = Mathf.Clamp(intensity, 0f, 2f);
        _pulseSpeed = Mathf.Clamp(speed, 1f, 20f);
    }
}
// クロスヘア画像を動的に生成するヘルパークラス（拡張版）
public static class CrosshairCreator
{
    // シンプルな十字クロスヘアのSpriteを作成
    public static Sprite CreateSimpleCrosshair(int size = 32, int thickness = 2, Color color = default)
    {
        if (color == default) color = Color.white;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        // 透明で初期化
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        int center = size / 2;
        int halfThickness = thickness / 2;

        // 垂直線を描画
        for (int y = 0; y < size; y++)
        {
            for (int x = center - halfThickness; x <= center + halfThickness; x++)
            {
                if (x >= 0 && x < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 水平線を描画
        for (int x = 0; x < size; x++)
        {
            for (int y = center - halfThickness; y <= center + halfThickness; y++)
            {
                if (y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // ドット付きクロスヘアのSpriteを作成
    public static Sprite CreateDotCrosshair(int size = 32, int thickness = 2, int dotSize = 4, Color color = default)
    {
        if (color == default) color = Color.white;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        // 透明で初期化
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        int center = size / 2;
        int halfThickness = thickness / 2;
        int lineLength = (size - dotSize) / 2 - 2; // 中心のドットを避けた線の長さ

        // 上の線
        for (int y = center + dotSize / 2 + 2; y < center + dotSize / 2 + 2 + lineLength; y++)
        {
            for (int x = center - halfThickness; x <= center + halfThickness; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 下の線
        for (int y = center - dotSize / 2 - 2 - lineLength; y < center - dotSize / 2 - 2; y++)
        {
            for (int x = center - halfThickness; x <= center + halfThickness; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 左の線
        for (int x = center - dotSize / 2 - 2 - lineLength; x < center - dotSize / 2 - 2; x++)
        {
            for (int y = center - halfThickness; y <= center + halfThickness; y++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 右の線
        for (int x = center + dotSize / 2 + 2; x < center + dotSize / 2 + 2 + lineLength; x++)
        {
            for (int y = center - halfThickness; y <= center + halfThickness; y++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 中心のドット
        int dotHalf = dotSize / 2;
        for (int y = center - dotHalf; y <= center + dotHalf; y++)
        {
            for (int x = center - dotHalf; x <= center + dotHalf; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // 円形クロスヘアのSpriteを作成
    public static Sprite CreateCircleCrosshair(int size = 32, int circleRadius = 12, int thickness = 2, Color color = default)
    {
        if (color == default) color = Color.white;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        // 透明で初期化
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        int center = size / 2;

        // 円を描画
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                if (distance >= circleRadius - thickness && distance <= circleRadius)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 中心の小さなドット
        int dotSize = 2;
        for (int y = center - dotSize; y <= center + dotSize; y++)
        {
            for (int x = center - dotSize; x <= center + dotSize; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // ロックオン専用クロスヘアのSpriteを作成
    public static Sprite CreateLockOnCrosshair(int size = 48, int outerRadius = 20, int thickness = 3, Color color = default)
    {
        if (color == default) color = Color.yellow;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        // 透明で初期化
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        int center = size / 2;

        // 外側の円（破線）
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float angle = Mathf.Atan2(y - center, x - center) * Mathf.Rad2Deg;

                // 角度を0-360に正規化
                if (angle < 0) angle += 360;

                // 破線パターン（8つのセグメント）
                bool shouldDraw = (angle % 45) < 22.5f;

                if (distance >= outerRadius - thickness && distance <= outerRadius && shouldDraw)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 4つの角の三角形マーカー
        int markerSize = 4;
        int markerOffset = outerRadius + 3;

        // 上
        for (int i = 0; i < markerSize; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                int px = center - i / 2 + j;
                int py = center - markerOffset - i;
                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    pixels[py * size + px] = color;
                }
            }
        }

        // 下
        for (int i = 0; i < markerSize; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                int px = center - i / 2 + j;
                int py = center + markerOffset + i;
                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    pixels[py * size + px] = color;
                }
            }
        }

        // 左
        for (int i = 0; i < markerSize; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                int px = center - markerOffset - i;
                int py = center - i / 2 + j;
                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    pixels[py * size + px] = color;
                }
            }
        }

        // 右
        for (int i = 0; i < markerSize; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                int px = center + markerOffset + i;
                int py = center - i / 2 + j;
                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    pixels[py * size + px] = color;
                }
            }
        }

        // 中心の十字
        int crossSize = 6;
        int crossThickness = 2;
        int crossHalf = crossThickness / 2;

        // 垂直線
        for (int y = center - crossSize; y <= center + crossSize; y++)
        {
            for (int x = center - crossHalf; x <= center + crossHalf; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // 水平線
        for (int x = center - crossSize; x <= center + crossSize; x++)
        {
            for (int y = center - crossHalf; y <= center + crossHalf; y++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
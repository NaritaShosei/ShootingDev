using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSelectView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private WeaponDatabase _weaponDatabase;
    [SerializeField] private WeaponCell _weaponCell;
    [SerializeField] private Transform _cellParent;
    [SerializeField] private WeaponExplanation _explanation;
    private WeaponCell _currentCell;
    private List<WeaponCell> _cells = new();
    [SerializeField] private BasicButton _exitButton;
    [SerializeField] private BasicButton _openButton;
    [Header("アニメーション設定")]
    [SerializeField] private float _animationDuration = 0.2f;

    private void Start()
    {
        SetUI();
        _exitButton.OnClick += () => OnClick(0);
        _openButton.OnClick += () => OnClick(1);
    }

    private void SetUI()
    {
        foreach (var data in _weaponDatabase.GetAllWeapons())
        {
            var cell = Instantiate(_weaponCell, _cellParent);
            cell.Initialize(data.WeaponIcon, data.WeaponMoney, data.WeaponID);
            _cells.Add(cell);
        }
        _currentCell = _cells[0];
        _currentCell.Select();
        SetExplanation(_currentCell.Id);
    }

    private void SetExplanation(int id)
    {
        var data = _weaponDatabase.GetWeapon(id);
        _explanation.Set(data.WeaponName, data.AttackPower, data.AttackRate);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click");
        if (eventData.pointerCurrentRaycast.gameObject.TryGetComponent(out WeaponCell cell))
        {
            Debug.Log(cell.gameObject.name);
            _currentCell?.UnSelect();
            ServiceLocator.Get<WeaponSelector>().SelectWeapon(cell.Id);
            _currentCell = cell;
            _currentCell.Select();
            SetExplanation(_currentCell.Id);
        }
    }
    /// <summary>
    /// ボタンクリック時のアニメーション
    /// </summary>
    public void OnClick(int target)
    {
        transform.DOScaleY(target, _animationDuration).
            SetEase(Ease.OutElastic);
    }
}

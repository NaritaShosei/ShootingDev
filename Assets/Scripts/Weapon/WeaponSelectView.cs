using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSelectView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private WeaponDatabase _weaponDatabase;
    [SerializeField] private WeaponCell _weaponCell;
    [SerializeField] private Transform _cellParent;
    private WeaponCell _currentCell;
    private List<WeaponCell> _cells = new();

    private void Start()
    {
        SetUI();
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
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSelectView : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] private WeaponDatabase _weaponDatabase;
    [SerializeField] private WeaponCell _weaponCell;
    [SerializeField] private Transform _cellParent;

    private void Start()
    {
        SetUI();
    }

    private void SetUI()
    {
        foreach (var data in _weaponDatabase.GetAllWeapons())
        {
            var cell = Instantiate(_weaponCell, _cellParent);
            cell.Initialize(data.WeaponIcon, data.WeaponMoney,data.WeaponID);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.TryGetComponent(out WeaponCell cell))
        {
            ServiceLocator.Get<WeaponSelector>().SelectWeapon(cell.Id);
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCell : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private Image _selectPanel;
    private int _id;
    public int Id => _id;

    public void Initialize(Sprite icon, int money, int id)
    {
        _icon.sprite = icon;
        _moneyText.text = $"{money}";
        _id = id;
    }

    public void Select()
    {
        var c = _selectPanel.color;
        c.a = 1;
        _selectPanel.color = c;
    }

    public void UnSelect()
    {
        var c = _selectPanel.color;
        c.a = 0;
        _selectPanel.color = c;
    }
}

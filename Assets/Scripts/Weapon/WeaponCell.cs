using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCell : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _moneyText;
    private int _id;
    public int Id => _id;

    public void Initialize(Sprite icon, int money, int id)
    {
        _icon.sprite = icon;
        _moneyText.text = $"{money}";
        _id = id;
    }
}

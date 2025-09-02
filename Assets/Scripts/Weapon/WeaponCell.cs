using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCell : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _powerText;
    [SerializeField] private TMP_Text _rateText;

    public void Initialize(Sprite icon, string name, float power, float rate)
    {
        _icon.sprite = icon;
        _nameText.text = name;
        _powerText.text = $"{power}";
        _rateText.text = $"{rate}";
    }
}

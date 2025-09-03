using TMPro;
using UnityEngine;

public class WeaponExplanation : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _power;
    [SerializeField] private TMP_Text _rate;

    public void Set(string name, float power, float rate)
    {
        _name.text = name;
        _power.text = $"{power}";
        _rate.text = $"{rate}";
    }
}

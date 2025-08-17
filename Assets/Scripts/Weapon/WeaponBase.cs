using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected WeaponData _data;

    public WeaponData Data => _data;

    public virtual void Initialize(WeaponData data)
    {
        _data = data;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }
    public abstract void Attack();
}
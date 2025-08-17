using UnityEngine;

public class PlayerAttacker : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
    private bool _isAttacking;
    private void Start()
    {
        ServiceLocator.Get<InputManager>().OnAttack += Attack;
    }

    private void Update()
    {
        if (_isAttacking)
        {
            _weaponManager.CurrentWeapon.Attack();
        }
    }

    private void Attack()
    {
        _isAttacking = !_isAttacking;
    }
}

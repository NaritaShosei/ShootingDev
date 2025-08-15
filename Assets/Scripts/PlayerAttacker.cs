using UnityEngine;

public class PlayerAttacker : MonoBehaviour
{
    private bool _isAttacking;
    private void Start()
    {
        ServiceLocator.Get<InputManager>().OnAttack += Attack;
    }

    private void Attack()
    {
        _isAttacking = !_isAttacking;
    }
}

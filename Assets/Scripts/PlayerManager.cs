using UnityEngine;

public class PlayerManager : MonoBehaviour, IPlayerHealth
{
    [SerializeField] private float _maxHealth;
    private float _currentHealth;

    public float Health => _currentHealth;

    public float MaxHealth => _maxHealth;

    public bool IsAlive => Health > 0;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
    }
}

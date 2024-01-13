using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    // Declare an event that will be used to notify subscribers of health changes.
    public event Action<float> OnHealthChanged;
    public event Action OnHealthCleared;

    [SerializeField]
    private float currentHealth;
    [SerializeField] private float maxHealth;

    public float MaxHealth {  get { return maxHealth; } }
    public float CurrentHealth { get { return currentHealth; } set { currentHealth = value; } }
    private void Awake()
    {
        currentHealth = MaxHealth; // Set the initial health to max health.
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, MaxHealth);

        // Trigger the event to notify subscribers of the health change.
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth == 0)
        {
            OnHealthCleared?.Invoke();
        }
    }
}
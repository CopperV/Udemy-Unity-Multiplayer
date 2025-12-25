using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField]
    public int MaxHealth { get; private set; } = 100;

    public NetworkVariable<int> CurrentHealth = new();

    private bool isDead;

    public event Action<Health> DieEvent;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int value) => ModifyHealth(-value);

    public void RestoreHealth(int value) => ModifyHealth(value);

    private void ModifyHealth(int value)
    {
        if (isDead)
            return;

        CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value + value, 0, MaxHealth);

        if(CurrentHealth.Value <= 0)
        {
            isDead = true;
            DieEvent?.Invoke(this);
        }
    }
}

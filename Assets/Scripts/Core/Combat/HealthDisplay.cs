using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]

    [SerializeField]
    private Health health;

    [SerializeField]
    private Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsClient)
            return;

        health.CurrentHealth.OnValueChanged += OnHealthChanged;
        UpdateDisplay(health.CurrentHealth.Value, health.MaxHealth);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsClient)
            return;

        health.CurrentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int prevHealth, int newHealth) => UpdateDisplay(newHealth, health.MaxHealth);

    private void UpdateDisplay(float currentHealth, float maxHealth) => healthBarImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
}

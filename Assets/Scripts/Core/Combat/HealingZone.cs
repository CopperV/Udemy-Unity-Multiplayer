using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Image healPowerBar;

    [Header("Settings")]
    [SerializeField]
    private int maxHealPower = 30;

    [SerializeField]
    private float healCooldown = 60f;

    [SerializeField]
    private float healTickRate = 1f;

    [SerializeField]
    private int coinsPerTick = 10;

    [SerializeField]
    private int healthPerTick = 10;

    private List<TankPlayer> playersInZone = new();
    private NetworkVariable<int> HealPower = new();

    private float remainingCooldown;
    private float tickTimer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            HealPower.OnValueChanged += OnHealPowerChanged;
            OnHealPowerChanged(0, HealPower.Value);
        }

        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsClient)
        {
            HealPower.OnValueChanged -= OnHealPowerChanged;
        }
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if(remainingCooldown > 0)
        {
            remainingCooldown -= Time.deltaTime;

            if (remainingCooldown > 0)
                return;

            HealPower.Value = maxHealPower;
        }

        tickTimer += Time.deltaTime;
        if (tickTimer < 1 / healTickRate)
            return;

        foreach (var player in playersInZone)
        {
            if (HealPower.Value <= 0)
                break;

            if (player.Health.CurrentHealth.Value == player.Health.MaxHealth)
                continue;
            if (player.Wallet.TotalCoins.Value < coinsPerTick)
                continue;

            player.Wallet.SpendCoins(coinsPerTick);
            player.Health.RestoreHealth(healthPerTick);

            HealPower.Value--;
        }

        if (HealPower.Value <= 0)
        {
            remainingCooldown = healCooldown;
        }

        tickTimer = tickTimer % (1 / healTickRate);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || collision.attachedRigidbody == null)
            return;

        if (!collision.attachedRigidbody.TryGetComponent(out TankPlayer player))
            return;

        playersInZone.Add(player);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer || collision.attachedRigidbody == null)
            return;

        if (!collision.attachedRigidbody.TryGetComponent(out TankPlayer player))
            return;

        playersInZone.Remove(player);
    }

    private void OnHealPowerChanged(int previousValue, int newValue)
    {
        healPowerBar.fillAmount = Mathf.Clamp01((float)newValue / maxHealPower);
    }
}

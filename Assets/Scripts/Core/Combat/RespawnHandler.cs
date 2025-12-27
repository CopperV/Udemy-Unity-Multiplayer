using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField]
    private TankPlayer playerPrefab;

    [SerializeField]
    private float keptCoinPercentage = 0.8f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            OnPlayerSpawn(player);
        }

        TankPlayer.PlayerSpawnEvent += OnPlayerSpawn;
        TankPlayer.PlayerDespawnEvent += OnPlayerDespawn;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer)
            return;

        TankPlayer.PlayerSpawnEvent -= OnPlayerSpawn;
        TankPlayer.PlayerDespawnEvent -= OnPlayerDespawn;
    }

    private void OnPlayerSpawn(TankPlayer player)
    {
        player.Health.DieEvent += (health) => OnDie(player, health);
    }

    private void OnPlayerDespawn(TankPlayer player)
    {
        player.Health.DieEvent -= (health) => OnDie(player, health);
    }

    private void OnDie(TankPlayer player, Health health)
    {
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * keptCoinPercentage);

        Destroy(player.gameObject);

        StartCoroutine(RespawnCoroutine(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnCoroutine(ulong ownerClientId, int keptCoins)
    {
        yield return null;

        var newPlayer = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        newPlayer.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        newPlayer.Wallet.TotalCoins.Value = keptCoins;
    }
}

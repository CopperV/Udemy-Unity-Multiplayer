using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject playerPrefab;

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
        Destroy(player.gameObject);

        StartCoroutine(RespawnCoroutine(player.OwnerClientId));
    }

    private IEnumerator RespawnCoroutine(ulong ownerClientId)
    {
        yield return null;

        var newPlayer = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        newPlayer.SpawnAsPlayerObject(ownerClientId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField]
    private RectTransform leaderboardEntityHolder;

    [SerializeField]
    private LeaderboardEntityDisplay leaderboardEntityPrefab;

    [SerializeField]
    private int entitiesToDisplay = 8;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;
    private List<LeaderboardEntityDisplay> leaderboardDisplays = new();

    private void Awake()
    {
        leaderboardEntities = new();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            leaderboardEntities.OnListChanged += OnListChanged;

            foreach(var entity in leaderboardEntities)
            {
                OnListChanged(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

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

        if (IsClient)
        {
            leaderboardEntities.OnListChanged -= OnListChanged;
        }

        if (!IsServer)
            return;

        TankPlayer.PlayerSpawnEvent -= OnPlayerSpawn;
        TankPlayer.PlayerDespawnEvent -= OnPlayerDespawn;
    }

    private void OnPlayerSpawn(TankPlayer player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.playerName.Value,
            Coins = 0
        });

        player.Wallet.TotalCoins.OnValueChanged += (old, @new) => OnWalletChanged(player.OwnerClientId, @new);
    }

    private void OnPlayerDespawn(TankPlayer player)
    {
        if (leaderboardEntities != null)
        {
            foreach (var entity in leaderboardEntities)
            {
                if (entity.ClientId != player.OwnerClientId)
                    continue;

                leaderboardEntities.Remove(entity);
                break;
            }
        }

        player.Wallet.TotalCoins.OnValueChanged -= (old, @new) => OnWalletChanged(player.OwnerClientId, @new);
    }

    private void OnWalletChanged(ulong clientId, int newValue)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientId != clientId)
                continue;

            leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                Coins = newValue
            };
            return;
        }
    }

    private void OnListChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (leaderboardDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                    break;

                var entity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                var state = changeEvent.Value;

                entity.Init(state.ClientId, state.PlayerName, state.Coins);

                leaderboardDisplays.Add(entity);
                break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                var displayToRemove = leaderboardDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if(displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    leaderboardDisplays.Remove(displayToRemove);
                }
                break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                var displayToUpdate = leaderboardDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if(displayToUpdate != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }

        leaderboardDisplays.Sort((ent1, ent2) => ent2.Coins.CompareTo(ent1.Coins));

        for (int i = 0; i < leaderboardDisplays.Count; i++)
        {
            leaderboardDisplays[i].transform.SetSiblingIndex(i);
            leaderboardDisplays[i].UpdateDisplay();

            bool shouldShow = i < entitiesToDisplay;
            leaderboardDisplays[i].gameObject.SetActive(shouldShow);
        }

        var myDisplay = leaderboardDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);
        if(myDisplay != null)
        {
            var index = myDisplay.transform.GetSiblingIndex();

            if(index >= entitiesToDisplay)
            {
                leaderboardDisplays[entitiesToDisplay - 1].gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }
}

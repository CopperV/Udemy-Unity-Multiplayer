using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Health health;

    [Header("Prefabs")]
    [SerializeField]
    private BountyCoin coinPrefab;

    [Header("Settings")]
    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private float bountyPercentage = 0.2f;

    [SerializeField]
    private float coinSpread = 3f;

    [SerializeField]
    private int bountyCoinCount = 10;

    [SerializeField]
    private int minBountyCoinValue = 5;

    public NetworkVariable<int> TotalCoins = new();

    private float coinRadius;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        health.DieEvent += OnDie;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer)
            return;

        health.DieEvent -= OnDie;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Coin coin))
            return;

        var coinValue = coin.Collect();

        if (!IsServer)
            return;

        TotalCoins.Value += coinValue;
    }

    public void SpendCoins(int coins)
    {
        if (TotalCoins.Value < coins)
            return;

        TotalCoins.Value -= coins;
    }

    private void OnDie(Health health)
    {
        int bountyValue = (int)(TotalCoins.Value * bountyPercentage);

        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoinValue)
            return;

        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin coin = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
            coin.SetValue(bountyCoinValue);

            coin.NetworkObject.Spawn();
        }
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2) transform.position + Random.insideUnitCircle * coinSpread;

            if (Physics2D.OverlapCircleAll(spawnPoint, coinRadius, layerMask).Length == 0)
                return spawnPoint;
        }
    }
}

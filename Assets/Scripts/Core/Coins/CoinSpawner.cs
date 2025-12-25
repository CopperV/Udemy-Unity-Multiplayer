using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField]
    private RespawningCoin coinPrefab;

    [SerializeField]
    private int maxCoins = 50;

    [SerializeField]
    private int coinValue = 10;

    [SerializeField]
    private Vector2 xSpawnRange;

    [SerializeField]
    private Vector2 ySpawnRange;

    [SerializeField]
    private LayerMask layerMask;

    private float coinRadius;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }
    }

    private void SpawnCoin()
    {
        var coin = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);

        coin.SetValue(coinValue);
        coin.GetComponent<NetworkObject>().Spawn();

        coin.CoinCollectedEvent += OnCollect;
    }

    private Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;

        while (true)
        {
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);

            Vector2 spawnPoint = new(x, y);

            if(Physics2D.OverlapCircleAll(spawnPoint, coinRadius, layerMask).Length == 0)
                return spawnPoint;
        }
    }

    private void OnCollect(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }
}

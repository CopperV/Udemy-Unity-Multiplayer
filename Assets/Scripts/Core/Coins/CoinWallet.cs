using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new();

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
}

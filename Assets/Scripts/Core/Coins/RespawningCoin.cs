using System;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> CoinCollectedEvent;

    private Vector3 previousPos;

    private void Update()
    {
        if(previousPos != transform.position)
        {
            Show(true);
        }

        previousPos = transform.position;
    }

    public override int Collect()
    {
        if (!IsServer)
        {
            Show(false);
            return 0;
        }

        if (alreadyCollected)
            return 0;

        alreadyCollected = true;

        CoinCollectedEvent?.Invoke(this);

        return coinValue;
    }

    public void Reset()
    {
        alreadyCollected = false;
    }
}

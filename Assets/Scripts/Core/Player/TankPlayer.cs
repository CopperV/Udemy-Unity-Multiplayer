using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private CinemachineCamera cmCamera;

    [field: SerializeField]
    public Health Health { get; private set; }

    [field: SerializeField]
    public CoinWallet Wallet { get; private set; }

    [SerializeField]
    private SpriteRenderer minimapIcon;

    [Header("Settings")]
    [SerializeField]
    private int cmCameraPriority = 100;

    [SerializeField]
    private Color myColor = Color.blue;

    public NetworkVariable<FixedString32Bytes> playerName = new();

    public static event Action<TankPlayer> PlayerSpawnEvent;
    public static event Action<TankPlayer> PlayerDespawnEvent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            var data = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            playerName.Value = data.userName;

            PlayerSpawnEvent?.Invoke(this);
        }

        if (IsOwner)
        {
            cmCamera.Priority = cmCameraPriority;
            minimapIcon.color = myColor;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if(IsServer)
        {
            PlayerDespawnEvent?.Invoke(this);
        }
    }
}

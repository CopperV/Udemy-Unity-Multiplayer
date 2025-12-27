using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private Dictionary<ulong, string> clientIdToAuth = new();
    private Dictionary<string, UserData> clientAuthToData = new();

    private NetworkManager networkManager;

    public event Action<string> OnClientLeft;

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += OnConnectionApproval;
        networkManager.OnServerStarted += OnServerStarted;
    }

    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var payload = Encoding.UTF8.GetString(request.Payload);
        var userData = JsonUtility.FromJson<UserData>(payload);

        Debug.Log(userData.userName);

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        clientAuthToData[userData.userAuthId] = userData;

        response.Approved = true;
        response.Position = SpawnPoint.GetRandomSpawnPos();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
    }

    private void OnServerStarted()
    {
        networkManager.OnClientDisconnectCallback += OnDisconnect;
    }

    private void OnDisconnect(ulong clientId)
    {
        if(clientIdToAuth.TryGetValue(clientId, out var authId))
        {
            clientIdToAuth.Remove(clientId);
            clientAuthToData.Remove(authId);
            OnClientLeft?.Invoke(authId);
        }
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (!clientIdToAuth.TryGetValue(clientId, out var authId) ||
            !clientAuthToData.TryGetValue(authId, out var data))
            return null;

        return data;
    }

    public void Dispose()
    {
        if(networkManager != null)
        {
            networkManager.ConnectionApprovalCallback -= OnConnectionApproval;
            networkManager.OnServerStarted -= OnServerStarted;
            networkManager.OnClientDisconnectCallback -= OnDisconnect;

            if (networkManager.IsListening)
            {
                networkManager.Shutdown();
            }
        }
    }
}

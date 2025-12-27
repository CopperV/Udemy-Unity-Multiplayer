using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private const string MenuSceneName = "Menu";

    private NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnDisconnect;
    }

    public void Dispose()
    {
        if(networkManager != null)
            networkManager.OnClientDisconnectCallback -= OnDisconnect;
    }

    public void Disconnect()
    {
        if (SceneManager.GetActiveScene().name != MenuSceneName)
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
        }
    }

    private void OnDisconnect(ulong clientId)
    {
        if (clientId != 0 && clientId != networkManager.LocalClientId)
            return;

        Disconnect();
    }
}

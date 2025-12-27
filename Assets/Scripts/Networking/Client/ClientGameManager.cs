using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private const string Menu = "Menu";

    private JoinAllocation allocation;
    private NetworkClient networkClient;

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        networkClient = new(NetworkManager.Singleton);

        var state = await AuthenticationWrapper.DoAuth();

        if(state == AuthState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(Menu);
    }

    public async Task StartClientAsync(string code)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(code);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }

        RelayServerData serverData = AllocationUtils.ToRelayServerData(allocation, "dtls");

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(serverData);

        UserData userData = new UserData()
        {
            userName = PlayerPrefs.GetString(NameSelector.playerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }

    public void Disconnect()
    {
        networkClient.Disconnect();
    }
}

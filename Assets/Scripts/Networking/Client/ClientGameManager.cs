using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private const string Menu = "Menu";

    private JoinAllocation allocation;

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

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

        NetworkManager.Singleton.StartClient();
    }
}

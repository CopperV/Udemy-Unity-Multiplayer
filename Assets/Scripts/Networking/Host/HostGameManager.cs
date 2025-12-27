using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable
{
    private const int maxConnections = 20;
    private const string GameSceneName = "Game";

    private Allocation allocation;
    private Lobby lobby;

    private Coroutine heartbeatCoroutine;

    private string joinCode;
    private string lobbyId;
    public NetworkServer NetworkServer { get; private set; }

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Lobby Code: "+joinCode);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }

        RelayServerData serverData = AllocationUtils.ToRelayServerData(allocation, "dtls");

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(serverData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new()
            {
                {
                    "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: joinCode)
                }
            };

            string playerName = PlayerPrefs.GetString(NameSelector.playerNameKey, "Unknown");
            lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", maxConnections, lobbyOptions);
            lobbyId = lobby.Id;

            heartbeatCoroutine = HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            return;
        }

        NetworkServer = new(NetworkManager.Singleton);

        UserData userData = new UserData()
        {
            userName = PlayerPrefs.GetString(NameSelector.playerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkServer.OnClientLeft += OnClientLeft;

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);

            yield return delay;
        }
    }

    public void Dispose()
    {
        Shutdown();
    }

    public async void Shutdown()
    {
        if (heartbeatCoroutine != null)
            HostSingleton.Instance.StopCoroutine(heartbeatCoroutine);

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }

            lobbyId = string.Empty;
        }

        NetworkServer.OnClientLeft -= OnClientLeft;

        NetworkServer?.Dispose();
    }

    private async void OnClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
}

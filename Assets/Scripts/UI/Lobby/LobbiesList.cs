using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField]
    private LobbyItem lobbyItemPrefab;

    [SerializeField]
    private RectTransform lobbyItemParent;

    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshing)
            return;

        isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new();
            options.Count = 25;

            options.Filters = new()
            {
                new QueryFilter(field: QueryFilter.FieldOptions.AvailableSlots, op: QueryFilter.OpOptions.GT, value: "0"),
                new QueryFilter(field: QueryFilter.FieldOptions.IsLocked, op: QueryFilter.OpOptions.EQ, value: "0"),
            };

            var response = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var lobby in response.Results)
            {
                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Init(this, lobby);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining)
            return;

        isJoining = true;
        
        try
        {
            var joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }

        isJoining = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

// Represents a single lobby entry in the scrollable list of available lobbies.
public class LobbyListSingleUI : MonoBehaviour {

    // UI element that displays the lobby's name
    [SerializeField] private TextMeshProUGUI lobbyNameText;

    // UI element that displays how many players are currently in the lobby
    [SerializeField] private TextMeshProUGUI playersText;

    // Stores the Lobby object data for this UI card
    private Lobby lobby;


    private void Awake() {

        // When a single lobby is clicked, call LobbyManager to join the associated lobby
        GetComponent<Button>().onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobby(lobby);
        });
    }

    // Populates the UI text fields with lobby information.
    // Called by LobbyListUI when creating/updating the scrollable list.
    public void UpdateLobby(Lobby lobby) {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
    }


}
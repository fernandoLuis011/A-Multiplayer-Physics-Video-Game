using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

// Displays the full lobby screen UI after a player joins.
// Shows player list, lobby name, player count,
// and gives host access to Start Game and Kick buttons.
public class LobbyUI : MonoBehaviour {

    // instance for easy access from other scripts
    public static LobbyUI Instance { get; private set; }

    // Prefab used to represent one player in the lobby
    [SerializeField] private Transform playerSingleTemplate;

    // UI container for the list of player entries
    [SerializeField] private Transform container;

    // UI text fields
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;

    // Buttons for leaving and starting the game
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button startGameButton;


    // Initializes button behavior and hides the template player entry on load.
    private void Awake() {
        Instance = this;

        // The template is only used for cloning, so we keep it hidden
        playerSingleTemplate.gameObject.SetActive(false);

        // When we click the leave button, we call LeaveLobby from lobby manager
        leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });

        // When we click the start game button, we call StartGame from lobby manager
        startGameButton.onClick.AddListener(() => {
            LobbyManager.Instance.StartGame();
             
        });
    }

    // Subscribes to lobby-related events
    private void Start() {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        // The lobby remains hidden on initialization of game.
        Hide();
    }

    // Called when the player leaves or is kicked from the lobby.
    // Clears the UI and hides the panel.
    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e) {
        ClearLobby();
        Hide();
    }

    // Called when the lobby is joined or updated.
    // Triggers a UI refresh.
    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        UpdateLobby();
    }

    // Helper method to get the current joined lobby from LobbyManager.
    private void UpdateLobby() {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    // Refreshes the UI to show updated lobby name, player list, and player count.
    // Rebuilds all player UI elements.
    private void UpdateLobby(Lobby lobby) {
        ClearLobby(); // Remove old player entries

        foreach (Player player in lobby.Players) {

            // Create a new UI card for each player
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);

            // Get the UI script on the new entry
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            // Only hosts can kick other players (not themselves)
            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            // Fill in player name and info
            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        // Update lobby name and player count at the top of the screen
        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        Show(); // Make sure the UI is visible
    }

    // Destroys all player UI entries except the hidden template.
    private void ClearLobby() {
        foreach (Transform child in container) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    // Hides the entire lobby UI panel.
    private void Hide() {
        gameObject.SetActive(false);
    }

    // Shows the lobby UI panel.
    private void Show() {
        gameObject.SetActive(true);
    }

}
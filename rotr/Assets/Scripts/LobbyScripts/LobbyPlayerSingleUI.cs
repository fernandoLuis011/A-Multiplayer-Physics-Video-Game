using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

// Represents a single plaayer in the lobby UI
public class LobbyPlayerSingleUI : MonoBehaviour {

    // UI element that displays the name of a player
    [SerializeField] private TextMeshProUGUI playerNameText;

    // UI element that displays a button to kick a player
    [SerializeField] private Button kickPlayerButton;

    // The Lobby.Player object this UI entry represents
    private Player player;

    // On initialization, assign the KickPlayer method to the button click event.
    private void Awake() {
        kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    // Called to show or hide the Kick button.
    // Hosts can see it, regular players cannot.
    public void SetKickPlayerButtonVisible(bool visible) {
        kickPlayerButton.gameObject.SetActive(visible);
    }

    // Updates this UI entry with the provided player's data.
    public void UpdatePlayer(Player player) {
        this.player = player;

        // Access the player's name from their Lobby metadata
        playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
    }

    // Called when the Kick button is clicked.
    private void KickPlayer() {
        if (player != null) {

            // Tells the LobbyManager to remove this player from the lobby.
            LobbyManager.Instance.KickPlayer(player.Id);
        }
    }


}
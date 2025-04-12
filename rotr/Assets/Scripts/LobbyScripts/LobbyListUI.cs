using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour {

    // reference to allow other scripts to access this UI
    public static LobbyListUI Instance { get; private set; }


    // Reference to a prefab used for displaying one lobby (disabled by default in the editor)
    [SerializeField] private Transform lobbySingleTemplate;

    // The container that holds the list of instantiated lobby UI entries
    [SerializeField] private Transform container;

    // UI buttons to refresh the list and create a new lobby
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;

    // sets up UI button listeners. 
    private void Awake() {
        Instance = this;
        
        // The template is hidden in the editor so we can clone it later
        lobbySingleTemplate.gameObject.SetActive(false);

        // Set up button click listeners
        refreshButton.onClick.AddListener(RefreshButtonClick);
        createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);
    }
    
    // Subscribes to events from the LobbyManager to respond to lobby changes and transitions.
    private void Start() {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    // Triggered when the player is kicked from a lobby.
    //  Re-opens the lobby list screen.
    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Show();
    }

    // Triggered when the player leaves a lobby.
    // Re-opens the lobby list screen.
    private void LobbyManager_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    // Triggered when the player successfully joins a lobby.
    // Hides the lobby list UI.
    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Hide();
    }

    // Triggered when the list of available lobbies changes.
    // Rebuilds the lobby list UI.
    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    // Rebuilds the UI list of lobbies from scratch using the updated list.
    private void UpdateLobbyList(List<Lobby> lobbyList) { // List of lobbies returned by Unity Lobby Service

        // Remove old lobby entries (except the hidden template)
        foreach (Transform child in container) {
            if (child == lobbySingleTemplate) continue; // Don't delete the template itself

            Destroy(child.gameObject);
        }

        // Create a new UI entry for each available lobby
        foreach (Lobby lobby in lobbyList) {
            // Create a new UI entry for each lobby returned by Unity
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);

            // Update every lobby
            // Pass the lobby data to the prefab’s script
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    // Called when the refresh button is clicked.
    // Requests a new lobby list from the LobbyManager.
    private void RefreshButtonClick() {
        LobbyManager.Instance.RefreshLobbyList();
    }

    // Called when the create lobby button is clicked.
    // Opens the lobby creation UI.
    private void CreateLobbyButtonClick() {
        LobbyCreateUI.Instance.Show();
    }

    // Hides the lobby list panel.
    private void Hide() {
        gameObject.SetActive(false);
    }

    // Shows the lobby list panel.
    private void Show() {
        gameObject.SetActive(true);
    }

}
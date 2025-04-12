using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class LobbyCreateUI : MonoBehaviour {

    //Instance to allow other scripts to access this UI globally
    public static LobbyCreateUI Instance { get; private set; }

    // UI Button references

    [SerializeField] private Button createButton;
    [SerializeField] private Button lobbyNameButton;
    //[SerializeField] private Button publicPrivateButton;
    //[SerializeField] private Button maxPlayersButton;


    // UI Text elements for displaying current lobby settings
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    //[SerializeField] private TextMeshProUGUI publicPrivateText;
    //[SerializeField] private TextMeshProUGUI maxPlayersText;


    // Internal variables for storing lobby configuration
    private string lobbyName;
    //private bool isPrivate;
    //private int maxPlayers;


    // Initializes and sets up all button behaviors.
    private void Awake() {
        Instance = this;

        // When the create button is clicked, call LobbyManager to create the lobby
        createButton.onClick.AddListener(async () => {

            // First, get the current list of lobbies
            QueryResponse response;
            try {
                response = await Lobbies.Instance.QueryLobbiesAsync(new QueryLobbiesOptions {
                    Count = 20
                });
            } catch (LobbyServiceException e) {
                Debug.LogError("Failed to query lobbies: " + e);
                return;
            }

            // Check if any lobby already has the same name
            foreach (var existingLobby in response.Results) {
                if (existingLobby.Name == lobbyName) {

                    // Show warning to user via UI
                    UI_InputWindow.ShowMessage_Static("Lobby name already taken");
                    return;
                }
            }


            // If name is unique, proceed to create the lobby
            LobbyManager.Instance.CreateLobby(
                lobbyName   // User-defined lobby name
                //maxPlayers,
                //isPrivate 
            );
            Hide(); // Close the UI after creating

        }); 

        // When the "Lobby Name" button is clicked, prompt user to enter a name
        lobbyNameButton.onClick.AddListener(() => {
            UI_InputWindow.Show_Static("Lobby Name", lobbyName, "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ .,-", 20,
            () => {
                // Cancel
            },
            (string lobbyName) => {
                this.lobbyName = lobbyName;
                UpdateText(); // Update display with new name
            });
        });

        // Toggle privacy status when "Public/Private" button is clicked
        /*
        publicPrivateButton.onClick.AddListener(() => {
            isPrivate = !isPrivate;
            UpdateText();
        });
        */


        // Prompt user to set max players when the corresponding button is clicked
        /*
        maxPlayersButton.onClick.AddListener(() => {
            UI_InputWindow.Show_Static("Max Players", maxPlayers,
            () => {
                // Cancel
            },
            (int maxPlayers) => {
                this.maxPlayers = maxPlayers;
                UpdateText();
            });
        });
        */

        Hide();
    }

    // Updates the UI text elements to reflect the current settings.
    private void UpdateText() {
        lobbyNameText.text = lobbyName;
        //publicPrivateText.text = isPrivate ? "Private" : "Public";
        //maxPlayersText.text = maxPlayers.ToString();
    }

    //Hides the create lobby UI.
    public void Hide() {
        gameObject.SetActive(false);
    }

    // Shows the create lobby UI and sets default values.
    public void Show() {
        // Make the menu visible
        gameObject.SetActive(true);

        lobbyName = "MyLobby";
        //isPrivate = false;
        //maxPlayers = 4;

        UpdateText();
    }



}
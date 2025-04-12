using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerName : MonoBehaviour {

    //instance for global access.
    public static EditPlayerName Instance { get; private set; }

    // Event triggered whenever the player's name is changed.
    public event EventHandler OnNameChanged;

    // Reference to the UI text element displaying the player name.
    [SerializeField] private TextMeshProUGUI playerNameText;

    // Internal variable storing the player's name.
    private string playerName;

    private void Awake() {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
        
        playerName = PlayerPrefs.GetString("PlayerName", "");

        // If there's no saved name, default to placeholder text
        if (string.IsNullOrEmpty(playerName))
        {
            playerNameText.text = "Enter Username";
        }
        else
        {
            playerNameText.text = playerName;
        }

        // When the Enter username button is clicked:
        GetComponent<Button>().onClick.AddListener(() => {

            // Show the input window for editing player name.
            // Parameters:
            // - Title: "Player Name"
            // - Current name
            // - Valid characters
            // - Max character limit (20)
            // - Cancel callback
            // - Confirm callback (stores new name)
            UI_InputWindow.Show_Static("Player Name", playerName, "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ .,-", 20,
            () => {
                // Cancel
            },
            (string newName) => {
                // Save the new name
                playerName = newName;
                PlayerPrefs.SetString("PlayerName", playerName);
                PlayerPrefs.Save();

                // Update the on-screen text element
                playerNameText.text = playerName;
                
                // Trigger OnNameChanged event
                //If anyone is subscribed to OnNameChanged, notify them
                OnNameChanged?.Invoke(this, EventArgs.Empty);
            });
        });

    
    }

    //Subscribes to the OnNameChanged event when the script starts.
    private void Start() {

        //Whenever OnNameChanged is triggered, call the method EditPlayerName_OnNameChanged
        OnNameChanged += EditPlayerName_OnNameChanged;
    }

    //Called when the player's name is changed
    private void EditPlayerName_OnNameChanged(object sender, EventArgs e) {

        //Updates the player name in the active lobby via LobbyManager.
        LobbyManager.Instance.UpdatePlayerName(GetPlayerName());
    }

    //Returns the current player name.
    public string GetPlayerName() {
        return playerName;
    }

    public void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.Save();
    }


}
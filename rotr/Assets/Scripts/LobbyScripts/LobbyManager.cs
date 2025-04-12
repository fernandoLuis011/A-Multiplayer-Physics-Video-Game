using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class LobbyManager : MonoBehaviour {

    // instance for global access
    public static LobbyManager Instance { get; private set; }

    // Constants used for metadata keys
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_START_GAME = "StartGame_RelayCode";

    // Public events used to notify UI and other systems
    public event EventHandler OnLeftLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnGameStarted;
    public class LobbyEventArgs : EventArgs {
        public Lobby lobby;
    
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs {
        public List<Lobby> lobbyList;
    }

    // Internal state
    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyListTimer = 5f;
    private Lobby joinedLobby;
    private string playerName;


    private void Awake() {

        Instance = this;
       
    }

    private void Update() {
        //HandleRefreshLobbyList(); // Disabled Auto Refresh for testing with multiple builds
        HandleLobbyHeartbeat(); // send a heartbeat to Unity Lobby service every 15 seconds.
        HandleLobbyPolling();
    }


    // Authenticates the player anonymously with a custom profile name.
    // Used when a player enters their name
   // It's marked async because it uses await to make an asynchronous call
    public async void Authenticate(string playerName) {
        this.playerName = playerName;

        // This is used to configure Unity Services before initialization
        InitializationOptions initializationOptions = new InitializationOptions();

        // Sets the player’s name as their profile name.
        // useful for local multiplayer testing, because it forces Unity to treat each player (or editor instance) as a unique user.
        // Without this, Unity would reuse the same user ID across editor windows, breaking testing.
        initializationOptions.SetProfile(playerName); 


        //Initializes Unity Gaming Services (UGS) using the profile we just set.
        // Required before using Authentication, Lobby, Relay
        // await means we wait for it to complete before moving forward.
        await UnityServices.InitializeAsync(initializationOptions);

        if(!AuthenticationService.Instance.IsSignedIn){
            // This registers a callback that runs once sign-in is complete.
            AuthenticationService.Instance.SignedIn += () => {
            
                // It logs the player ID for debugging.
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
                
                // to show available lobbies after the user signs in.
                RefreshLobbyList();
            };

            // Signs the player in anonymously, they don’t need an email or password.
            // This generates a unique player ID tied to the current profile.
            // await means it finishes signing in before continuing.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        } else {
            Debug.Log("Already signed in as " + AuthenticationService.Instance.PlayerId);
            RefreshLobbyList(); // still refresh if re-entering menu
        }

    }

    /*
    private void HandleRefreshLobbyList() {
        //  ensures Unity Services (Lobby, Relay) are fully initialized.
        // makes sure the player has successfully signed in.
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn) {

            //Decreases the timer by the time since the last frame.
            refreshLobbyListTimer -= Time.deltaTime;

            // When the timer reaches 0, it’s time to refresh the lobby list again.
            if (refreshLobbyListTimer < 0f) {

                // Resets the timer back to 5 seconds so the loop can repeat.
                // This means we’ll refresh the lobby list every 5 seconds.
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyListTimer = refreshLobbyListTimerMax;

                // Calls the actual method that requests the latest list of lobbies from Unity’s Lobby service.
                RefreshLobbyList();
            }
        }
    }
    */


    // It's marked async because it uses await to make an asynchronous call to Unity Lobby.
    // Keeps the current lobby alive by sending a heartbeat to Unity Lobby service every 15 seconds.
    private async void HandleLobbyHeartbeat() {

        // This check ensures that only the host sends heartbeat pings.
        if (IsLobbyHost()) {

            // This line decreases the heartbeatTimer by the time since the last frame.
            heartbeatTimer -= Time.deltaTime;

            // Once the timer reaches zero, it’s time to send a heartbeat to Unity.
            if (heartbeatTimer < 0f) {

                // Resets the timer to 15 seconds.
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                Debug.Log("Heartbeat");

                // Sends the actual heartbeat to Unity’s Lobby backend.
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }


    private async void HandleLobbyPolling() {

        // Only poll if the player is currently in a lobby.
        if (joinedLobby != null) {

            // Count down the polling timer 
            lobbyPollTimer -= Time.deltaTime;

            // If it's time to poll again
            if (lobbyPollTimer < 0f) {

                // Reset the timer so it waits another second before polling again.
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;
                
                // Contact Unity's Lobby service to get the latest info about the current lobby.
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                // Notify any UI components (like LobbyUI.cs) that the lobby data has been updated.
                // This can trigger updates to the player list or lobby name in the UI.
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                // Checks if the local player was kicked (their ID no longer appears in the player list).
                if (!IsPlayerInLobby()) {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");
 
                    // Notify the UI/system that this player was kicked.
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    // Clear the local joinedLobby so polling and heartbeats stop.
                    joinedLobby = null;
                }


                // Check if the host started the game by updating a lobby-wide metadata value.
                if (joinedLobby.Data[KEY_START_GAME].Value != "0"){

                    // If you're a client and not the host, use the Relay code to join the host's session.
                    if (!IsLobbyHost()){ 
                        TestRelay.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                    }

                    // Clear joinedLobby so polling ends.
                    joinedLobby = null;

                    // Notify listeners (likely LobbyUI or scene manager) that it's time to transition to gameplay.
                    OnGameStarted?.Invoke(this, (LobbyEventArgs)EventArgs.Empty);
                }
            }
        }
    }

    // Returned the lobby we joined
    public Lobby GetJoinedLobby() {
        return joinedLobby;
    }

    // checks if our player ID matches the host's ID.
    public bool IsLobbyHost() {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    
    // helper method to check if a player is still part of the lobby
    private bool IsPlayerInLobby() {
        //joined lobby and list of players are not null
        if (joinedLobby != null && joinedLobby.Players != null) {
            
            // Loops through every Player object in the lobby’s player list.
            foreach (Player player in joinedLobby.Players) {

                // Checks if the current player's ID matches the local player's ID,
                // we know the local player is still in the lobby
                if (player.Id == AuthenticationService.Instance.PlayerId) {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }
    
    // This method returns a Player object (from Unity Lobby).
    // used anytime you want to create or join a lobby.
    private Player GetPlayer() {
        // Parameters: 
        // AuthenticationService.Instance.PlayerId - gets the unique player ID assigned during sign-in.
        // null - This would be the player’s connection data which we don’t need to manually set here, so it’s left as null.
        // new Dictionary<string, PlayerDataObject> - This is the metadata dictionary attached to the player.
        // KEY_PLAYER_NAME is a constant string "PlayerName" declared on top of program
        // PlayerDataObject.VisibilityOptions.Public makes it visible to everyone in the lobby (so others can see your name)
        // playerName is the actual string the player typed
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }//,
        });
    }

    // creates the lobby on Unity’s servers
    public async void CreateLobby(string lobbyName){ //int maxPlayers, bool isPrivate){ 

        // max number of players allowed in this lobby.
        int maxPlayers = 4;

        // Calls GetPlayer() method to create a Player object.
        Unity.Services.Lobbies.Models.Player player = GetPlayer();

        // Prepares extra configuration for the lobby:
        CreateLobbyOptions options = new CreateLobbyOptions {

            // sets this player as the host when the lobby is created.
            Player = player,

            //IsPrivate = isPrivate,

            // metadata shared across the whole lobby.
            // KEY_START_GAME is used to indicate whether the host has started the game.
            // VisibilityOptions.Member means only players in the lobby can see this data.
            // It's initialized to "0" to mean “game not started yet.”
            Data = new Dictionary<string, DataObject>{
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0")}
            }
        };

       // Calls Unity’s Lobby Service API to create the lobby online.
       // We pass in the lobby name to display, player capacity, metadata and host info
       // Waits for the operation to complete and stores the result in lobby.
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers , options); //maxPlayers, options);

        joinedLobby = lobby;

        // Triggers an event so any UI scripts (like LobbyUI) can update.
        // Notifies listeners that we have created a lobby.
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        Debug.Log("Created Lobby " + lobby.Name);
    }


    public async void RefreshLobbyList() {
        try {

            // Creates a new set of search options for how we want to query lobbies.
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            // Limits the number of lobbies returned to 25 (to avoid overloading the UI).
            options.Count = 25;

            // Filter for open lobbies only
            //  filter so we only get lobbies with at least 1 open player slot.
            // AvailableSlots is the field
            // GT means “greater than”
            // "0" means don’t show full lobbies
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };


            // Order by newest lobbies first
            // false means descending order.
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            // Sends the actual request to Unity Lobby Service.
            // Waits (await) for a list of available lobbies to come back.
            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            // If there are any subscribers (like LobbyListUI), this triggers the event and sends the lobby list to update the UI.
            // response.Results contains the full list of lobbies returned.
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });

        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    /*
    public async void JoinLobbyByCode(string lobbyCode) {
        Player player = GetPlayer();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions {
            Player = player
        });

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }
    */

    // called from LobbyListSingleUI.cs when a player clicks on a lobby in the UI.
    public async void JoinLobby(Lobby lobby) {

        // create a Player object for the player trying to join.
        Player player = GetPlayer();

        // Sends a request to Unity Lobby to join this lobby by its unique ID.
        // Passes in the player info so Unity knows who is joining.
        // When the response comes back, it assigns the returned Lobby object to joinedLobby
        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions {
            Player = player
        });

        //Notifies all subscribed systems (like LobbyUI) that a lobby has been joined.
        // This triggers a UI transition to the lobby screen,
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
    }

    public async void UpdatePlayerName(string playerName) {
        this.playerName = playerName;

        // Only proceed if the player is currently in a lobby.
        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                // Builds a new metadata dictionary.
                //  The key is your custom "PlayerName" constant.
                // The value is a new PlayerDataObject containing the updated name.
                // Visibility is set to Public, so all players in the lobby can see the name.
                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_NAME, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerName)
                    }
                };

                // Fetches the local player’s Unity-generated ID so you can tell the server which player to update.
                string playerId = AuthenticationService.Instance.PlayerId;

                //Calls Unity Lobby to update the player in the current lobby.
                // Uses your lobby ID and player ID to locate and update the record.
                // await waits for the response from Unity.
                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);

                //Saves the updated lobby object Unity returns.
                joinedLobby = lobby;

                // Notifies the UI that the lobby data has changed.
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    /*
    public async void QuickJoinLobby() {
        try {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
    */


    public async void LeaveLobby() {

        // This line ensures the player is currently in a lobby before trying to leave.
        if (joinedLobby != null) {
            try {

                // it tells Unity Lobby Service to remove the local player from the lobby.
                // joinedLobby.Id: the unique ID of the lobby you're leaving.
                // AuthenticationService.Instance.PlayerId: the unique player ID.
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                // no longer part of a lobby
                joinedLobby = null;

                // Update the UI by hiding the lobby screen
                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    // It takes the playerId of the player to be kicked.
    public async void KickPlayer(string playerId) {

        // Uses your IsLobbyHost() helper method to check if the local player is the host.
        if (IsLobbyHost()) {
            try {

                // Sends a request to Unity Lobby Service to remove another player from the lobby.
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    /**
     * Returns the player to the Main Menu scene.
     */
    public void ReturnMenu(){
        SceneManager.LoadScene("Main Menu");
    }



    public async void StartGame(){

        // Only the host is allowed to start the game.
        if (IsLobbyHost()) {
            try{
                Debug.Log("StartGame");

                // It calls the TestRelay.CreateRelay() method.
                // that method allocates a Relay server and returns a unique joinCode
                string relayCode = await TestRelay.Instance.CreateRelay();

                // Updates the lobby metadata to include the new Relay code.
                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions{
                    
                    // The key is a string
                    // The value is a DataObject
                    // KEY_START_GAME is a custom string constant
                    // Visibility is set to Member, meaning only players in the lobby can see the Relay code
                    Data = new Dictionary<string, DataObject>{
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                    }
                });

                // Stores the updated lobby data (with the Relay code embedded) locally.
                joinedLobby = lobby;
            } catch (LobbyServiceException e){
                Debug.Log(e);
            }
        }
    }



}
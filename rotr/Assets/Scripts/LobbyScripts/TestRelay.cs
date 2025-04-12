using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

// TestRelay handles all the logic for setting up a Relay multiplayer session.
// It configures UnityTransport for the host or client and loads the game scene.
public class TestRelay : MonoBehaviour
{
    // make the relay handler accessible globally
    public static TestRelay Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    /*
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    */

    // Creates a new Relay allocation for hosting a multiplayer session.
    // Configures the UnityTransport with Relay data and starts hosting.
    public async Task<string> CreateRelay(){
        try {
            // Allocate a Relay server for up to 3 clients (4 total players)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            
            // Get a join code for clients to use
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            
            // Configure Unity Transport (UTP) with host-specific Relay data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort) allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Start hosting and load the In-Game scene for the host
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("In-Game", LoadSceneMode.Single); // Load the game scene for the host


            return joinCode;
        } catch (RelayServiceException exception){
            Debug.Log(exception);
            return null;
        }
    }

    // Joins a Relay session using a provided join code.
    // Configures UnityTransport with client-specific Relay data and connects to host.
    public async void JoinRelay(string joinCode){
        try {
            Debug.Log("Joining Relay with " + joinCode);

            // Request Relay join allocation from Unity's Relay Service
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Configure Unity Transport (UTP) with client-specific Relay data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort) joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // Start client and load the In-Game scene
            NetworkManager.Singleton.StartClient();
            NetworkManager.Singleton.SceneManager.LoadScene("In-Game", LoadSceneMode.Single);


        } catch (RelayServiceException e){
            Debug.Log(e);
        }
    }

}

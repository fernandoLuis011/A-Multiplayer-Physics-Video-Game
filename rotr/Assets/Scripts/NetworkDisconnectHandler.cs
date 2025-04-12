using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

// Handles the scenario where a client loses connection to the host.
// Automatically transitions the client back to the lobby menu if the host disconnects.
public class NetworkDisconnectHandler : MonoBehaviour
{

    // Called when the object becomes enabled and active.
    // Subscribes to the disconnect callback from Netcode.
    private void OnEnable()
    {   
        Debug.Log("NetworkDisconnectHandler enabled 1");

        // Subscribe to the client disconnect callback if NetworkManager is initialized
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect;
    }


    // Called when any client disconnects from the network.
    // If the local player is not the host and we detect a disconnect, start the return process.
    private void HandleDisconnect(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}, host = {NetworkManager.ServerClientId}");

        // Only clients (not the host) should respond to losing connection to the host
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Lost connection to host. Returning to Lobby Menu...");
            StartCoroutine(ReturnToLobbiesAfterDelay());
        }
    }


    // Coroutine to wait briefly before transitioning to the lobby scene.
    // This delay ensures that Netcode finishes its shutdown processes cleanly.
    private IEnumerator ReturnToLobbiesAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Small buffer to allow shutdown to finish
        Debug.Log("Loading LobbiesMenu scene...");
        SceneManager.LoadScene("LobbiesMenu");
       
    }

    
    // Called when the object is disabled or destroyed.
    ///Unsubscribes from the disconnect callback to avoid duplicate events.
    private void OnDisable()
    {
        Debug.Log("NetworkDisconnectHandler enabled 2");
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleDisconnect;
    }
}

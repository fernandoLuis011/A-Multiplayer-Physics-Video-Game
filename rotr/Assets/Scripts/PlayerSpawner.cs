using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab; // Assign the Player Prefab in the Inspector

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Host or Dedicated Server
        {
            SpawnPlayer(OwnerClientId); // Spawns the Host Player
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer; // Spawn new clients
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return; // Only the Server can spawn players

        GameObject playerInstance = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5f, 5f), 1f, 0f); // Spawn players in different positions
    }
}

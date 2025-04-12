using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField ] private GameObject playerPrefab; // Player prefab reference
    [SerializeField ] private Transform[] spawnPoints; // Array of possible spawn points


    private void Start()
    {
        // Listen for when a client connects
        OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
    {
        // Pick a random spawn point from the array
        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject playerInstance = Instantiate(playerPrefab, randomSpawn.position, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
    }
}


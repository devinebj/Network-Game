using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class MainArea : NetworkBehaviour {
    [SerializeField] private Player hostPrefab;
    [SerializeField] private Player clientPrefab;
    [SerializeField] private Camera areaCamera;
    private int positionIndex;
    private NetworkedPlayers networkedPlayers;
    
    private Vector3[] startPositions = new Vector3[] {
        new Vector3(4, 2, 0),
        new Vector3(-4, 2, 0),
        new Vector3(0, 2, 4),
        new Vector3(0, 2, -4)
    };

    private void Start() {
        areaCamera.enabled = false;
        areaCamera.GetComponent<AudioListener>().enabled = !IsClient;
        networkedPlayers = GameObject.Find("NetworkedPlayers").GetComponent<NetworkedPlayers>();
        
        if (IsServer) {
            SpawnPlayers();
        }
    }

    private void Update() {
        EnforceBoundary();
    }

    private Vector3 NextPostition() {
        Vector3 pos = startPositions[positionIndex % startPositions.Length];
        positionIndex++;
        return pos;
    }

    private void SpawnPlayers() {
        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers) {
            Player playerPrefab = hostPrefab;
            Player playerSpawn = Instantiate(playerPrefab, NextPostition(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.ClientId);
            playerSpawn.playerColor = info.Color;
        }
    }

    private void EnforceBoundary() {
        if (!IsServer) { return; }
        
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds) {
            if (clientId == NetworkManager.LocalClientId) { continue; }
            
            NetworkClient client = NetworkManager.ConnectedClients[clientId];
            if (client?.PlayerObject is null) {
                print("PlayerObject is null");
                continue;
            }

            Player player = client.PlayerObject.GetComponent<Player>();
            if (player is null) {
                print("Player component is null");
                continue;
            }

            Vector3 position = player.transform.position;
            Vector3 clampedPosition = position;

            clampedPosition.x = Mathf.Clamp(position.x, -5, 5);
            clampedPosition.z = Mathf.Clamp(position.z, -5, 5);

            if (clampedPosition != position) {
                player.transform.position = clampedPosition;
            }
        }
    }
}
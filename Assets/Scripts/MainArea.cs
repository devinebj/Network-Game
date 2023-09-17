using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class MainArea : NetworkBehaviour {
    [SerializeField] private Player HostPrefab;
    [SerializeField] private Player ClientPrefab;
    [SerializeField] private Camera areaCamera;
    private int positionIndex;

    private Vector3[] startPositions = new Vector3[] {
        new Vector3(4, 2, 0),
        new Vector3(-4, 2, 0),
        new Vector3(0, 2, 4),
        new Vector3(0, 2, -4)
    };

    private int colorIndex = 0;
    private Color[] playerColors = new Color[] {
        Color.red,
        Color.yellow,
        Color.green,
        Color.blue
    };

    private void Start() {
        areaCamera.enabled = false;
        areaCamera.GetComponent<AudioListener>().enabled = !IsClient;
        
        if (IsServer) { SpawnPlayers(); }
    }

    private void Update() {
        foreach(ulong clientId in NetworkManager.ConnectedClientsIds) {
            if (clientId != NetworkManager.LocalClientId) { EnforceBoundary(); }
        }
    }

    private Color NextColor() {
        Color newColor = playerColors[colorIndex % playerColors.Length];
        colorIndex++;
        return newColor;
    }

    private Vector3 NextPostition() {
        Vector3 pos = startPositions[positionIndex % startPositions.Length];
        positionIndex++;
        return pos;
    }

    private void SpawnPlayers() {
        foreach(ulong clientId in NetworkManager.ConnectedClientsIds) {
            Player playerPrefab = (clientId == NetworkManager.LocalClientId) ? HostPrefab : ClientPrefab;
            Player playerSpawn = Instantiate(playerPrefab, NextPostition(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            playerSpawn.playerColorNetVar.Value = NextColor();
        }
    }

    private void EnforceBoundary() {
        Vector3 position = transform.position;

        if (position.x > 5) { position.x = 5; }
        if (position.x < -5) { position.x = -5; }
        if (position.z > 5) { position.z = 5; }
        if (position.z < 5) { position.z = -5; }
    }
}

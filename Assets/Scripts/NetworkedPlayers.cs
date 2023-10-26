using UnityEngine;
using Unity.Netcode;

public class NetworkedPlayers : NetworkBehaviour {
    public NetworkList<NetworkPlayerInfo> allNetPlayers;

    private int colorIndex = 0;
    private Color[] playerColors = new Color[] {
        Color.red,
        Color.yellow,
        Color.green,
        Color.blue
    };
    
    private void Awake() {
        allNetPlayers = new NetworkList<NetworkPlayerInfo>();
    }
    
    void Start() {
        DontDestroyOnLoad(this.gameObject);
        if (IsServer) {
            ServerStart();
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        }
    }
    
    void ServerStart() {
        NetworkPlayerInfo host = new NetworkPlayerInfo(NetworkManager.LocalClientId);
        host.Ready = true;
        host.Color = NextColor();
        host.PlayerName = "The Host";
        allNetPlayers.Add(host);
    }

    private void ServerOnClientConnected(ulong clientId) {
        NetworkPlayerInfo client = new NetworkPlayerInfo(clientId);
        client.Ready = false;
        client.Color = NextColor();
        client.PlayerName = $"Player {clientId}";
        allNetPlayers.Add(client);
    }

    private void ServerOnClientDisconnected(ulong clientId) {
        var idx = FindPlayerIndex(clientId);
        if (idx != -1) {
            allNetPlayers.RemoveAt(idx);
        }
    }
    
    private Color NextColor() {
        Color newColor = playerColors[colorIndex % playerColors.Length];
        colorIndex++;
        return newColor;
    }

    public int FindPlayerIndex(ulong clientId) {
        var idx = 0;
        var found = false;

        while (idx < allNetPlayers.Count && !found) {
            if (allNetPlayers[idx].ClientId == clientId) {
                found = true;
            } else {
                idx++;
            }
        }
        
        if (!found) { idx--; }
        return idx;
    }

    public void UpdateReady(ulong clientId, bool ready) {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1) { return; }

        NetworkPlayerInfo info = allNetPlayers[idx];
        info.Ready = ready;
        allNetPlayers[idx] = info;
    }

    public void UpdatePlayerName(ulong clientId, string playerName) {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1) { return; }

        NetworkPlayerInfo info = allNetPlayers[idx];
        info.PlayerName = playerName;
        allNetPlayers[idx] = info;
    }
    
    public NetworkPlayerInfo GetMyInfo() {
        NetworkPlayerInfo toReturn = new NetworkPlayerInfo(ulong.MaxValue);
        int idx = FindPlayerIndex(NetworkManager.LocalClientId);
      
        if (idx != -1) { toReturn = allNetPlayers[idx]; }
        return toReturn;
    }

    public bool AllPlayersReady() {
        bool theyAre = true;
        int idx = 0;

        while (theyAre && idx < allNetPlayers.Count) {
            theyAre = allNetPlayers[idx].Ready;
            idx++;
        }

        return theyAre;
    }
}
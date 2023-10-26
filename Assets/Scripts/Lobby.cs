using UnityEngine;
using Unity.Netcode;

public class Lobby : NetworkBehaviour {
    public LobbyUi LobbyUi;
    public NetworkedPlayers NetworkedPlayers;

    void Start() {
        if (IsServer) {
            ServerPopulateCards();
            NetworkedPlayers.allNetPlayers.OnListChanged += ServerOnNetworkedPlayersChanged;
            LobbyUi.ShowStart(true);
            LobbyUi.OnStartClicked += ServerStartClicked;
        }
        else {
            ClientPopulateCards();
            NetworkedPlayers.allNetPlayers.OnListChanged += ClientNetPlayersChanged;
            LobbyUi.ShowStart(false);
            LobbyUi.OnReadyToggled += ClientOnReadyToggled;
            NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnect;
        }

        LobbyUi.OnChangeNameClicked += OnChangeNameClicked;
    }

    private void OnChangeNameClicked(string newValue) {
        UpdatePlayerNameServerRpc(newValue);
    }

    private void ClientNetPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent) {
        ClientPopulateCards();
        PopulateMyInfo();
    }

    private void ServerOnNetworkedPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent) {
        ServerPopulateCards();
        PopulateMyInfo();
        LobbyUi.EnableStart(NetworkedPlayers.AllPlayersReady());
    }

    private void ServerStartClicked() {
        NetworkManager.SceneManager.LoadScene("MainArea", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void ServerOnKickClicked(ulong clientId) {
        NetworkManager.DisconnectClient(clientId);
    }

    private void PopulateMyInfo() {
        NetworkPlayerInfo myInfo = NetworkedPlayers.GetMyInfo();
        if (myInfo.ClientId != ulong.MaxValue) {
            LobbyUi.SetPlayerName(myInfo.PlayerName.ToString());
        }
    }

    private void ServerPopulateCards() {
        LobbyUi.playerCards.Clear();

        foreach (NetworkPlayerInfo info in NetworkedPlayers.allNetPlayers) {
            PlayerCard pc = LobbyUi.playerCards.AddCard("Some player");
            pc.clientId = info.ClientId;
            pc.ready = info.Ready;
            pc.color = info.Color;
            pc.playerName = info.PlayerName.ToString();
            if (info.ClientId == NetworkManager.LocalClientId) {
                pc.ShowKick(false);
            } else {
                pc.ShowKick(true);
            }

            pc.OnKickClicked += ServerOnKickClicked;
            pc.UpdateDisplay();
        }
    }

    private void ClientPopulateCards() {
        LobbyUi.playerCards.Clear();

        foreach (NetworkPlayerInfo info in NetworkedPlayers.allNetPlayers) {
            PlayerCard pc = LobbyUi.playerCards.AddCard("Some player");
            pc.ready = info.Ready;
            pc.color = info.Color;
            pc.playerName = info.PlayerName.ToString();
            pc.clientId = info.ClientId;
            pc.ShowKick(false);
            pc.UpdateDisplay();
        }
    }

    private void ClientOnReadyToggled(bool newValue) {
        UpdateReadyServerRpc(newValue);
    }

    private void ClientOnClientDisconnect(ulong clientId) {
        LobbyUi.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyServerRpc(bool newValue, ServerRpcParams serverRpcParams = default) {
        NetworkedPlayers.UpdateReady(serverRpcParams.Receive.SenderClientId, newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerNameServerRpc(string newValue, ServerRpcParams rpcParams = default) {
        NetworkedPlayers.UpdatePlayerName(rpcParams.Receive.SenderClientId, newValue);
    }
}
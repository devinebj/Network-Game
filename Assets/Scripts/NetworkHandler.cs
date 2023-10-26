using Unity.Netcode;
using UnityEngine;

public class NetworkHandler : NetworkBehaviour {
    private bool hasPrinted = false;
    
    void Start() {
        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
    }

    private void PrintMe() {
        if (hasPrinted) return;

        Debug.Log("I AM");
        hasPrinted = true;

        if (IsServer) { Debug.Log($" the Server! {NetworkManager.ServerClientId}"); }
        if (IsHost) { Debug.Log($" the Host! {NetworkManager.ServerClientId}/{NetworkManager.LocalClientId}"); }
        if (IsClient) { Debug.Log($" a Client! {NetworkManager.LocalClientId}"); }
        if (!IsServer && !IsClient) {
            Debug.Log("nothing yet...");
            hasPrinted = false;
        }
    }

    #region Client Actions
    private void OnClientStarted() {
        Debug.Log("!! Client Started !!");
        NetworkManager.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnected;
        NetworkManager.OnClientStopped += ClientOnClientStopped;
    }

    private void ClientOnClientConnected(ulong clientId) {
        if (!IsServer) Debug.Log($"I {clientId} have connected to the server");
        else Debug.Log("Some other client connected");
    }

    private void ClientOnClientDisconnected(ulong clientId) {
        Debug.Log($"I {clientId} have discconnected from the server");

    }
    
    private void ClientOnClientStopped(bool indicator) {
        Debug.Log("!! Client Stopped !!");
        hasPrinted = false;
        NetworkManager.OnClientConnectedCallback -= ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ClientOnClientDisconnected;
        NetworkManager.OnClientStopped -= ClientOnClientStopped;
    }
    #endregion 

    #region Server Actions
    private void OnServerStarted() {
        Debug.Log("!! Server Started !!");
        NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        NetworkManager.OnServerStopped += ServerOnServerStopped;
    }

    private void ServerOnClientConnected(ulong clientId) {
        Debug.Log($"Client {clientId} connected to the server");
    }

    private void ServerOnClientDisconnected(ulong clientId) {
        Debug.Log($"Client {clientId} disconnected from the server");
    }

    private void ServerOnServerStopped(bool indicator) {
        hasPrinted = false;
        NetworkManager.OnClientConnectedCallback -= ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ServerOnClientDisconnected;
        NetworkManager.OnServerStopped -= ServerOnServerStopped;
    }
    #endregion
}

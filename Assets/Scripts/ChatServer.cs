using UnityEngine;
using Unity.Netcode;
using System;

public class ChatServer : NetworkBehaviour {
	[SerializeField] private ChatUi chatUi;
	const ulong SYSTEM_ID = ulong.MaxValue;
	private ulong[] dmClientIds = new ulong[2];
	private void Start() {
		chatUi.printEnteredText = false;
		chatUi.MessageEntered += OnChatUiMessageEntered;

		if (IsHost) {
			DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}");
		}
		if (IsServer) {
			DisplayMessageLocally(SYSTEM_ID, "You are the server");
			NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
			NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
		}
		if (IsClient) {
			DisplayMessageLocally(SYSTEM_ID, $"You are a client {NetworkManager.LocalClientId}");
		}
	}

	private void ServerOnClientConnected(ulong clientId) {
		BroadcastMessage($"{clientId} has connected to the server.");

		if (clientId == NetworkManager.LocalClientId) { return; }
		ServerSendDirectMessage($"Welcome to the server {clientId}", NetworkManager.ServerClientId, clientId);
	}

	private void ServerOnClientDisconnected(ulong clientId) {
		BroadcastMessage($"{clientId} has disconnected from the server.");
	}

	private void DisplayMessageLocally(ulong from, string message) {
		string fromString = $"Player {from}";
		Color textColor = chatUi.defaultTextColor;

		if (from == NetworkManager.LocalClientId) {
			fromString = "You";
			textColor = Color.magenta;
		} else if (from == SYSTEM_ID) {
			fromString = "SYS";
			textColor = Color.green;
		}

		chatUi.addEntry(fromString, message, textColor);
	}

	private void OnChatUiMessageEntered(string message) {
		SendChatMessageServerRpc(message);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default) {
		if (message.StartsWith("@")) {
			string[] parts = message.Split(" ");
			string clientIdString = parts[0].Replace("@", "");
			ulong toClientId = ulong.Parse(clientIdString);

			ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);
		} else {
			ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
		}
	}

	[ClientRpc]
	public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default) {
		DisplayMessageLocally(from, message);
	}

	private void BroadcastMessage(string message) {
		ReceiveChatMessageClientRpc(message, NetworkManager.ServerClientId);
	}

	private void ServerSendDirectMessage(string message, ulong from, ulong to) {
		if (!NetworkManager.ConnectedClients.ContainsKey(to)) {
			ServerSendDirectMessage($"Failed to send message: Client {to} doesn't exist", NetworkManager.ServerClientId, from);
			return;
		}
		
		dmClientIds[0] = from;
		dmClientIds[1] = to;

		ClientRpcParams rpcParams = default;
		rpcParams.Send.TargetClientIds = dmClientIds;

		if (from == NetworkManager.ServerClientId) {
			ReceiveChatMessageClientRpc($"{message}", from, rpcParams);
		} else {
			ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
		}
	}
}

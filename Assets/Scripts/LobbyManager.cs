using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour {
    [SerializeField] Button startButton;
    [SerializeField] public TMPro.TMP_Text statusLabel;

    void Start() {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Not connected to anything";

        startButton.onClick.AddListener(OnStartButtonClicked);

        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
    }

    private void OnStartButtonClicked() {
        StartGame();
    }

    public void GoToLobby() {
        NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void StartGame() {
        NetworkManager.SceneManager.LoadScene("MainArea", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void OnServerStarted() {
        GoToLobby();
    }

    private void OnClientStarted() {
        if (!IsHost) {
            statusLabel.text = "Waiting for game to start";
        }
    }
}
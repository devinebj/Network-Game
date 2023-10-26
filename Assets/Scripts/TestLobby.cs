using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLobby : MonoBehaviour {
    public LobbyUi LobbyUi;

    void Start() {
        CreateTestCards();
        LobbyUi.OnReadyToggled += TestOnReadyToggled;
        LobbyUi.OnStartClicked += TestOnStartClicked;
        LobbyUi.ShowStart(true);
        LobbyUi.OnChangeNameClicked += TestOnChangedNameClicked;
    }

    private void CreateTestCards() {
        PlayerCard pc = LobbyUi.playerCards.AddCard("Test Player 1");
        pc.color = Color.gray;
        pc.ready = true;
        pc.ShowKick(true);
        pc.clientId = 99;
        pc.OnKickClicked += TestOnKickClicked;
        pc.UpdateDisplay();
        
        pc = LobbyUi.playerCards.AddCard("Test Player 2");
        pc.color = Color.green;
        pc.ready = false;
        pc.ShowKick(true);
        pc.clientId = 50;
        pc.OnKickClicked += TestOnKickClicked;
        pc.UpdateDisplay();
    }

    private void TestOnKickClicked(ulong clientId) {
        Debug.Log($"We wanna kick client {clientId}");
    }

    private void TestOnReadyToggled(bool newValue) {
        Debug.Log($"Ready = {newValue}");
    }

    private void TestOnStartClicked() {
        LobbyUi.ShowStart(false);
    }
    
    private void TestOnChangedNameClicked(string newName) {
        Debug.Log($"New name = {newName}");
    }
}
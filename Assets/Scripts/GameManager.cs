using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    void Start() {
        if (Application.isEditor) {
        } else {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }
    }

    private void OnGUI() {
        NetworkHelper.GUILayoutNetworkControls();
    }
}

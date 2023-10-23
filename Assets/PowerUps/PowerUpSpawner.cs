using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class PowerUpSpawner : NetworkBehaviour {
    public BasePowerUp powerUp;
    public float spawnDelay = 3.0f;

    private BasePowerUp spawnedPowerUp;
    private float timeSinceDespawn = 0f;

    private void Start() {
        if (IsServer) { SpawnPowerUp(); }
    }

    private void Update() {
        if (IsServer) {
            ServerHandlePowerUpRespawn();
        }
    }

    private void ServerHandlePowerUpRespawn() {
        if (spawnedPowerUp == null) {
            timeSinceDespawn += Time.deltaTime;

            if (timeSinceDespawn >= spawnDelay) {
                SpawnPowerUp();
                timeSinceDespawn = 0f;
            }
        }
}

    public void SpawnPowerUp() {
        if (!powerUp) { return; }
        
        Vector3 position = transform.position;
        position.y += 2;

        spawnedPowerUp = Instantiate(powerUp, position, transform.rotation);
        spawnedPowerUp.gameObject.GetComponent<NetworkObject>().Spawn();
    }
}
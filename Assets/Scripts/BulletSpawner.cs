using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletSpawner : NetworkBehaviour {
    [SerializeField] Rigidbody BulletPrefab;
    private float bulletSpeed = 50f;
    public float timeBetweenBullets = 0.5f;
    private float shotCountDown = 0f;

    private void Update() {
        if (IsServer) {
            if (shotCountDown > 0) {
                shotCountDown -= Time.deltaTime;
            }
        }
    }
    
    [ServerRpc]
    public void FireServerRpc(ServerRpcParams rpcParams = default) {
        if (shotCountDown > 0) { return; }
        
        Rigidbody newBullet = Instantiate(BulletPrefab, transform.position, transform.rotation);
        newBullet.velocity = transform.forward * bulletSpeed;
        newBullet.gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        Destroy(newBullet.gameObject, 3);

        shotCountDown = timeBetweenBullets;
    }
}

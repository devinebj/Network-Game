using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour {
    [SerializeField] private float movementSpeed = 50f;
    [SerializeField] private float rotationSpeed = 130f;
    [SerializeField] private MeshRenderer playerMeshRenderer;
    public BulletSpawner bulletSpawner;

    public Color playerColor = Color.red;
    public NetworkVariable<Color> playerColorNetVar = new(Color.red);
    public NetworkVariable<int> scoreNetVar = new(0);

    private GameObject playerBody;
    private Camera playerCamera;

    private void NetworkInit() {
        playerBody = transform.Find("PlayerBody").gameObject;
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;
        ApplyColor();

        if (IsClient) {
            scoreNetVar.OnValueChanged += ClientOnScoreValueChanged;
        }
    }
    
    private void Start() {
        NetworkHelper.Log(this, "Start");
    }

    private void Update() {
        if (IsOwner) {
            OwnerHandleInput();
            if (Input.GetButtonDown("Fire1")) {
                NetworkHelper.Log("Requesting Fire");
                bulletSpawner.FireServerRpc();
            }
        }
    }

    private void OwnerHandleInput() {
        Vector3 movement = CalcMovement();
        Vector3 rotation = CalcRotation();

        if (movement != Vector3.zero || rotation != Vector3.zero) {
            MoveServerRpc(CalcMovement(), CalcRotation());
        }
    }

    private void ApplyColor() {
        playerMeshRenderer.GetComponent<MeshRenderer>().material.color = playerColorNetVar.Value;
    }

    public override void OnNetworkSpawn() {
        NetworkHelper.Log(this, "OnNetworkSpawn");
        NetworkInit();
        base.OnNetworkSpawn();
    }

    private void ClientOnScoreValueChanged(int old, int current) {
        if (IsOwner) {
            NetworkHelper.Log(this, $"My score is {scoreNetVar.Value}");
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (IsServer) {
            ServerHandleCollision(collision);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (IsServer) {
            if (other.CompareTag("PowerUp")) {
                other.GetComponent<BasePowerUp>().ServerPickup(this);
            }
        }
    }

    private void ServerHandleCollision(Collision collision) {
        if (collision.gameObject.CompareTag("Bullet")) {
            ulong ownerId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            NetworkHelper.Log(this,
                $"Hit by {collision.gameObject.name}" +
                $"owned by {ownerId}");
            Player other = NetworkManager.Singleton.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
            other.scoreNetVar.Value += 1;
            collision.gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }

    public void OnPlayerColorChanged(Color previous, Color current) {
        ApplyColor();
    }
    
    [ServerRpc]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation) {
        transform.Translate(movement);
        transform.Rotate(rotation);
    }

    private Vector3 CalcRotation() {
        bool isShiftKeyDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        Vector3 rotVec = Vector3.zero;

        if(!isShiftKeyDown) {
            rotVec = rotationSpeed * Time.deltaTime * new Vector3(0, Input.GetAxis("Horizontal"), 0);
        }
        
        return rotVec;
    }

    private Vector3 CalcMovement() {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown) { x_move = Input.GetAxis("Horizontal"); }

        return movementSpeed * Time.deltaTime * new Vector3(x_move, 0, -z_move);
    }
}

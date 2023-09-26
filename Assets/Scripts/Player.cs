using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour {
    [SerializeField] private float movementSpeed = 50f;
    [SerializeField] private float rotationSpeed = 130f;
    [SerializeField] private MeshRenderer playerMeshRenderer;

    public Color playerColor = Color.red;
    public NetworkVariable<Color> playerColorNetVar = new NetworkVariable<Color>(Color.red);

    private Camera playerCamera;

    private void Start() {
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;
        ApplyColor();
    }

    private void Update() {
        if (IsOwner) { OwnerHandleInput(); } 
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

    [ServerRpc(RequireOwnership = true)]
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

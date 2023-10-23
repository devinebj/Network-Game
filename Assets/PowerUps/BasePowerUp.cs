using Unity.Netcode;

public abstract class BasePowerUp : NetworkBehaviour {
    public void ServerPickup(Player thePickerUpper) {
        if (IsServer) {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    protected abstract bool ApplyToPlayer(Player thePickerUpper);
}
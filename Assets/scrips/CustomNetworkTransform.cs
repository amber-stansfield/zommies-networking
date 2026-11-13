using Unity.Netcode;
using UnityEngine;

public class CustomNetworkTransform : NetworkBehaviour
{
    private Vector3 lastPosition, currentPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!IsOwner) { return; }
        currentPosition = this.transform.position;
        setPositionServerRpc(currentPosition);
    }

    [ServerRpc]
    void setPositionServerRpc(Vector3 position)
    {
        this.transform.position = position;
        setPositionClientRpc(position);
    }


    [ClientRpc]
    void setPositionClientRpc(Vector3 position)
    {
        this.transform.position = position;
    }
}

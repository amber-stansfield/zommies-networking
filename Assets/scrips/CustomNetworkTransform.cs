using System.Numerics;
using Unity.Netcode;
using UnityEditor.Animations.Rigging;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomNetworkTransform : NetworkBehaviour
{
    private UnityEngine.Vector3 lastPosition, currentPosition;
    private UnityEngine.Quaternion lastRotation, currentRotation;

    float tolerance = 12f;
    float rotationTolerance = 0.99f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        currentPosition = this.transform.position;
        setPositionServerRpc(currentPosition);

        currentRotation = this.transform.rotation;
        setRotationServerRpc(currentRotation);
    }

    [ServerRpc]
    void setPositionServerRpc(UnityEngine.Vector3 position)
    {
        this.transform.position = position;
        setPositionClientRpc(position);
    }


    [ClientRpc]
    void setPositionClientRpc(UnityEngine.Vector3 position)
    {

        if (IsLocalPlayer && UnityEngine.Vector3.Distance(this.transform.position, position) < tolerance)
        {
            return;
        }

        this.transform.position = UnityEngine.Vector3.Lerp(this.transform.position,position,0.4f);
    }

    [ServerRpc]
    void setRotationServerRpc(UnityEngine.Quaternion rotation)
    {
        this.transform.rotation = rotation;
        setRotationClientRpc(rotation);
    }


    [ClientRpc]
    void setRotationClientRpc(UnityEngine.Quaternion rotation)
    {
        if (IsLocalPlayer /*&& (UnityEngine.Quaternion.Dot(this.transform.rotation,rotation) > Mathf.Abs(rotationTolerance))*/)
        {
            return;
        }
        this.transform.rotation = UnityEngine.Quaternion.Lerp(this.transform.rotation,rotation,0.3f);
    }
}

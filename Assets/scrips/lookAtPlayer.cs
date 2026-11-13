using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class lookAtPlayer : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            this.transform.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner)
        {
            this.transform.LookAt(NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetChild(1).transform.position);
        }
    }
}

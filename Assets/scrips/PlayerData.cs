using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public string playerName;
    public string playerID;
    public Color playerColour;

    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

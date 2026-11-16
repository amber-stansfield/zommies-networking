using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static Unity.U2D.Physics.PhysicsLayers;





public class serverManager : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI joinCodeDisplay;
    [SerializeField] TMP_InputField joinCodeInput;
    private NetworkManager m_NetworkManager;
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TextMeshProUGUI inGameUserList;

    [SerializeField] GameObject startButton;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject mainMenuHolder;
    [SerializeField] GameObject subMenuHolder;

    [SerializeField] GameObject playerData;

    [SerializeField] Transform colourSelection, exampleImage;

    public Color selectedColour = new Color(0, 0, 0);

    public class PlayerData
    {
        public string name;
        public ulong clientID;
        public Color Colour;
    }
    Dictionary<ulong, PlayerData> players = new();

    //public bool raceInProgress;
    //public GameObject raceStartDude;
    //public GameObject raceEndDude;

    //public NativeArray<FixedString32Bytes> names = new NativeArray<FixedString32Bytes>(8, Allocator.Persistent);
    //public Dictionary<int, string> names = new Dictionary<int, string>();
    private void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();

    }

    private async void Start()
    {
        DontDestroyOnLoad(this);
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F))
        {

        }
    }

    public async void JoinRelay()
    {
        if (usernameInput.text == null || usernameInput.text.Length < 3 || usernameInput.text.Length > 10 || selectedColour == new Color(0, 0, 0))
        {
            return;
        }
        await StartClientWithRelay(joinCodeInput.text);
        mainMenuHolder.SetActive(false);
        subMenuHolder.SetActive(true);

        await AuthenticationService.Instance.UpdatePlayerNameAsync(usernameInput.text);
        string playerName = AuthenticationService.Instance.PlayerName.Substring(0, AuthenticationService.Instance.PlayerName.Length - 6);
        Debug.Log($"clap {playerName}");
        StartCoroutine(addPlayer(playerName));
        //AddToUserListServerRpc();
    }


    public async void StartRelay()
    {
        if (usernameInput.text == null || usernameInput.text.Length < 3 || usernameInput.text.Length > 10)
        {
            return;
        }
        string joinCode = await StartHostWithRelay();
        joinCodeDisplay.text = joinCode;
        mainMenuHolder.SetActive(false);
        startButton.SetActive(true);
        subMenuHolder.SetActive(true);

        string playerName = AuthenticationService.Instance.PlayerName.Substring(0, AuthenticationService.Instance.PlayerName.Length - 6);
        players[NetworkManager.Singleton.LocalClientId] = new PlayerData
        {
            clientID = NetworkManager.Singleton.LocalClientId,
            name = playerName,
            Colour = selectedColour
        };

        inGameUserList.text += $"\n{players[0].name}";
        Debug.Log($" utter gaming {players[NetworkManager.Singleton.LocalClientId].clientID}");
    }

    private async Task<string> StartHostWithRelay(int maxConnections = 3)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        var relayServerData = allocation.ToRelayServerData("udp");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        await AuthenticationService.Instance.UpdatePlayerNameAsync(usernameInput.text);

        //players[NetworkManager.Singleton.LocalClientId] = new PlayerData
        //{
        //    clientID = NetworkManager.Singleton.LocalClientId,
        //    name = playerName,
        //    Colour = selectedColour
        //};


        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void updateInGameUserlistServerRpc(string name)
    {
        inGameUserList.text += name;
        updateInGameUserlistClientRpc(name);
    }
    [ClientRpc]
    void updateInGameUserlistClientRpc(string name)
    {
        inGameUserList.text += name;
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = joinAllocation.ToRelayServerData("udp");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData((relayServerData));
        mainMenuHolder.SetActive(false);
        subMenuHolder.SetActive(true);
        //askforConnectedClientsServerRpc(name);

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();

    }

    IEnumerator addPlayer(string name)
    {
        if (IsServer || IsHost)
        {
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        //askforConnectedClientsServerRpc(input);
        AddUserDataServerRpc(name, selectedColour);
    }

    //[ServerRpc(RequireOwnership = false)]
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void AddUserDataServerRpc(string name, Color colour, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        players[senderId] = new PlayerData
        {
            clientID = senderId,
            name = name,
            Colour = colour
        };

        Debug.Log($" utter gaming {players[senderId].clientID}");
        sendPlayerList();
    }

    void sendPlayerList()
    {
        ulong[] ids = players.Keys.ToArray();
        FixedString32Bytes[] names = players.Values.Select(x => new FixedString32Bytes(x.name)).ToArray();
        Color[] colours = players.Values.Select(x => x.Colour).ToArray();

        RebuildPlayerListRpc(ids, names, colours);

    }
    
    //[ClientRpc]
    //void AddUserDataClientRpc(ulong clientId, string playerName, Color colour)
    //{
    //    if (IsHost) { return; }
    //    players[clientId] = new PlayerData
    //    {
    //        clientID = clientId,
    //        name = playerName,
    //        Colour = colour
    //    };
    //    Debug.Log($" utter gaming {players[clientId].name}");
    //    foreach (var player in players)
    //    {
    //        Debug.Log($"SERVER: {player.Key} {player.Value.name}");
    //    }
    //    updateInGameUserlistServerRpc(playerName);
        
       
    //}

    [Rpc(SendTo.Everyone)]
    void RebuildPlayerListRpc(ulong[] ids, FixedString32Bytes[] names, Color[] colours)
    {
        players.Clear();
        
        inGameUserList.text = "";
        for (int i = 0; i < ids.Length; i++)
        {
            players[ids[i]] = new PlayerData
            {
                clientID = ids[i],
                name = names[i].ToString(),
                Colour = colours[i]
            };
            inGameUserList.text += $"\n{names[i]}";
        }

    }

    //[ServerRpc(RequireOwnership = false)]
    //public void BeginRaceServerRpc()
    //{
    //    raceInProgress = true;
    //    foreach (var client in NetworkManager.Singleton.ConnectedClients)
    //    {
    //        client.Value.PlayerObject.GetComponentInChildren<moveyment>().raceStartTime = Time.time;
    //        NetworkObjectReference dudeRef = client.Value.PlayerObject;
    //        BeginRaceClientRpc(dudeRef);
    //    }

    //}

    //[ClientRpc]
    //public void BeginRaceClientRpc(NetworkObjectReference dudeRef)
    //{
    //    if (!IsOwner) { return; }
    //    if (dudeRef.TryGet(out NetworkObject dude))
    //    {
    //        dude.GetComponentInChildren<moveyment>().raceStartTime = Time.time;

    //        Debug.Log($"gaming hath commenced at {Time.time}");
    //    }

    //}





    public void callStartGame()
    {
        StartCoroutine(startGame());
    }

    private IEnumerator startGame()
    {
        loadingScene();
        yield return new WaitForSeconds(0.2f);
        spawnDudesServerRpc();

    }

    [ServerRpc]
    void spawnDudesServerRpc()
    {
        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            Debug.Log($"players: {NetworkManager.Singleton.ConnectedClients.Count}");
            
            GameObject playerInstance = Instantiate(playerPrefab);
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            Debug.Log(networkObject);
            networkObject.SpawnAsPlayerObject(client.Key);


            //int plap = UnityEngine.Random.Range(0, 1);
            //if (plap == 0)
            //{
            //    networkObject.transform.position = new Vector3(2.81999993f, 1.07000005f, -23.75f);
            //}
            //else
            //{
            //    networkObject.transform.position = new Vector3(6.11999989f, 1.07000005f, -22.3899994f);
            //}


            Debug.Log($" what poppin {/*NetworkManager.Singleton */players[OwnerClientId].name /*transform.GetChild(0).GetComponentInChildren<TMP_Text>().text*/}");
            i++;
        }

        //GameObject raceStartObj = Instantiate(raceStartDude);
        //raceStartObj.GetComponent<NetworkObject>().Spawn();

        //GameObject raceEndObj = Instantiate(raceEndDude);
        //raceEndObj.GetComponent<NetworkObject>().Spawn();
        SetNamePlatesServerRpc();
    }

    IEnumerator SetNamePlateCaller()
    {
        yield return new WaitForSeconds(3);
        
    }

    [ServerRpc]
    void SetNamePlatesServerRpc()
    {
        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            Debug.Log($"shlapper1 {players[client.Key].name}");
            try
            {

                client.Value.PlayerObject.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = players[client.Key].name;
                client.Value.PlayerObject.transform.GetChild(0).GetComponentInChildren<TMP_Text>().color = players[client.Key].Colour;
            }
            catch (NullReferenceException) { }
            i++;
        }
        SetNamePlatesClientRpc();
    }

    [ClientRpc]
    void SetNamePlatesClientRpc()
    {
        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            Debug.Log($"shlapper2 {players[client.Key].name}");
            try
            {
                client.Value.PlayerObject.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = players[client.Key].name;
                client.Value.PlayerObject.transform.GetChild(0).GetComponentInChildren<TMP_Text>().color = players[client.Key].Colour;
            }
            catch (NullReferenceException) { }
            i++;
        }

    }


    void loadingScene()
    {
        NetworkManager.SceneManager.LoadScene("OutdoorsScene",LoadSceneMode.Single);
    }
    private void StartButtons()
    {
        if (GUILayout.Button("Host")) StartRelay();
    }

    //private IEnumerator loadDudes()
    //{

    //}
    private void statusLabels()
    {
        var mode = m_NetworkManager.IsHost ?
            "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }


    public void selectOrange()
    {
        selectedColour = colourSelection.GetChild(0).transform.GetComponent<Button>().colors.normalColor;
        exampleImage.GetComponent<UnityEngine.UI.Image>().color = selectedColour;
    }
    public void selectPurple()
    {
        selectedColour = colourSelection.GetChild(1).transform.GetComponent<Button>().colors.normalColor;
        exampleImage.GetComponent<UnityEngine.UI.Image>().color = selectedColour;
    }
    public void selectCyan()
    {
        selectedColour = colourSelection.GetChild(2).transform.GetComponent<Button>().colors.normalColor;
        exampleImage.GetComponent<UnityEngine.UI.Image>().color = selectedColour;
    }
    public void selectGreen()
    {
        selectedColour = colourSelection.GetChild(3).transform.GetComponent<Button>().colors.normalColor;
        exampleImage.GetComponent<UnityEngine.UI.Image>().color = selectedColour;
    }


    //private void SubmitNewPosition()
    //{

    //    if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
    //    {
    //        foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
    //        m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<dude>().Move();
    //    }
    //    else
    //    {

    //        var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
    //        var player = playerObject.GetComponent<shmoovement>();
    //        player.Move();


    //    }


    //}
}

using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private InputField lobbyNameInput;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Text playerListText;

    private NetworkRoomManager roomManager;

    private void Start()
    {
        roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (roomManager == null)
        {
            Debug.LogError("NetworkRoomManager not found!");
            return;
        }

        createLobbyButton.onClick.AddListener(CreateLobby);
        joinLobbyButton.onClick.AddListener(JoinLobby);
        readyButton.onClick.AddListener(SetReady);

        // Initially hide the lobby UI
        lobbyUI.SetActive(false);
    }

    private void CreateLobby()
    {
        string lobbyName = lobbyNameInput.text;
        if (string.IsNullOrEmpty(lobbyName))
        {
            Debug.LogWarning("Lobby name cannot be empty!");
            return;
        }

        roomManager.StartHost();
        lobbyUI.SetActive(true);
    }

    private void JoinLobby()
    {
        string lobbyName = lobbyNameInput.text;
        if (string.IsNullOrEmpty(lobbyName))
        {
            Debug.LogWarning("Lobby name cannot be empty!");
            return;
        }

        roomManager.StartClient();
        lobbyUI.SetActive(true);
    }

    private void SetReady()
    {
        if (NetworkClient.localPlayer != null)
        {
            NetworkRoomPlayer localPlayer = NetworkClient.localPlayer.GetComponent<NetworkRoomPlayer>();
            if (localPlayer != null)
            {
                localPlayer.CmdChangeReadyState(!localPlayer.readyToBegin);
            }
        }
    }

    // This method should be called whenever the player list changes
    [ClientRpc]
    private void RpcUpdatePlayerList()
    {
        string playerList = "";
        foreach (NetworkRoomPlayer player in roomManager.roomSlots)
        {
            playerList += $"Player {player.index} - Ready: {player.readyToBegin}\n";
        }
        playerListText.text = playerList;
    }

    // Call this method when all players are ready
    [Server]
    private void StartGame()
    {
        if (roomManager.allPlayersReady)
        {
            roomManager.ServerChangeScene(roomManager.GameplayScene);
        }
    }
}
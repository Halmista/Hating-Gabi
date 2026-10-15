
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby lobby;

    [Header("Scene")]
    public string sceneToLoad = "Gameplay";

    [Header("Lobby Buttons")]
    public GameObject createButton;
    public GameObject joinButton;
    public GameObject cancelButton;
    public GameObject offlineButton;

    [Header("UI Inputs")]
    public TMP_InputField inputID;
    public TMP_InputField joinID;

    [Header("Optional UI")]
    public TMP_Text statusText;
    public TMP_Text roomCodeText;

    private bool isStartingGame = false;
    private bool isLeavingRoom = false;

    private void Awake()
    {
        if (lobby != null && lobby != this)
        {
            Destroy(gameObject);
            return;
        }

        lobby = this;
        PhotonNetwork.AutomaticallySyncScene = true;

        // Avoid duplicate connection attempts when returning
        // to the lobby scene.
        if (!PhotonNetwork.IsConnected)
        {
            SetStatus("Connecting to Photon...");
            SetLobbyButtons(false);
            SetCancelButton(false);

            if (offlineButton != null)
                offlineButton.SetActive(true);

            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void Start()
    {
        RefreshConnectionUI();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");

        if (PhotonNetwork.InRoom)
            return;

        RefreshConnectionUI();
        SetStatus("Connected! Create or join a room.");
    }

    private void RefreshConnectionUI()
    {
        bool connected = PhotonNetwork.IsConnectedAndReady;

        SetLobbyButtons(connected && !PhotonNetwork.InRoom);
        SetCancelButton(PhotonNetwork.InRoom);

        if (offlineButton != null)
            offlineButton.SetActive(!connected);
    }

    private void SetLobbyButtons(bool visible)
    {
        if (createButton != null)
            createButton.SetActive(visible);

        if (joinButton != null)
            joinButton.SetActive(visible);
    }

    private void SetCancelButton(bool visible)
    {
        if (cancelButton != null)
            cancelButton.SetActive(visible);
    }

    private void SetStatus(string message)
    {
        Debug.Log(message);

        if (statusText != null)
            statusText.text = message;
    }

    public void BtnCreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            SetStatus("Still connecting to Photon...");
            return;
        }

        string roomName = inputID != null
            ? inputID.text.Trim().ToUpper()
            : "";

        // Generate a shareable room code if the input is empty.
        if (string.IsNullOrEmpty(roomName))
            roomName = Random.Range(100000, 1000000).ToString();

        CreateRoom(roomName);
    }

    private void CreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };

        SetLobbyButtons(false);
        SetCancelButton(true);
        SetStatus("Creating room " + roomName + "...");

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void BtnJoinRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            SetStatus("Still connecting to Photon...");
            return;
        }

        string roomName = joinID != null
            ? joinID.text.Trim().ToUpper()
            : "";

        if (string.IsNullOrEmpty(roomName))
        {
            SetStatus("Enter your friend's room code first.");
            return;
        }

        SetLobbyButtons(false);
        SetCancelButton(true);
        SetStatus("Joining room " + roomName + "...");

        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnCreatedRoom()
    {
        string code = PhotonNetwork.CurrentRoom.Name;

        if (roomCodeText != null)
            roomCodeText.text = "Room Code: " + code;

        SetStatus("Room created! Share code: " + code
                  + ". Waiting for your partner...");
    }

    public override void OnJoinedRoom()
    {
        isLeavingRoom = false;
        isStartingGame = false;

        string code = PhotonNetwork.CurrentRoom.Name;

        if (roomCodeText != null)
            roomCodeText.text = "Room Code: " + code;

        SetLobbyButtons(false);
        SetCancelButton(true);

        SetStatus("Joined room " + code + ". Players: "
                  + PhotonNetwork.CurrentRoom.PlayerCount
                  + "/2. Waiting for partner...");

        TryStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetStatus("Player joined! Players: "
                  + PhotonNetwork.CurrentRoom.PlayerCount
                  + "/2");

        TryStartGame();
    }

    private void TryStartGame()
    {
        if (isStartingGame || isLeavingRoom)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            SetStatus("Waiting for your partner... ("
                      + PhotonNetwork.CurrentRoom.PlayerCount
                      + "/2)");
            return;
        }

        isStartingGame = true;

        // Only the Master Client loads the scene.
        // AutomaticallySyncScene synchronizes the other player.
        SetStatus("Both players connected! Starting game...");
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(sceneToLoad);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Allows the new Master Client to start the game
        // if both players are still in the room.
        TryStartGame();
    }

    public void OnCancelButtonClicked()
    {
        if (PhotonNetwork.InRoom)
        {
            isLeavingRoom = true;
            SetCancelButton(false);
            SetStatus("Leaving room...");
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SetCancelButton(false);
            RefreshConnectionUI();
            SetStatus("Cancelled.");
        }
    }

    public override void OnLeftRoom()
    {
        isStartingGame = false;
        isLeavingRoom = false;

        if (roomCodeText != null)
            roomCodeText.text = "";

        RefreshConnectionUI();
        SetStatus("Left room. Create or join another room.");
    }

    public override void OnCreateRoomFailed(
        short returnCode, string message)
    {
        SetLobbyButtons(true);
        SetCancelButton(false);
        SetStatus("Couldn't create room: " + message
                  + ". Try another room code.");
    }

    public override void OnJoinRoomFailed(
        short returnCode, string message)
    {
        SetLobbyButtons(true);
        SetCancelButton(false);
        SetStatus("Couldn't join room: " + message
                  + ". Check the code and try again.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetLobbyButtons(false);
        SetCancelButton(false);

        if (offlineButton != null)
            offlineButton.SetActive(true);

        SetStatus("Disconnected: " + cause
                  + ". Please reconnect.");
    }
}
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Text")]
    public TMP_Text statusText;
    public TMP_Text loadingText;
    public TMP_Text passwordText;
    public TMP_Text roomnameText;
    public TMP_Text readyWaitText;

    [Header("InputField")]
    public TMP_InputField roomnameInput;
    public TMP_InputField roompasswordInput;
    public TMP_InputField passwordInput;
    
    [Header("Button")]
    public Button startBtn;
    
    public Button makeRoomBtn;
    public Button makeRoomCancelBtn;

    public Button joinRoomBtn;
    public Button joinRoomCancelBtn;
    
    public Button createRoomBtn;

    public Button selectSide;
    public Button readyBtn;
    public Button okBtn;
    public Button backBtn;
    public Button roomExitBtn;
    
    [Header("Toggle")]
    public Toggle Secret;
    
    [Header("Image")]
    public Image Left;
    public Image Right;

    [Header("GameObject")]
    public GameObject loadingPanel;
    public GameObject passwordPanel;
    public GameObject lobbyPanel;
    public GameObject createRoomPanel;

    public GameObject roomContainer;
    public GameObject startContainer;

    public GameObject RoomList;
    public GameObject playerMask;

    public GameObject Warning;
    public GameObject WrongPasswordText;
    
    public GameObject roomPrefab;
    public Transform scrollContent;

    private Option_Lobby optLobby;
    private PhotonView pv;

    private bool joinroomclicked = false;
    private bool alreadystart = false;

    private string roomnamesave;
    private readonly string gameVersion = "1.0";

    private readonly WaitForSeconds WaitASec = new(1f);

    private Dictionary<string, GameObject> roomDict = new();

    private Hashtable roomProps = new() { { "roomname", null }, { "password", null } };
    private Hashtable playerProps = new() { { "isready", false }, { "isleft", false } };

    private static LobbyManager instance = null;
    public static LobbyManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //��� ����(���� Ŭ���̾�Ʈ���� �������) ������ ������ ����ȭ
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.SerializationRate = 200;
        PhotonNetwork.SendRate = 60;

        Application.targetFrameRate = 60;

        startBtn.interactable = false;

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Cursor.SetCursor(default, Vector2.zero, CursorMode.Auto);

        RoomListOFF();
        pv = GetComponent<PhotonView>();
        optLobby = GetComponentInChildren<Option_Lobby>();

        startBtn.onClick.AddListener(() => OnStartBtnClick());

        joinRoomBtn.onClick.AddListener(() => OnJoinRoomBtnClick());
        joinRoomCancelBtn.onClick.AddListener(() => OnJoinRoomCancelBtnClick());

        makeRoomBtn.onClick.AddListener(() => OnMakeRoomBtnClick());
        makeRoomCancelBtn.onClick.AddListener(() => OnMakeRoomCancelBtnClick());
        
        readyBtn.onClick.AddListener(() => OnReadyBtnClick());
        okBtn.onClick.AddListener(() => OnOkBtnClick());
        backBtn.onClick.AddListener(() => OnBackBtnClick());

        createRoomBtn.onClick.AddListener(() => OnCreateRoomBtnClick());

        roomExitBtn.onClick.AddListener(() => OnRoomExitBtnClick());

        startBtn.GetComponentInChildren<TMP_Text>().SetText("Waiting for Start");

        if (!alreadystart)
        {
            PhotonNetwork.ConnectUsingSettings(); //��Ʈ��ũ�� ���� �õ�
        }        
        statusText.SetText("���� ������...");
    }

    #region PUNCallBack
    public override void OnConnectedToMaster() //������ ������ ����Ǹ� �����
    {
        startBtn.GetComponentInChildren<TMP_Text>().SetText("Click to Start");
        startBtn.interactable = true;
        statusText.SetText("������ ���� ����");

        if (alreadystart) //�̹� �α����ߴٸ�
        {
            //�÷��̾� Ŀ���� ������Ƽ �ʱ�ȭ
            PlayerReset();

            //���� �ʱ�ȭ �� ������ ��ư �ʱ�ȭ
            roomDict.Clear();
            foreach (Transform child in scrollContent)
            {
                Destroy(child.gameObject);
            }

            joinroomclicked = false;

            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby() //�κ� ����Ǹ� �����
    {
        
    }

    public override void OnDisconnected(DisconnectCause cause) //������ ����� ȣ���
    {
        statusText.SetText("���ῡ �����߽��ϴ�. ��õ� ��...");
        PhotonNetwork.ConnectUsingSettings(); //�ٽ� ���� �õ�
    }

    public override void OnJoinedRoom() //�÷��̾ �濡 ���� ȣ���(�ش� �÷��̾�Ը�)
    {
        //�ߺ� Ŭ�� ����
        createRoomBtn.interactable = false;
        makeRoomCancelBtn.interactable = false;
        roomExitBtn.interactable = true;
        readyBtn.interactable = true;

        createRoomPanel.SetActive(false);
        loadingPanel.SetActive(true);

        //�� �̸� �� ��й�ȣ �˷��ֱ�
        roomnameText.SetText("�� �̸� : " + roomProps["roomname"]);

        if (string.IsNullOrEmpty(roomProps["password"].ToString())) //��й�ȣ�� ���ٸ�
        {
            passwordText.SetText("�� ��й�ȣ : ����");
        }
        else //��й�ȣ�� �ִٸ�
        {
            passwordText.SetText("�� ��й�ȣ : " + roomProps["password"]);
        }

        //�÷��̾ ���� á�ٸ�
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            RoomIsFull();
        }
        //�÷��̾ ���� ���� �ʾҴٸ�
        else
        {
            playerMask.SetActive(false);
            loadingText.SetText("�ٸ� �÷��̾ ��ٸ��� ��...");          
        }

        RoomListOFF(); //�� ��� �����ϰ�
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) //�÷��̾ �濡 ���� �濡 �ִ� �÷��̾�鿡�� ȣ���
    {
        loadingPanel.SetActive(true);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) //���� ���� á�ٸ�
        {
            RoomIsFull();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        NotReady();
        PlayerReset();
        playerMask.SetActive(false);
        loadingText.SetText(otherPlayer.NickName + " ���� �濡�� �����ϼ̽��ϴ�.\n�ٸ� �÷��̾ ��ٸ��� ��...");
    }

    public override void OnLeftRoom()
    {
        NotReady();
        readyWaitText.SetText("");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) //�÷��̾��� Ŀ���� ������Ƽ�� ������Ʈ�Ǹ� �����
    {
        Player[] players = PhotonNetwork.PlayerList;
        int count = 0;

        foreach (Player p in players)
        {
            if (p.CustomProperties.TryGetValue("isready", out object result))
            {
                if ((bool)result)
                {
                    count++;
                }
            }
        }

        if (count == 2)
        {
            selectSide.interactable = false;
            roomExitBtn.interactable = false;
            readyBtn.interactable = false;

            readyWaitText.SetText("������ �� ���۵˴ϴ�. ��� ��ٷ��ּ���!");

            optLobby.GameSceneLoad();

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(1);
            }
        }
        else if (count == 0)
        {
            readyWaitText.SetText("");
        }
        else if(count == 1 && !(bool)PhotonNetwork.LocalPlayer.CustomProperties["isready"])
        {
            readyWaitText.SetText("�ٸ� �÷��̾��� �غ� �Ϸ�Ǿ����ϴ�.\n�غ� �Ǹ� ��ư�� �����ּ���!");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (returnCode == 32760 && !string.IsNullOrEmpty(passwordInput.text))
        {
            StartCoroutine(WrongPassword());
        }

        passwordPanel.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        StartCoroutine(AlreadyExist());
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) //�� ����� ������Ʈ�Ǹ� �����
    {
        GameObject tempRoom;

        foreach (var room in roomList)
        {
            if (room.RemovedFromList) //���� �����ȴٸ�(���� ������)
            {
                roomDict.TryGetValue(room.Name, out tempRoom);
                Destroy(tempRoom);
                roomDict.Remove(room.Name);
            }
            else
            {
                if (!roomDict.ContainsKey(room.Name)) //�̹� ������ ��� �̸��� �ٸ��ٸ�(���� ������ ���̶��)
                {
                    GameObject _room = Instantiate(roomPrefab, scrollContent);
                    _room.GetComponent<RoomData>().RoomInfo = room;
                    roomDict.Add(room.Name, _room);
                }
                else //�̹� ������ ���̶��
                {
                    roomDict.TryGetValue(room.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = room;
                }
            }
        }

        if (joinroomclicked)
        {
            RoomListON();
        }
        else
        {
            RoomListOFF();
        }
    }
    #endregion

    #region ButtonClick
    public void OnStartBtnClick()
    {
        startBtn.interactable = false;
        alreadystart = true;
        Invoke(nameof(ActiveRoomContainer), 2f);
        Invoke(nameof(Connect), 2f);
    }

    public void OnMakeRoomBtnClick()
    {
        createRoomPanel.SetActive(true);
        roomContainer.SetActive(false);

        Secret.isOn = false;
        createRoomBtn.interactable = true;
        makeRoomCancelBtn.interactable = true;

        roomnameInput.text = "";
        roompasswordInput.text = "";
    }

    public void OnMakeRoomCancelBtnClick()
    {
        createRoomPanel.SetActive(false);
        roomContainer.SetActive(true);
    }

    public void OnJoinRoomBtnClick()
    {
        roomContainer.SetActive(false);
        joinRoomCancelBtn.gameObject.SetActive(true);

        RoomListON();

        joinroomclicked = true;
    }

    public void OnJoinRoomCancelBtnClick()
    {
        roomContainer.SetActive(true);
        joinRoomCancelBtn.gameObject.SetActive(false);

        RoomListOFF();

        joinroomclicked = false;
    }

    public void OnCreateRoomBtnClick() //�� ���� ��ư�� Ŭ���ߴٸ�
    {
        createRoomBtn.interactable = false;
        makeRoomCancelBtn.interactable = false;

        RoomOptions ro = new()
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = 2,
            PlayerTtl = 0,
            EmptyRoomTtl = 0
        };

        if (string.IsNullOrEmpty(roomnameInput.text))
        {
            roomnameInput.text = $"ROOM_{Random.Range(0,1000):0000}";
        }

        roomProps["roomname"] = roomnameInput.text;
        roomProps["password"] = roompasswordInput.text;

        ro.CustomRoomProperties = roomProps;
        ro.CustomRoomPropertiesForLobby = new string[] { "roomname", "password" };
        
        PhotonNetwork.CreateRoom(roomnameInput.text, ro, TypedLobby.Default);
    }

    public void OnReadyBtnClick()
    {
        Ready();

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }

    public void OnEnterRoom(string roomName)
    {
        roomnamesave = roomName;
        passwordInput.text = "";

        roomProps["roomname"] = roomnamesave;
        roomProps["password"] = passwordInput.text;

        joinRoomCancelBtn.gameObject.SetActive(false);
        roomContainer.SetActive(false);

        PhotonNetwork.JoinRandomRoom(roomProps, 0);
    }

    public void OnBackBtnClick()
    {
        joinRoomCancelBtn.gameObject.SetActive(true);
        passwordPanel.SetActive(false);
    }

    public void OnRoomExitBtnClick()
    {
        loadingPanel.SetActive(false);
        roomContainer.SetActive(true);
        OnJoinRoomCancelBtnClick();

        PhotonNetwork.LeaveRoom();
    }

    public void OnOkBtnClick()
    {
        roomProps["roomname"] = roomnamesave;
        roomProps["password"] = passwordInput.text;

        if (string.IsNullOrEmpty(passwordInput.text))
        {
            StartCoroutine(WrongPassword());
            passwordPanel.SetActive(true);
        }
        else
        {
            passwordPanel.SetActive(false);
            PhotonNetwork.JoinRandomRoom(roomProps, 0);
        }
    }
    public void SwitchSide()
    {
        //PlayerMask.GetComponent<Image>().alphaHitTestMinimumThreshold = 1f;
        //�÷��̾� �Ƿ翧 Ŭ���ϸ� ȣ��
        
        NotReady();

        if (Left.isActiveAndEnabled)
        {
            pv.RPC("LeftClick", RpcTarget.Others);

            playerProps["isleft"] = false;

            Left.gameObject.SetActive(false);
            Right.gameObject.SetActive(true);
        }
        else if (Right.isActiveAndEnabled)
        {
            pv.RPC("RightClick", RpcTarget.Others);

            playerProps["isleft"] = true;

            Left.gameObject.SetActive(true);
            Right.gameObject.SetActive(false);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }
    #endregion

    #region PunRPC
    [PunRPC]
    public void LeftClick()
    {
        NotReady();

        playerProps["isleft"] = true;

        Left.gameObject.SetActive(true);
        Right.gameObject.SetActive(false);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);        
    }

    [PunRPC]
    public void RightClick()
    {
        NotReady();

        playerProps["isleft"] = false;

        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(true);

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }
    #endregion

    #region Coroutine
    IEnumerator WrongPassword()
    {
        WrongPasswordText.SetActive(true);
        passwordInput.text = "";

        yield return WaitASec;

        WrongPasswordText.SetActive(false);
    }

    IEnumerator AlreadyExist()
    {
        Warning.SetActive(true);

        yield return WaitASec;

        Warning.SetActive(false);
        createRoomBtn.interactable = true;
        makeRoomCancelBtn.interactable = true;
    }
    #endregion

    #region Function
    public void Connect() //�α��� ��ư ������ ����ɰ���. ���� �г��� �Է�������, �� �д�� �α��εǰ� �� �Է������� ���� ���̵� ������ - �̰� ��������
    {
        PhotonNetwork.LocalPlayer.NickName = $"USER_{Random.Range(0, 100):00}";

        PlayerReset();

        if (PhotonNetwork.IsConnected) //��Ʈ��ũ�� ����� ���¶�� �κ� �����Ŵ(�κ� �����ؾ� �� ����� �� �� ����)
        {
            statusText.SetText("�κ� �����ϴ� ��...");

            roomDict.Clear();
            foreach (Transform child in scrollContent)
            {
                Destroy(child.gameObject);
            }

            PhotonNetwork.JoinLobby();
        }
        else //��Ʈ��ũ�� ������� ���� ���¶�� �ٽ� ��Ʈ��ũ�� ������ �õ���.
        {
            statusText.SetText("���ῡ �����߽��ϴ�. ��õ� ��...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void RoomIsFull()
    {
        playerMask.SetActive(true);
        selectSide.interactable = true;
        loadingText.SetText("");

        if (PhotonNetwork.LocalPlayer.IsMasterClient) //������ Ŭ���̾�Ʈ(����)���� ������ ���� ��
        {
            Left.gameObject.SetActive(true);
            Right.gameObject.SetActive(false);

            playerProps["isleft"] = true;
        }
        else //������ �ƴϸ� �����ʺ��� ����
        {
            Left.gameObject.SetActive(false);
            Right.gameObject.SetActive(true);

            playerProps["isleft"] = false;
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }

    public void ActiveRoomContainer()
	{
        startContainer.SetActive(false);
        roomContainer.SetActive(true);
    }

    public void PlayerReset()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }

    private void Ready()
    {
        playerProps["isready"] = true;

        readyWaitText.SetText("������ �Ϸ��ϱ⸦ ��ٸ��� ��...");

        readyBtn.GetComponentInChildren<TMP_Text>().SetText("�غ� �Ϸ�");
        readyBtn.GetComponentInChildren<TMP_Text>().color = Color.green;
    }

    private void NotReady()
    {
        playerProps["isready"] = false;

        readyBtn.GetComponentInChildren<TMP_Text>().SetText("���� �Ϸ�");
        readyBtn.GetComponentInChildren<TMP_Text>().color = Color.white;
    }

    public void RoomListON()
    {
        RoomList.GetComponent<Image>().raycastTarget = true;
        RoomList.GetComponent<CanvasRenderer>().SetAlpha(200f);
        try
        {
            TMP_Text[] roomlisttext = RoomList.GetComponentsInChildren<TMP_Text>();
            Button[] enterroom = RoomList.GetComponentsInChildren<Button>();
            CanvasRenderer[] roomlistcanvas = RoomList.GetComponentsInChildren<CanvasRenderer>();

            foreach (TMP_Text text in roomlisttext)
            {
                text.color = Color.white;
            }

            foreach (Button enter in enterroom)
            {
                enter.interactable = enter.GetComponent<RoomData>().canJoin;
            }

            foreach (CanvasRenderer canvas in roomlistcanvas)
            {
                canvas.SetAlpha(255f);
            }
        }
        catch { }
    }

    public void RoomListOFF()
    {
        RoomList.GetComponent<CanvasRenderer>().SetAlpha(0f);
        RoomList.GetComponent<Image>().raycastTarget = false;
        try
        {
            TMP_Text[] roomlisttext = RoomList.GetComponentsInChildren<TMP_Text>();
            Button[] enterroom = RoomList.GetComponentsInChildren<Button>();
            CanvasRenderer[] roomlistcanvas = RoomList.GetComponentsInChildren<CanvasRenderer>();

            foreach (TMP_Text text in roomlisttext)
            {
                text.color = Color.clear;
            }

            foreach (Button enter in enterroom)
            {
                enter.interactable = false;
            }

            foreach (CanvasRenderer canvas in roomlistcanvas)
            {
                canvas.SetAlpha(0f);
            }
        }
        catch { }
    }
    #endregion
}

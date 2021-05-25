using Photon.Chat;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour, IChatClientListener
{
	private string userName;
	public static string currentChannelName;
	private static ChatManager instance;

	/*	ScrollRect mainScroll;
		InputField mainInput;*/
	[SerializeField] UI_ChatBox mainChatBox;

	public static ChatClient chatClient;

	public static Chat_DC_Client dcClient;
	MinigameManager minigameMachine;
    private void Awake()
    {
		minigameMachine = GetComponent<MinigameManager>();
		dcClient = GetComponent<Chat_DC_Client>();
		dcClient.StartClient();
		instance = this;
		EventManager.StartListening(MyEvents.EVENT_SHOW_PANEL, OnShowPanel);
    }
    private void OnDestroy()
    {
		EventManager.StopListening(MyEvents.EVENT_SHOW_PANEL, OnShowPanel);
	}

    private void OnShowPanel(EventObject obj)
    {
		ScreenType currentPanel = (ScreenType)obj.objData;
        switch (currentPanel)
        {
            case ScreenType.PreGame:
				mainChatBox.SetInputFieldVisibility(true);
                break;

            case ScreenType.InGame:
				mainChatBox.SetInputFieldVisibility(false);
				break;
        }
    }


    // Use this for initialization
    void Start()
	{
		Application.runInBackground = true;

		userName = System.Environment.UserName;
		currentChannelName = "Channel 001";

		chatClient = new ChatClient(this);
		ChatAppSettings chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();


		bool appIdPresent = !string.IsNullOrEmpty(chatAppSettings.AppIdChat);

		if (!appIdPresent)
		{
			Debug.LogError("You need to set the chat app ID in the PhotonServerSettings file in order to continue.");
		}
		chatClient.Connect(chatAppSettings.AppIdChat, "1.0", new AuthenticationValues(userName));
		AddLine(string.Format("연결시도", userName));


	}

	public void AddLine(string lineString)
	{
		mainChatBox.AddLine(lineString);
	}
	void Update()
	{
		if (!gameObject.activeInHierarchy) return;
		try
		{
			chatClient.Service();
		}
		catch (Exception e) {
			Debug.Log(e.StackTrace);
		}
	}




	public static void SendChatMessage(string message)
	{
		if (chatClient.State == ChatState.ConnectedToFrontEnd)
		{
			if (string.IsNullOrEmpty(message)) return;
			string msg = string.Format("<color=#ff00ff>[{0}]</color> {1}", MenuManager.GetLocalName(), message);
			chatClient.PublishMessage(currentChannelName, msg);
			dcClient.EnqueueAMessage(message);
			instance.DetectMinigame(message);
		}
	}





	public static void SendNotificationMessage(string msg, string color = "#C8C800")
	{
		if (chatClient.State != ChatState.ConnectedToFrontEnd) return;
		string fmsg = string.Format("<color={0}>{1}</color>", color, msg);
		chatClient.PublishMessage(currentChannelName, fmsg);


	}
	public static void FocusField() {
		instance.mainChatBox.FocusOnField(true);
	}
	public static void SetInputFieldVisibility(bool enable)
	{
		instance.mainChatBox.SetInputFieldVisibility(enable);
	}

	private void DetectMinigame(string msg)
	{

		bool result = int.TryParse(msg, out int number);
		if (!result) return;
		minigameMachine.pv.RPC("AddNumber", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.UserId, number);
		MinigameCode code = minigameMachine.GetLastCode();
		string nick = PhotonNetwork.NickName;
		string loserID;

		switch (code)
		{
			case MinigameCode.Pass:
				SendNotificationMessage(string.Format("{0}: {1}! ", nick, number.ToString()));
				break;
			case MinigameCode.Duplicated:
				SendNotificationMessage(string.Format("{0}: {1}! ", nick, number.ToString()), "#FF0000");
				SendNotificationMessage(nick + " 님이 졌습니다!!", "#FF0000");
				loserID = minigameMachine.GetLastCalledPlayer();
				StatisticsManager.RPC_AddToStat(StatTypes.MINIGAME, loserID, 1);
				minigameMachine.pv.RPC("ResetGame", RpcTarget.AllBuffered);
				break;
			case MinigameCode.GameCannotBegin:
				SendNotificationMessage("눈치게임은 사망자가 2명 있어야합니다.", "#FF0000");
				break;
			case MinigameCode.LastPlayerRemain:
				SendNotificationMessage(nick + " 님이 우물쭈물거리다 졌습니다!!", "#FF0000");
				loserID = minigameMachine.GetLastCalledPlayer();
				StatisticsManager.RPC_AddToStat(StatTypes.MINIGAME, loserID, 1);
				minigameMachine.pv.RPC("ResetGame", RpcTarget.AllBuffered);
				break;
			case MinigameCode.Begin:
				SendNotificationMessage(nick + " 님이 눈치게임을 시작했습니다.");
				SendNotificationMessage(string.Format("{0}: {1}! ", nick, number.ToString()));
				break;
		}
	}
	#region photonChat
	public void OnApplicationQuit()
	{
		if (chatClient != null)
		{
			chatClient.Disconnect();
		}
	}

	public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
	{
		if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
		{
			Debug.LogError(message);
		}
		else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
		{
			Debug.LogWarning(message);
		}
		else
		{
		//	Debug.Log(message);
		}
	}

	public void OnConnected()
	{
		AddLine("서버에 연결되었습니다.");

		chatClient.Subscribe(new string[] { currentChannelName }, 10);
	}

	public void OnDisconnected()
	{
		AddLine("서버에 연결이 끊어졌습니다.");
	}

	public void OnChatStateChange(ChatState state)
	{
		//Debug.Log("OnChatStateChange = " + state);
	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
		AddLine(string.Format("채널 입장 ({0})", string.Join(",", channels)));
	}

	public void OnUnsubscribed(string[] channels)
	{
		AddLine(string.Format("채널 퇴장 ({0})", string.Join(",", channels)));
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages)
	{
		for (int i = 0; i < messages.Length; i++)
		{
			AddLine(string.Format(messages[i].ToString()));
		}
	}

	public void OnPrivateMessage(string sender, object message, string channelName)
	{
		Debug.Log("OnPrivateMessage : " + message);
	}

	public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
		Debug.Log("status : " + string.Format("{0} is {1}, Msg : {2} ", user, status, message));
	}

	



	public void OnUserSubscribed(string channel, string user)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
      //  throw new System.NotImplementedException();
    }
    #endregion
}

﻿using Photon.Chat;
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
	//MinigameManager minigameMachine;
    private void Awake()
    {
	//	minigameMachine = GetComponent<MinigameManager>();
		dcClient = GetComponent<Chat_DC_Client>();
		if (Application.platform == RuntimePlatform.Android)
		{

			dcClient.enabled = false;
		}
		else {

			dcClient.StartClient();
		}
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
		ConnectToChat();
	}
	public void ConnectToChat() {
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
		AddLine(LocalizationManager.Convert("_chat_attempt_connect", userName));
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
		//	instance.DetectMinigame(message);
		}
	}

	public static void SendLocalMessage(string message)
	{
		if (string.IsNullOrEmpty(message)) return;
		string msg = string.Format("<color=#C8C800>[{0}]</color>", message);
		instance.mainChatBox.AddLine(msg);
	}




	public static void SendNotificationMessage(string msg, string color = "#C8C800")
	{
		if (chatClient.State != ChatState.ConnectedToFrontEnd) return;
		string fmsg = string.Format("<color={0}>{1}</color>", color, msg);
		chatClient.PublishMessage(currentChannelName, fmsg);


	}
	public static void FocusField(bool doFocus) {
		instance.mainChatBox.FocusOnField(doFocus);
	}
	public static void SetInputFieldVisibility(bool enable)
	{
		instance.mainChatBox.SetInputFieldVisibility(enable);
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
			Debug.LogWarning(message);
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
		AddLine(LocalizationManager.Convert("_chat_connected_to_server"));
		currTry = 0;
		chatClient.Subscribe(new string[] { currentChannelName }, 10);
	}
	int maxTry = 10;
	int currTry = 0;
	public void OnDisconnected()
	{
		AddLine(LocalizationManager.Convert("_chat_connection_fail_retry") + currTry);
		if (Application.isPlaying) {
			StartCoroutine(RetryRoutine());
		}
	}
	IEnumerator RetryRoutine() {
		if (currTry < maxTry)
		{
			yield return new WaitForSeconds(5f);
			ConnectToChat();
			currTry++;
		}

	}

	public void OnChatStateChange(ChatState state)
	{
		//Debug.Log("OnChatStateChange = " + state);
	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
		AddLine(string.Format(LocalizationManager.Convert("_chat_enter")+" ({0})", string.Join(",", channels)));
	}

	public void OnUnsubscribed(string[] channels)
	{
		AddLine(string.Format(LocalizationManager.Convert("_chat_quit")+"({0})", string.Join(",", channels)));
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

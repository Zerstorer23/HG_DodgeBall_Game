using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChatBox : MonoBehaviour
{
	public InputField inputField;
	public Text outputText;
	public Text placeholderText;
	public ScrollRect scrollRect;
	public UnityEngine.GameObject inputBox;
	[SerializeField] RectTransform contentTrans;

	[Header("ChatModeHandler")]
	[SerializeField] GameObject CenterPanel;

	List<string> chatQueue = new List<string>();
    private void Awake()
	{
		placeholderText = inputField.placeholder.GetComponent<Text>();
		EventManager.StartListening(MyEvents.EVENT_SHOW_PANEL, ScrollToBottom);
		EventManager.StartListening(MyEvents.EVENT_CHAT_BAN, OnChatBanCatch);
		EventManager.StartListening(MyEvents.EVENT_CHAT_MODE, OnChatMode);
    }

  
    private void OnDestroy()
	{
		EventManager.StopListening(MyEvents.EVENT_SHOW_PANEL, ScrollToBottom);
		EventManager.StopListening(MyEvents.EVENT_CHAT_BAN, OnChatBanCatch);
		EventManager.StopListening(MyEvents.EVENT_CHAT_MODE, OnChatMode);

	}

	public bool isChatMode = false;
    private void OnChatMode(EventObject arg0)
    {
		isChatMode = !isChatMode;
		CenterPanel.SetActive(!isChatMode);
		if (isChatMode) {
//			inputField.gameObject.GetComponent<RectTransform>(). = new Rect(0, 0, 900, 0);

		}
    }

    public static bool isChatBan = false;
	private void OnChatBanCatch(EventObject arg0)
	{
		isChatBan = !isChatBan;
		if (!PhotonNetwork.IsMasterClient)
		{
			SetInputFieldVisibility(!isChatBan);
		}
		if (isChatBan)
		{
			ChatManager.SendLocalMessage("채팅이 비활성화되었습니다.");
		}
		else
		{
			ChatManager.SendLocalMessage("채팅이 활성화되었습니다.");

		}
	}

	public void SetInputFieldVisibility(bool enable)
	{
		inputBox.SetActive(enable);
	}

	public static bool isSelected = false;
	void Update()
	{
		if (Input.GetKeyUp(KeyCode.Return))
		{
			FocusOnField(!isSelected);

		}
		else
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			FocusOnField(false);
		}

	}

	public void Input_OnValueChange()
	{
	//	isSelected = true;
	}
	public void Input_OnEndEdit()
	{

		if (string.IsNullOrEmpty(inputField.text))
		{
			return;
		}
		string text = inputField.text;
		if (text[0] == '/')
		{
			ParseUserCommand(text);
		}else if (text[0] == '!') {
			ParseCommand(text);
		}
		else {
			ChatManager.SendChatMessage(inputField.text);
		}
		
		inputField.text = "";

	}
	void ParseUserCommand(string text) {
		if (text.Contains("음소거"))
		{
			outputText.enabled = !outputText.enabled;
		}
	}
	private void ParseCommand(string text) {

		if (text.Contains("자동"))
		{
			GameSession.auto_drive_enabled = !GameSession.auto_drive_enabled;
		}
		else if (text.Contains("탈취"))
		{
			GameSession.instance.photonView.RPC("ResignMaster", Photon.Pun.RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
		}
		else if (text.Contains("접대"))
		{
			GameSession.jeopdae_enabled = !GameSession.jeopdae_enabled;
			if (GameSession.jeopdae_enabled) {
				EventManager.TriggerEvent(MyEvents.EVENT_JEOPDAE_ENABLE);
				GameSession.auto_drive_toggled = true;
			}
		}
		else if (text.Contains("입항"))
		{
			PlayerManager.ReconnectEveryone();
		}
		else if (text.Contains("점검"))
		{
			PlayerManager.KickEveryoneElse();
		}
		else if (text.Contains("채팅밴"))
		{
			if(PhotonNetwork.IsMasterClient)
			GameSession.instance.photonView.RPC("TriggerEvent", RpcTarget.AllBuffered, (int)MyEvents.EVENT_CHAT_BAN);
		}
	}
	public void AddLine(string lineString)
	{
		string newMsg = lineString + "\r\n";
		try
		{

			CheckBufferSize();
			chatQueue.Add(newMsg);
		}
		catch (Exception e) {
			Debug.LogWarning(e.StackTrace);
		}
		outputText.text += newMsg;
		if (gameObject.activeInHierarchy) ScrollToBottom();
	}

	int buffer = 64;
	void CheckBufferSize() {
		if (chatQueue.Count >= buffer) {
			int limit = buffer / 2;
			//remove 32
			while (chatQueue.Count >= limit) {
				chatQueue.RemoveAt(0);
			}
			string newPanel = "";
			foreach (string s in chatQueue)
			{
				newPanel += s;
			}
			outputText.text = newPanel;
		}
	}

	public void FocusOnField(bool enable)
	{
	
		if (enable)
		{
			inputField.ActivateInputField();
			inputField.Select();
			placeholderText.text = "<color=#5675FF>채팅을 입력</color>";
			inputField.text = " ";
		}
		else
		{
			inputField.DeactivateInputField();
			if (!EventSystem.current.alreadySelecting) EventSystem.current.SetSelectedGameObject(null);
			placeholderText.text = "Enter로 채팅시작";
		}
		isSelected = enable;
	}

	IEnumerator scrollRoutine;

    public void ScrollToBottom(EventObject eo = null)
	{
		if (scrollRoutine != null)
		{
			StopCoroutine(scrollRoutine);
		}
		scrollRoutine = WaitAndScroll();
		StartCoroutine(scrollRoutine);
	}
	IEnumerator WaitAndScroll()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		scrollRect.normalizedPosition = new Vector2(0, 0);

	}
}

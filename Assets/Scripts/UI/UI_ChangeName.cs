using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChangeName : MonoBehaviourPun
{
    [SerializeField] InputField userNameInput;
    public static string default_name = "ㅇㅇ";

    private void OnEnable()
    {
        userNameInput.placeholder.GetComponent<Text>().text = (PhotonNetwork.NickName.Length <= 1) ? default_name : PhotonNetwork.NickName;
    }
    public void OnNameField_Changed()
    {
        string name = userNameInput.text;
        if (name.Length < 1) return;
        if (name.Length > 15) {
            name = name.Substring(0, 15);
        }
        Debug.Assert(UI_PlayerLobbyManager.localPlayerInfo != null, " no local player");
        UI_PlayerLobbyManager.localPlayerInfo.pv.RPC("ChangeName", RpcTarget.AllBuffered, name);
    }
}

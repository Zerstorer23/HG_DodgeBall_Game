using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChangeName : MonoBehaviourPun
{
    [SerializeField] InputField userNameInput;
    public static string default_name = "ㅇㅇ";
    private void Awake()
    {

        userNameInput.placeholder.GetComponent<Text>().text = (PhotonNetwork.NickName.Length <= 1) ? default_name : PhotonNetwork.NickName;
    }
    public void OnNameField_Changed()
    {
        string name = userNameInput.text;
        if (name.Length < 1) return;
        Debug.Assert(MenuManager.localPlayerInfo != null, " no local player");
        MenuManager.localPlayerInfo.pv.RPC("ChangeName", RpcTarget.AllBuffered, userNameInput.text);
    }
}

using Photon.Pun;
using UnityEngine;

public abstract class SkillManager : MonoBehaviourPun
{
    public PhotonView pv;
    protected Unit_Movement unitMovement;
    protected Unit_Player player;
    internal BuffManager buffManager;
    //Data
    public CharacterType myCharacter;

    public int maxStack = 1;
    public float cooltime;
    public bool skillInUse = false;

    public float remainingStackTime;
    public int currStack;

    double lastActivated = 0d;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        unitMovement = GetComponent<Unit_Movement>();
        player = GetComponent<Unit_Player>();
        buffManager = GetComponent<BuffManager>();
        myCharacter = player.myCharacter;
        LoadInformation();
    }



    private void CheckSkillActivation()
    {
        if (PhotonNetwork.Time < lastActivated + 0.4) return;
        if (InputHelper.skillKeyFired() || 
            (GameSession.IsAutoDriving() && unitMovement.autoDriver.CanAttackTarget())   
            )
        {
            if (currStack > 0)
            {
                UI_TouchPanel.isTouching = false;
                lastActivated = PhotonNetwork.Time;
                player.PlayShootAudio();
                pv.RPC("SetSkillInUse", RpcTarget.All, true);
                pv.RPC("ChangeStack", RpcTarget.AllBuffered, -1);
                MySkillFunction();
            }
        }
    }
    public abstract void MySkillFunction();
    public abstract void LoadInformation();

    private void OnEnable()
    {
        InitSkill();
        if (pv.IsMine)
        {
            GameSession.GetInst().skillPanelUI.SetSkillInfo(this);
        }
    }
    public void InitSkill() {
        skillInUse = false;
        remainingStackTime = cooltime; // a;
    }
    [PunRPC]
    public void SetSkillInUse(bool startSkill)
    {
        skillInUse = startSkill;
    }
    [PunRPC]
    public void ChangeStack(int a)
    {
        currStack += a;
    }
    public bool SkillIsReady() {
        return (currStack > 0);
    }
    public bool SkillInUse()
    {
        return (skillInUse);
    }
    public bool IsInvincible()
    {
        return (buffManager.GetTrigger(BuffType.InvincibleFromBullets));
    }
    private void OnDisable()
    {
        skillInUse = false;
    }
    private void Update()
    {
        CheckSkillStack();
        if (!pv.IsMine) return;
        CheckSkillActivation();
    }

    private void CheckSkillStack()
    {
       // Debug.Log(currStack + " " + maxStack + " " + skillInUse);
        if (currStack < maxStack && !skillInUse)
        {
            remainingStackTime -= Time.deltaTime * buffManager.GetBuff(BuffType.Cooltime);
        }
        if (remainingStackTime <= 0)
        {
            if (pv.IsMine)
            {
                pv.RPC("ChangeStack", RpcTarget.AllBuffered, 1);
            }
            remainingStackTime += cooltime;
        }

    }

}
public enum CharacterType
{
    NONE, NAGATO, HARUHI, MIKURU, KOIZUMI, KUYOU, ASAKURA, KYOUKO, KIMIDORI, KYONMOUTO, SASAKI, TSURUYA, KOIHIME, YASUMI
}
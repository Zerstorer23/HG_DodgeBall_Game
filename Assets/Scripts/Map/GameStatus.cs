using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatus
{
    public Player lastSurvivor;
    public Player lastDied;
    public int total;
    public int alive;
    public int alive_ourTeam;
    public int dead;
    public int toKill;
    public GameStatus(SortedDictionary<string, Unit_Player> unitDict, Player lastDied)
    {
        Team myTeam = (Team)UI_PlayerLobbyManager.GetPlayerProperty("TEAM", Team.HOME);
        this.lastDied = lastDied;
        foreach (Unit_Player p in unitDict.Values)
        {
            total++;
            if (p != null && p.gameObject.activeInHierarchy)
            {
                lastSurvivor = p.pv.Owner;
                alive++;
                if (GameSession.gameModeInfo.isTeamGame)
                {
                    if (p.myTeam != myTeam)
                    {
                        toKill++;
                    }
                    else
                    {
                        alive_ourTeam++;
                    }
                }
                else
                {
                    if (p.pv.Owner.UserId != PhotonNetwork.LocalPlayer.UserId)
                    {
                        toKill++;
                    }
                }
            }
            else
            {
                dead++;
            }
        }
        if (lastSurvivor == null) {
            lastSurvivor = lastDied;
        }
    }


      
    

    public override string ToString()
    {
        string o = "Game mode : " + GameSession.gameModeInfo.gameMode + "\n";
        o += "Total Players:" + total + "\n";
        o += "Total Alive:" + alive + "\n";
        o += "Total Alive in my team:" + UI_PlayerLobbyManager.GetPlayerProperty("TEAM", Team.HOME) + " ? " + alive + "\n";
        o += "Total dead:" + dead + "\n";
        o += "Total to kill:" + toKill + "\n";
        o += "Last survivor:" + lastSurvivor + "\n";
        return o;

    }
}
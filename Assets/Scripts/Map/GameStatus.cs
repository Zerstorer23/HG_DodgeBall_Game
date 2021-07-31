using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatus
{
    public UniversalPlayer lastSurvivor;
    public UniversalPlayer lastDied;
    public int total;
    public int alive;
    public int alive_ourTeam;
    public int alive_otherTeam;
    public int dead;
    public bool onlyBotsRemain = true;
    public GameStatus(SortedDictionary<string, Unit_Player> unitDict, UniversalPlayer lastDied)
    {
        Team myTeam = PlayerManager.LocalPlayer.GetProperty("TEAM", Team.HOME);
        this.lastDied = lastDied;
        foreach (Unit_Player p in unitDict.Values)
        {
            total++;
            if (!ConstantStrings.IsInactive(p))
            {
                lastSurvivor = p.controller.Owner;
                alive++;
                if (lastSurvivor.IsHuman) {
                    onlyBotsRemain = false;
                }
                if (GameSession.gameModeInfo.isTeamGame)
                {
                    if (p.myTeam != myTeam)
                    {
                        alive_otherTeam++;
                    }
                    else
                    {
                        alive_ourTeam++;
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
        string o = "<color=#00c8c8>===============GameStat========================</color>\n";
            o+="Game mode : " + GameSession.gameModeInfo.gameMode + "\n";
        o += "Total Players:" + total + "\n";
        o += "Total Alive " + alive + "\n";
        o += "\t \t my team:" + PlayerManager.LocalPlayer.GetProperty("TEAM", Team.HOME) + " ? " + alive_ourTeam + "\n";
        o += "\t \t other team:" + PlayerManager.LocalPlayer.GetProperty("TEAM", Team.HOME) + " ? " + alive_otherTeam + "\n";
        o += "Total dead:" + dead + "\n";
        o += "Last survivor:" + lastSurvivor + "\n";
        o += "<color=#00c8c8>===========================================</color>\n";
        return o;

    }
}
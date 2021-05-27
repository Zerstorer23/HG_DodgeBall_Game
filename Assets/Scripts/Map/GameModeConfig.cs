using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/My Game Mode")]
public class GameModeConfig : ScriptableObject
{
    public GameMode gameMode;
    public bool callSuddenDeath;
    public bool useDesolator;
    public bool scaleMapByPlayerNum = true;
    public bool isTeamGame = false;
    public bool isCoop = false;

    public bool IsFieldFinished(GameStatus stat)
    {
        bool fieldFinished = false;
        switch (GameSession.gameModeInfo.gameMode)
        {
            case GameMode.PVP:
                fieldFinished = (stat.alive <= 1);
                if (fieldFinished && stat.lastDied == null) fieldFinished = false;
                break;
            case GameMode.TEAM:
                fieldFinished = (stat.toKill <= 0 || stat.alive_ourTeam <= 0);
                break;
            case GameMode.PVE:
                fieldFinished = (stat.alive == 0);
                break;
            case GameMode.Tournament:
                fieldFinished = (stat.alive <= 1);
                break;
        }
        return fieldFinished;
    }
}

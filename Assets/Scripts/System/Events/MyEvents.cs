﻿using UnityEngine.Events;


public class MyEvents {
    //HUD Manager
    public const string EVENT_SPAWNER_EXPIRE = "SpanwerExpired";
    public const string EVENT_SPAWNER_SPAWNED = "SpawnerSpawned";
    public const string EVENT_PLAYER_SPAWNED = "PlayerSpawned";
    public const string EVENT_PLAYER_DIED = "PlayerDied";
    public const string EVENT_PLAYER_KILLED_A_PLAYER = "PlayerKilledAPlayer";
    public const string EVENT_MY_PROJECTILE_HIT = "ProjectileHit";
    public const string EVENT_MY_PROJECTILE_MISS = "ProjectileMiss";
    public const string EVENT_BOX_SPAWNED = "BoxSpawned";
    public const string EVENT_BOX_ENABLED = "BoxEnabled";

    //Start Scene
    public const string EVENT_PLAYER_JOINED = "PlayerJoined";
    public static string EVENT_PLAYER_TOGGLE_READY = "PlayerToggleReady";
    public const string EVENT_PLAYER_SELECTED_CHARACTER = "PlayerSelectedCharacter";

    public const string EVENT_GAME_FINISHED = "GameFinished";
    public const string EVENT_FIELD_FINISHED = "FieldFinished";
    public const string EVENT_FIELD_STARTED = "FieldStarted";
    public const string EVENT_GAME_STARTED = "GameStarted";
    public const string EVENT_GAMEMODE_CHANGED = "GameModeChanged";
    public const string EVENT_REQUEST_SUDDEN_DEATH = "SuddenDeathRequested";
    public const string EVENT_SEND_MESSAGE = "SendMessage";
    public const string EVENT_SHOW_PANEL = "ShowPanel";
    public const string EVENT_POP_UP_PANEL = "PopUpPanel";


    public const string EVENT_SCREEN_TOUCH = "ScreenTouched";

}

public class EventOneArg: UnityEvent<EventObject>
{


}


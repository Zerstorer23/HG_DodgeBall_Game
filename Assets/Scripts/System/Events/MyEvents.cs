﻿using UnityEngine.Events;


public enum MyEvents {
    //HUD Manager
    EVENT_SPAWNER_EXPIRE 
    ,EVENT_SPAWNER_SPAWNED 
    ,EVENT_PLAYER_SPAWNED 
    ,EVENT_PLAYER_DIED 
    ,EVENT_PLAYER_KILLED_A_PLAYER 
    ,EVENT_MY_PROJECTILE_HIT 
    ,EVENT_MY_PROJECTILE_MISS 
    ,EVENT_BOX_SPAWNED 
    ,EVENT_BOX_ENABLED 

    //Start Scene
    ,EVENT_PLAYER_JOINED 
    ,EVENT_PLAYER_TOGGLE_READY 
    ,EVENT_PLAYER_SELECTED_CHARACTER 

    ,EVENT_GAME_FINISHED 
    ,EVENT_FIELD_FINISHED 
    ,EVENT_FIELD_STARTED
    ,EVENT_GAME_STARTED 
    ,EVENT_GAMEMODE_CHANGED 
    ,EVENT_REQUEST_SUDDEN_DEATH 
    ,EVENT_SEND_MESSAGE
    ,EVENT_SHOW_PANEL
    ,EVENT_POP_UP_PANEL 
    ,EVENT_GAME_CYCLE_RESTART


    ,EVENT_SCREEN_TOUCH
    ,EVENT_CP_CAPTURED
    ,EVENT_PLAYER_LEFT

    ,EVENT_CHAT_BAN
    ,EVENT_CHAT_MODE
    ,EVENT_JEOPDAE_ENABLE

        , EVENT_LOCALIZATION_LOADED
}

public class EventOneArg: UnityEvent<EventObject>
{


}


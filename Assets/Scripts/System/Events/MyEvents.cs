using UnityEngine.Events;


public class MyEvents {
    //HUD Manager
    public const string EVENT_SPAWNER_EXPIRE = "SpanwerExpired";
    public const string EVENT_PLAYER_SPAWNED = "PlayerSpawned";
    public const string EVENT_PLAYER_DIED = "PlayerDied";

    //Start Scene
    public const string EVENT_PLAYER_JOINED = "PlayerJoined";
    public const string EVENT_PLAYER_LEFT = "PlayerLeft";
    public const string EVENT_PLAYER_SELECTED_CHARACTER = "PlayerSelectedCharacter";

    public const string EVENT_GAME_FINISHED = "GameFinished";
    public const string EVENT_SCENE_CHANGED = "SceneChanged";


}

public class EventOneArg: UnityEvent<EventObject>
{


}


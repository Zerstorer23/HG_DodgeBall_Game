using UnityEngine.Events;


public class MyEvents {
    //HUD Manager
    public const string EVENT_SPAWNER_EXPIRE = "SpanwerExpired";
    public const string EVENT_PLAYER_SPAWNED = "PlayerSpawned";
    public const string EVENT_PLAYER_DIED = "PlayerDied";

    public const string EVENT_PLAYER_JOINED = "PlayerJoined";
    public const string EVENT_PLAYER_LEFT = "PlayerLeft";


}

public class EventOneArg: UnityEvent<EventObject>
{


}


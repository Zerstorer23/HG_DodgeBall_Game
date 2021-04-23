using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventManager : MonoBehaviour
{
    private static EventManager prEvManager;

    public UnitConfig[] unitConfigs;
    public static Dictionary<CharacterType, UnitConfig> unitDictionary;
    public static EventManager eventManager
    {
        get
        {
            if (!prEvManager)
            {
                prEvManager = FindObjectOfType<EventManager>();
                if (!prEvManager)
                {
                }
                else
                {
                    prEvManager.Init();
                }
            }

            return prEvManager;
        }
    }
    private Dictionary<string, EventOneArg> eventDictionary;

    void Init() {

        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, EventOneArg>();
        }
    }

    public static CharacterType GetRandomCharacter()
    {
        int rand = Random.Range(1, eventManager.unitConfigs.Length);
        return eventManager.unitConfigs[rand].characterID;
    }
    private void Awake()
    {
        EventManager[] obj =  FindObjectsOfType<EventManager>();
        if (obj.Length > 1)
        {
            Destroy(gameObject);

        }
        else {
            unitDictionary = new Dictionary<CharacterType, UnitConfig>();
            foreach (UnitConfig u in unitConfigs)
            {
                unitDictionary.Add(u.characterID, u);
            }

            DontDestroyOnLoad(gameObject);
        }
    }

    public EventOneArg GetEvent(string eventName) {

        EventOneArg thisEvent = null;
        eventDictionary.TryGetValue(eventName, out thisEvent);
        return thisEvent;
//       bool found= eventDictionary.TryGetValue(eventName,out thisEvent);

    }
    public void AddEvent(string eventName, EventOneArg thisEvent) {


        eventDictionary.Add(eventName, thisEvent);
    }

    public static void StartListening(string eventName, UnityAction<EventObject> listener)
    {
        if (eventManager == null) return;
        EventOneArg thisEvent = eventManager.GetEvent(eventName);
        if (thisEvent != null)
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new EventOneArg();
            thisEvent.AddListener(listener);
            eventManager.AddEvent(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<EventObject> listener)
    {
        if (eventManager == null) return;
        EventOneArg thisEvent = eventManager.GetEvent(eventName);
        if (thisEvent != null)
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static bool TriggerEvent(string eventName, EventObject variable)
    {
        if (eventManager == null) {
            Debug.LogWarning("On Destroy no EventManager.");
            return false;
        }
        EventOneArg thisEvent =  eventManager.GetEvent(eventName);
        if (thisEvent != null)
        {
            thisEvent.Invoke(variable);
            return true;
        }
        return false;
    }

}





using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    float currentEventCooldown = 0;
    public EventData[] events;

    [Tooltip("How long to wait beforethis becomes active")]
    public float firstTriggerDelay = 180f;

    [Tooltip("How long to wait between each event")]
    public float triggerInterval = 30f;

    public static EventManager instance;

    [System.Serializable]
    public class Event
    {
        public EventData data;
        public float duration,cooldown = 0;
    }
    List<Event> runningEvents = new List<Event>();//these are events that have been activated and are running
    PlayerStats[] allPlayers;

    private void Start()
    {
        if (instance) Debug.LogWarning("there is more than 1 spawn manager in the scene! please remove extras");
        instance = this;
        currentEventCooldown = firstTriggerDelay > 0 ? firstTriggerDelay : triggerInterval;
        allPlayers = FindObjectsOfType<PlayerStats>();
    }
    private void Update()
    {
        //cooldown for adding another event to the slate
        currentEventCooldown -= Time.deltaTime;
        if(currentEventCooldown <= 0)
        {
            //get an event and try to execute it
            EventData e = GetRandomEvent();
            if (e && e.CheckIfWillHappen(allPlayers[Random.Range(0, allPlayers.Length)]))
                runningEvents.Add(new Event
                {
                    data = e,
                    duration = e.duration
                });

            //set the cooldown for the event
            currentEventCooldown = triggerInterval;
        }

        //events that we want to remove
        List<Event> toRemove = new List<Event>();

        //cooldown for existing event to see if they should continue running
        foreach (Event e in runningEvents)
        {
            //reduce the current duration
            e.duration -= Time.deltaTime;
            if(e.duration <= 0)
            {
                toRemove.Add(e);
                continue;
            }

            //reduce the current cooldown
            e.cooldown -= Time.deltaTime;
            if(e.cooldown <= 0)
            {
                //pick a random player to sic this mob on
                //then reset the cooldown
                e.data.Activate(allPlayers[Random.Range(0, allPlayers.Length)]);
                e.cooldown = e.data.GetSpawnInterval();
            }
        }

        //remove all the events that have expired
        foreach (Event e in toRemove) runningEvents.Remove(e);
    }

    public EventData GetRandomEvent()
    {
        //if no events are assigned, don't return anything
        if(events.Length <= 0) return null;

        //get a list of all possible events
        List<EventData> possibleEvents = new List<EventData>(events);

        //add the events in event to the possible events only if the event is active
        foreach (EventData e in events)
        {
            if (e.IsActive())
            {
                possibleEvents.Add(e);
            }
        }
        //randomly pick an event from the possible events to play
        if (possibleEvents.Count > 0)
        {
            EventData result = possibleEvents[Random.Range(0, possibleEvents.Count)];
            return result;
        }
        return null;
    }
}

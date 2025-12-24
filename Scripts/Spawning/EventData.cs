using UnityEngine;

public abstract class EventData : SpawnData
{
    [Header("Event Data")]
    [Range(0f, 1f)] public float probability = 1f; //whether this event occurs.
    [Range(0f, 1f)] public float luckFactor = 1f; // how much luck affects the probability of this event

    [Tooltip("If a value is specified, this event will only occur after the level runs for this number of seconds")]
    public float activeAfter = 0f;

    public abstract bool Activate(PlayerStats player = null);

    //checks whether this event is currently active
    public bool IsActive()
    {
        if(!GameManager.instance) return false;
        if (GameManager.instance.GetElapsedTime() > activeAfter) return true;
        return false;
    }

    //calculates a random probability of this event happening
    public bool CheckIfWillHappen(PlayerStats s)
    {
        //probability of 1 means it always happens.
        if(probability >= 1) return true;

        //otherwise, get a random number and see if we pass the probability test.
        if(probability / Mathf.Max(1,(s.Stats.luck * luckFactor)) >= Random.Range(0f, 1f)) return true;

        return false;
    }
}

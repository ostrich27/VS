using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Stats is a class that is inherited by both PlayerStats and EnemyStats
/// it is here to provide a way for buffs to be applied to both PlayerStats and EnemyStats
/// </summary>
/// 

public abstract class EntityStats : MonoBehaviour
{
    protected float health;

    [System.Serializable]
    public class Buff
    {
        public BuffData data;
        public float remainingDuration, nextTick;
        public int variant;

        public Buff(BuffData d, int variant = 0, float durationMultiplier = 1f)
        {
            data = d;
            BuffData.Stats buffStats = d.Get(variant);
            remainingDuration = buffStats.duration * durationMultiplier;
            nextTick = buffStats.tickInterval;
            this.variant = variant;
        }

        public BuffData.Stats GetData()
        {
            return data.Get(variant);
        }
    }

    protected List<Buff> activeBuffs = new List<Buff>();

    [System.Serializable]
    public class BuffInfo
    {
        public BuffData data;
        public int variant;
        [Range(0f, 1f)] public float probability = 1f;
    }

    //Gets a certain buff from the active buffs list
    //if <variant> is not specified, it only checks whether the buff is there
    //otherwise, we will only get the buff if it is the correct <data> and <variant> values
    public virtual Buff GetBuff(BuffData data, int variant = -1)
    {
        foreach(Buff b in activeBuffs)
        {
            if(b.data == data)
            {
                //if a variant of the buff is specified, we must make
                //sure our buff is the same variant before returning it
                if(variant >= 0)
                {
                    if(b.variant == variant) return b;
                }
                else
                {
                    return b;
                }
            }
        }
        return null;
    }

    //if applying a buff via BuffInfo, we will aslo check the probability first
    public virtual bool ApplyBuff(BuffInfo info,float durationMultiplier = 1f)
    {
        if (Random.value <= info.probability)
            return ApplyBuff(info.data, info.variant, durationMultiplier);
        return false;
    }

    //adds a buff to an entity
    public virtual bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1)
    {
        Buff b;
        BuffData.Stats s = data.Get(variant);

        switch (s.stackType)
        {
            //if the buff stacks fully, then we have multiple copies of the buff
            case BuffData.StackType.stacksFully:
                activeBuffs.Add(new Buff(data, variant, durationMultiplier));
                RecalculateStats();
                return true;

            //if it only refreshes the duration, we will find the buff that is allready there
            //and reset the remaining duration(if the buff is not there yet)
            //otherwise, we just add the buff
            case BuffData.StackType.refreshDurationOnly:
                b = GetBuff(data, variant);
                if(b != null)
                {
                    b.remainingDuration = s.duration * durationMultiplier;
                }
                else
                {
                    activeBuffs.Add(new Buff(data,variant, durationMultiplier));
                    RecalculateStats();
                }
                return true;

            //in cases where buffs do not stack, if the buff already exist, we ignore it
            case BuffData.StackType.doesNotStack:
                b = GetBuff(data, variant);
                if (b != null)
                {
                    activeBuffs.Add(new Buff(data,variant,durationMultiplier));
                    RecalculateStats();
                    return true;
                }
                return false;

        }
        return false;
    }

    //removes all copies of a certain buff
    public virtual bool RemoveBuff(BuffData data, int variant = -1)
    {
        //loop trough all the buffs, and find buffs that we need to remove
        List<Buff> toRemove = new List<Buff>();
        foreach (Buff b in activeBuffs)
        {
            if(b.data == data)
            {
                if(variant >= 0)
                {
                    if (b.variant == variant) toRemove.Add(b);
                }
                else
                {
                    toRemove.Add(b);
                }
            }
        }

        //we need to remove the buffs outside of the loop, otherwise this
        //will cause performance issues with the foreach loop above
        if(toRemove.Count > 0)
        {
            activeBuffs.RemoveAll(item => toRemove.Contains(item));
            RecalculateStats();
            return true;
        }
        return false;
    }

    //Generic take damage function for dealing damage
    public abstract void TakeDamage(float damage);

    //Generic restore health function
    public abstract void RestoreHealth(float ammount);

    //Generic kill function
    public abstract void Kill();

    //forces the entity to recalculate its stats
    public abstract void RecalculateStats();

    protected virtual void Update()
    {
        //counts down each buff and removes them after their remaining duration falls below 0
        List<Buff> expired = new List<Buff>();
        foreach (Buff b in activeBuffs)
        {
            BuffData.Stats s = b.data.Get(b.variant)

                //tick down on the damage / heal timer
                b.nextTick -= Time.deltaTime;
            if(b.nextTick < 0)
            {
                float tickDmg = b.data.GetTickDamage(b.variant);
                if(tickDmg > 0) TakeDamage(tickDmg);
                float tickHeal = b.data.GetTickHeal(b.variant);
                if (tickHeal > 0) RestoreHealth(tickHeal);
                b.nextTick = s.tickInterval;
            }

            //if the buff has a duration of 0 or less, it will stay forever
            //don't reduce remaining duration
            if (s.duration <= 0) continue;

            //also tick down on the remaining buff duration
            b.remainingDuration -= Time.deltaTime;
            if(b.remainingDuration < 0) expired.Add(b);
        }

        //we remove the buffs outside the foreach loop, as it will affect the 
        //iteration if we remove items from the list while a loop is still running
        activeBuffs.RemoveAll(item => expired.Contains(item));
        RecalculateStats();
    }
}

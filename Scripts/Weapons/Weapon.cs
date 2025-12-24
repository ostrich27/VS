using System.Buffers;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// component to be attached to all weapon prefabs.the weapon prefab works together with the WeaponData
/// scriptableobjects to manage and run the behaviours of all weapons in the game.
/// </summary>
/// REPLACEMENT FOR WeaponController.cs

public abstract class Weapon : Item
{
    [System.Serializable]
    public class Stats : LevelData
    {
        [Header("Visuals")]
        public Projectile projectilePrefab;  // if attached projectile will spawn every time weapon cools down
        public Aura auraPrefab;              // if attached aura will be spawn when weapon is equipped
        public ParticleSystem hitEffect,procEffect;
        public Rect spawnVariance;

        [Header("Values")]
        public float lifespan; // if 0 it will last forever
        public float damage,damageVariance,area,speed,cooldown,projectileInterval,knockback;
        public int number,piercing,maxInstances;

        public EntityStats.BuffInfo[] appliedBuffs;

        //allows us to use + operator to add two stats together
        //verry important later when we want to increase our weapon stats
        public static Stats operator +(Stats s1, Stats s2)
        {
            Stats result = new Stats();
            result.name = s2.name?? s1.name;
            result.description = s2.description ?? s1.description;
            result.projectilePrefab = s2.projectilePrefab ?? s1.projectilePrefab;
            result.auraPrefab = s2.auraPrefab?? s1.auraPrefab;
            result.hitEffect = s2.hitEffect == null? s1.hitEffect: s2.hitEffect;
            result.procEffect = s2.procEffect == null ? s1.procEffect : s2.procEffect;
            result.spawnVariance = s2.spawnVariance;
            result.lifespan = s1.lifespan + s2.lifespan;
            result.damage = s1.damage+ s2.damage;
            result.damageVariance = s1.damageVariance+ s2.damageVariance;
            result.area = s1.area+ s2.area;
            result.speed = s1.speed+ s2.speed;
            result.cooldown = s1.cooldown+ s2.cooldown;
            result.number = s1.number + s2.number;
            result.piercing = s1.piercing + s2.piercing;
            result.projectileInterval = s1.projectileInterval + s2.projectileInterval;
            result.knockback = s1.knockback+ s2.knockback;
            result.appliedBuffs = s2.appliedBuffs == null || s2.appliedBuffs.Length <= 0 ? s1.appliedBuffs : s2.appliedBuffs;
            return result;
        }

        //get damage dealt
        public float GetDamage()
        {
            return damage + UnityEngine.Random.Range(0, damageVariance);
        }
    }

    protected Stats currentStats;
    protected float currentCooldown;
    protected PlayerMovement movement; // reference to the player movement


    //for dynamicaly created weapons, call initialise to set everything up
    public virtual void Initialise(WeaponData data)
    {
        print(name + "initialised");
        base.Initialise(data);
        this.data = data;
        currentStats = data.baseStats;
        if (owner != null)
        {
            movement = owner.GetComponent<PlayerMovement>();
        }
        else
        {
            movement= GetComponent<PlayerMovement>();
        }
        ActivateCooldown();
    }




    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            Attack(currentStats.number + owner.Stats.ammount);
        }
    }
    

    //levels up weapon by 1, and calculates corresponding stats
    public override bool DoLevelUp(bool updateUI = true)
    {
        base.DoLevelUp(updateUI);
        //prevents level up if we allready at max level
        if (!CanLevelUp())
        {
            Debug.LogWarning(string.Format("cannot level up {0} to level {1}, max level {2} allready reached", name, currentLevel, data.maxLevel));
            return false;
        }
        //otherwise add stats of next level to our weapon
        currentStats += (Stats)data.GetLevelData(++currentLevel);
        return true;
    }

    // lets us check whether this weapon can attack at this current moment
    protected virtual bool CanAttack()
    {
        if(Mathf.Approximately(owner.Stats.might, 0)) return false;

        return currentCooldown <= 0f;
    }


    //performs an attack with the weapon
    //returns true if attack was successful
    //this does not do anything we have to override this at the child class to add behaviour
    protected virtual bool Attack(int attackCount = 1)
    {
        if(CanAttack())
        {
            ActivateCooldown();
            return true;
        }
        return false;
    }


    //gets the amount of damage that the weapon is supposed to deal
    //factoring in the weapon's stats (include damage variance)
    //as well as character's Might stat
    public virtual float GetDamage()
    {
        return currentStats.GetDamage() * owner.Stats.might;
    }

    //get the area, including modifications from the player's stats
    public virtual float GetArea()
    {
        return currentStats.area * owner.Stats.area;
    }

    //for retrieving weapon's stats
    public virtual Stats GetStats()
    {
        return currentStats;
    }

    public virtual bool ActivateCooldown(bool strict = false)  //if strict is true, will activate cooldown if the weapon is allready cooled down, otherwise will activate cooldown even if the weapon has not cooled down yet
    {
        //if <strict> is enabled and the cooldown is not yet finished, do not refresh the cooldown

        if(strict && currentCooldown > 0f) return false;

        //calculate what the cooldown is going to be, factoring in the cooldown reduction stat in the player character

        float actualCooldown = currentStats.cooldown * Owner.Stats.cooldown;

        //limit the maximum cooldown to actual cooldown, so we cannot increase the cooldown above the cooldown stat, if we accidentally call this function multiple times

        currentCooldown = Mathf.Min(actualCooldown, currentCooldown + actualCooldown);
        return true;
    }

    //makes the weapon apply its buff to a targeted EntityStats object
    public void ApplyBuffs(EntityStats e)
    {
        //apply all assigned buffs to the target
        Stats stats = GetStats();
        if (stats.appliedBuffs != null)
        {
            foreach(EntityStats.BuffInfo b in stats.appliedBuffs)
            {
                e.ApplyBuff(b, owner.Actual.duration);
            }
        }
    }
}

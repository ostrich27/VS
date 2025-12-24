using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : EntityStats
{
    [System.Serializable]
    public struct Resistances
    {
        [Range(-1f, 1f)] public float freeze, kill, debuff;

        //to allow us to multiply the resistances
        public static Resistances operator *(Resistances r, float factor)
        {
            r.freeze = Mathf.Min(1, r.freeze * factor);
            r.kill = Mathf.Min(1, r.kill * factor);
            r.debuff = Mathf.Min(1, r.debuff * factor);
            return r;
        }

        public static Resistances operator +(Resistances r, Resistances r2)
        {
            r.freeze += r2.freeze;
            r.kill += r2.kill;
            r.debuff += r2.debuff;
            return r;
        }

        //allows us to multiply resistances by one another, for multiplicative buffs
        public static Resistances operator *(Resistances r1, Resistances r2)
        {
            r1.freeze = Mathf.Min(1, r1.freeze * r2.freeze);
            r1.kill = Mathf.Min(1, r1.kill * r2.kill);
            r1.debuff = Mathf.Min(1, r1.debuff * r2.debuff);
            return r1;
        }
    }

    [System.Serializable]
    public struct Stats
    {
        public float maxHealth, moveSpeed, damage, knockbackMultiplier;
        public Resistances resistances;

        [System.Flags]
        public enum Boostable { health = 1, moveSpeed = 2, damage = 4, knockbackMultiplier = 8, resistances = 16 }
        public Boostable curseBoosts, levelBoosts;

        private static Stats Boost( Stats s1, float factor, Boostable boostable)
        {
            if((boostable & Boostable.health) != 0) s1.maxHealth *= factor;
            if((boostable & Boostable.moveSpeed) != 0) s1.moveSpeed *= factor;
            if((boostable & Boostable.damage) != 0) s1.damage *= factor;
            if((boostable & Boostable.knockbackMultiplier)!= 0) s1.knockbackMultiplier /= factor;
            if((boostable & Boostable.resistances)!= 0) s1.resistances *= factor;
            return s1;
        }

        //use the multiply operator for curse
        public static Stats operator *(Stats s1, float factor) { return Boost(s1, factor, s1.curseBoosts); }

        //use the XOR operator for level boosted stats
        public static Stats operator ^(Stats s1, float factor) { return Boost(s1, factor, s1.levelBoosts); }

        //use the add operator to add stats to the enemy
        public static Stats operator +(Stats s1, Stats s2)
        {
            s1.maxHealth += s2.maxHealth;
            s1.moveSpeed += s2.moveSpeed;
            s1.damage += s2.damage;
            s1.knockbackMultiplier += s2.knockbackMultiplier;
            s1.resistances += s2.resistances;
            return s1;
        }

        //use the multiply operator to scale stats
        //used by the buff / debuff system
        public static Stats operator *(Stats s1, Stats s2)
        {
            s1.maxHealth *= s2.maxHealth;
            s1.moveSpeed *= s2.moveSpeed;
            s1.damage *= s2.damage;
            s1.knockbackMultiplier *= s2.knockbackMultiplier;
            s1.resistances *= s2.resistances;
            return s1;
        }
    }

    public Stats baseStats = new Stats { maxHealth = 10, moveSpeed = 1, damage = 3, knockbackMultiplier = 1 };
    Stats actualStats;
    public Stats Actual
    {
        get {  return actualStats; }
    }


    public BuffInfo[] attackEffects;

    [Header("Damage Feedback")]
    public Color damageColor; // color to flash when taking damage
    public float damageFlashDuration = 0.2f; // duration of the damage flash
    public float deathFadeTime = 0.6f; // time it takes to fade out on death
    EnemyMovement movement;

    public static int count; // track the number of enemies on the screen


    private void Awake()
    {
        count++;
    }

    protected override void Start()
    {
        base.Start();
        if (UILevelSelector.globalBuff && !UILevelSelector.globalBuffAffectsEnemies)
            ApplyBuff(UILevelSelector.globalBuff);
        RecalculateStats();
        health = actualStats.maxHealth;
        movement = GetComponent<EnemyMovement>();
    }

    public override bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1)
    {
        //if the debuff is a freeze, we check for freeze resistance
        //roll a number and if it succeeds, we ignore the freeze
        if((data.type & BuffData.Type.freeze) > 0)
            if (Random.value <= Actual.resistances.freeze) return false;

        //if the debuff is a debuff, we check for debuff resistance
        if((data.type & BuffData.Type.debuff) > 0)
            if(Random.value <= Actual.resistances.debuff) return false;

        return base.ApplyBuff(data, variant, durationMultiplier);
    }

    //calculates the actual stats of the enemy based on a variety of factors
    public override void RecalculateStats()
    {
        //calculate curse boosts
        float curse = GameManager.GetCumulativeCurse(),
            level = GameManager.GetCumulativeLevels();
        actualStats = (baseStats * curse) ^ level;

        //create a variable to store all the cumulative multiplier values
        Stats multiplier = new Stats
        {
            maxHealth = 1f,
            moveSpeed = 1f,
            damage = 1f,
            knockbackMultiplier = 1,
            resistances = new Resistances { freeze = 1f, debuff = 1f, kill = 1f }
        };

        foreach(Buff b in activeBuffs)
        {
            BuffData.Stats bd = b.GetData();
            switch (bd.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += bd.enemyModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    multiplier *= bd.enemyModifier;
                    break;
            }
        }
        //apply the multipliers last
        actualStats *= multiplier;
    }

    public override void TakeDamage(float dmg)
    {
        health -= dmg;

        //if damage is exactly equal to maximum health, we assume it is an insta-kill and
        //check for the kill resistance to see if we can dodge this damage
        if(dmg == actualStats.maxHealth)
        {
            //roll a dice to check if we can dodge the damage.
            //gets a random value between 0 to 1, and if the number is 
            //bellow the kill resistance, then we avoid getting killed
            if(Random.value < actualStats.resistances.kill)
            {
                return; //don't take damage
            }
        }

        //create the text popup when enemy takes damage
        if(dmg > 0)
        {
            StartCoroutine(DamageFlash());
            GameManager.GenerateFloatingText(Mathf.FloorToInt(dmg).ToString(), transform);
        }

        //kills the enemy if the health drops below zero
        if(health <= 0)
        {
            Kill();
        }
    }

    /// <summary>
    /// this function always needs at least 2 values, the amount of damage dealt<paramref name="dmg"/>, as well as where the damage
    /// is coming from, which is passed as <paramref name="sourcePosition"/>. the <paramref name="sourcePosition"/> is necessary because it is
    /// used to calculate the direction of knockback
    /// </summary>


    public void TakeDamage(float dmg, Vector2 sourcePosition,float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        TakeDamage(dmg);

        //apply knockback if it is not zero
        if(knockbackForce > 0)
        {
            //get the direction of knockback
            Vector2 dir = (Vector2)transform.position - sourcePosition;
            movement.Knockback(dir.normalized * knockbackForce, knockbackDuration);
        }
    }

    public override void RestoreHealth(float ammount)
    {
        if(health < actualStats.maxHealth)
        {
            health += ammount;
            if(health > actualStats.maxHealth)
            {
                health = actualStats.maxHealth;
            }
        }
    }

    // this is coroutine that makes the enemy flash when taking damage
    IEnumerator DamageFlash()
    {
        ApplyTint(damageColor);
        yield return new WaitForSeconds(damageFlashDuration);
        RemoveTint(damageColor);
    }

    public override void Kill()
    {
        //enable drops if the enemy is killed, since drops are disabled by default
        DropRateManager drops = GetComponent<DropRateManager>();
        if (drops) drops.active = true;
        StartCoroutine(KillFade());
    }

    // this is coroutine that makes the enemy fade out slowly when killed
    IEnumerator KillFade()
    {
        // wait for a single frame
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0, origAlpha = sprite.color.a;

        //this is a loop that fires every frame
        while (t < deathFadeTime)
        {
            yield return w;
            t += Time.deltaTime;

            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, (1 - t / deathFadeTime) * origAlpha);
        }
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (Mathf.Approximately(Actual.damage, 0)) return;


        if(col.collider.TryGetComponent(out PlayerStats p))
        {
            p.TakeDamage(actualStats.damage);
            foreach(BuffInfo b in attackEffects)
                p.ApplyBuff(b);
        }
    }

    private void OnDestroy()
    {
        count--;
    }
}

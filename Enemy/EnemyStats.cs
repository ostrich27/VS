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
    }

    public Stats baseStats = new Stats { maxHealth = 10, moveSpeed = 1, damage = 3, knockbackMultiplier = 1 };
    Stats actualStats;
    public Stats Actual
    {
        get {  return actualStats; }
    }


    [Header("Damage Feedback")]
    public Color damageColor; // color to flash when taking damage
    public float damageFlashDuration = 0.2f; // duration of the damage flash
    public float deathFadeTime = 0.6f; // time it takes to fade out on death
    Color originalColor;
    SpriteRenderer sr;
    EnemyMovement movement;

    public static int count; // track the number of enemies on the screen


    private void Awake()
    {
        count++;
    }

    void Start()
    {
        RecalculateStats();
        health = actualStats.maxHealth;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        movement = GetComponent<EnemyMovement>();
    }

    //calculates the actual stats of the enemy based on a variety of factors
    public override void RecalculateStats()
    {
        //we have to account for the buffs from EntityStats as well
        foreach(Buff b in activeBuffs)
        {
            actualStats += b.GetData().enemyModifier;
        }

        //calculate curse boosts
        float curse = GameManager.GetCumulativeCurse(),
            level = GameManager.GetCumulativeLevels();
        actualStats = (baseStats * curse) ^ level;
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
        sr.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);
        sr.color = originalColor;
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
        float t = 0, origAlpha = sr.color.a;

        //this is a loop that fires every frame
        while (t < deathFadeTime)
        {
            yield return w;
            t += Time.deltaTime;

            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, (1 - t / deathFadeTime) * origAlpha);
        }
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if(col.collider.TryGetComponent(out PlayerStats p))
        {
            p.TakeDamage(actualStats.damage);
        }
    }

    private void OnDestroy()
    {
        count--;
    }
}

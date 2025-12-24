using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : WeaponEffect
{
    Dictionary<EnemyStats, float> affectedTargets = new Dictionary<EnemyStats, float>();
    List<EnemyStats> targetsToUnaffect = new List<EnemyStats>();


    private void Update()
    {
        Dictionary<EnemyStats, float> affectedTargetsCopy = new Dictionary<EnemyStats, float>(affectedTargets);

        //loop trough every target affected by the aura, and reduce the cooldown
        //of the aura for it. if the cooldown reaches 0, deal damage to it.
        foreach (KeyValuePair<EnemyStats, float> pair in affectedTargetsCopy)
        {
            affectedTargets[pair.Key] -= Time.deltaTime;
            if (pair.Value <= 0)
            {
                if (targetsToUnaffect.Contains(pair.Key))
                {
                    //if the target is marked for removal, remove it.
                    affectedTargets.Remove(pair.Key);
                    targetsToUnaffect.Remove(pair.Key);
                }
                else
                {
                    //reset the cooldown and deal damage
                    Weapon.Stats stats = weapon.GetStats();
                    affectedTargets[pair.Key] = stats.cooldown * Owner.Stats.cooldown;
                    pair.Key.TakeDamage(GetDamage(), transform.position, stats.knockback);


                    weapon.ApplyBuffs(pair.Key); // apply all assigned buffs to the target

                    //play the hit effect if it is assigned
                    if (stats.hitEffect)
                    {
                        Destroy(Instantiate(stats.hitEffect, pair.Key.transform.position, Quaternion.identity), 5f);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            //if the target is not  yet affected by this aura, add it
            //to our list of affected tarets
            if (!affectedTargets.ContainsKey(es))
            {
                //allways start with an interval of 0, so that it will get
                //damaged in next update() tick
                affectedTargets.Add(es, 0);
            }
            else
            {
                if (targetsToUnaffect.Contains(es))
                {
                    targetsToUnaffect.Remove(es);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.TryGetComponent(out EnemyStats es))
        {
            //do not directly remove the target upon leaving
            //because we will still have to track their cooldowns
            if (affectedTargets.ContainsKey(es))
            {
                targetsToUnaffect.Add(es);
            }
        }
    }

    public virtual void OnEquip()
    {

    }
}

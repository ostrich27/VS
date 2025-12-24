using System.Collections.Generic;
using UnityEngine;

//damage does not scale with might currently
public class Lighting : ProjectileWeapon
{
    List<EnemyStats> allSelectedEnemies = new List<EnemyStats>();

    protected override bool Attack(int attackCount = 1)
    {
        //if no projectile prefab is assigned, leave a warning message
        if (!currentStats.hitEffect)
        {
            Debug.LogWarning(string.Format("hit effect prefab has not been set for {0}", name));
            ActivateCooldown();
            return false;
        }

        //if there is no projectile assigned, set the weapon on cooldown
        if(!CanAttack()) return false;

        //if the cooldown is less than 0, this is first firing of the weapon
        //refresh the array of sellected enemies
        if(currentCooldown <= 0)
        {
            allSelectedEnemies = new List<EnemyStats>(FindObjectsOfType<EnemyStats>());
            ActivateCooldown();
            currentAttackCount = attackCount;
        }


        //find an enemy on the map to strike with lighting
        EnemyStats target = PickEnemy();
        if (target)
        {
            DamageArea(target.transform.position, GetArea(), GetDamage());
            Instantiate(currentStats.hitEffect, target.transform.position,Quaternion.identity);
        }


        //if there is proc effect, play it on the player
        if (currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

        //if we have more than 1 attack count

        if (attackCount > 0)
        {
            currentAttackCount = attackCount - 1;
            currentAttackInterval = currentStats.projectileInterval;
        }

        return true;
    }

    //randomly picks an enemy on the screen
    EnemyStats PickEnemy()
    {
        EnemyStats target = null;
        while(!target && allSelectedEnemies.Count > 0)
        {
            int idx = Random.Range(0, allSelectedEnemies.Count);
            target = allSelectedEnemies[idx];

            //if target is allready dead, remove  and skip it
            if (!target)
            {
                allSelectedEnemies.RemoveAt(idx);
                continue;
            }

            //check if enemy is on screen
            //if enemy is missing a renderer, it cannot be struck, as we cannot check whether it is on the screen or not
            Renderer r = target.GetComponent<Renderer>();
            if(!r || !r.isVisible)
            {
                allSelectedEnemies.Remove(target);
                target = null;
                continue;
            }
        }

        allSelectedEnemies.Remove(target);
        return target;
    }

    void DamageArea(Vector2 position,float radius,float damage)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(position,radius);
        foreach(Collider2D t in targets)
        {
            EnemyStats es = t.GetComponent<EnemyStats>();
            if (es)
            {
                es.TakeDamage(damage, transform.position);
                ApplyBuffs(es);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whip : ProjectileWeapon
{
    int currentSpawnCount; //how many times whip has been attacking in this iteration
    float currentSpawnYOffset; // if there are more than 2 whips, we will start offsetin it upwards


    protected override bool Attack(int attackCount = 1)
    {
        //if no projectile prefab is assigned, leave a warning
        if(!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("projectile prefab is not set for{0}", name));
            ActivateCooldown(true);
            return false;
        }

        //if there is no projectile assigned, set the weapon on cooldown
        if(!CanAttack()) return false;

        //if this is the first time the attack has been fired
        //we reset the currentSpawnCount
        if(currentCooldown <= 0)
        {
            currentSpawnCount = 0;
            currentSpawnYOffset = 0f;
        }


        //otherwise calculate angle and offset of our spawned projectile.
        //then if <currentSpawnCount> is even (i.e more than 1 projectile)
        //we flip the direction of the spawn

        float spawnDir = Mathf.Sign(movement.lastMovedVector.x) * (currentSpawnCount % 2 != 0 ? 1 : -1);
        Vector2 spawnOffset = new Vector2(spawnDir* Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax), currentSpawnYOffset);

        //if there is procEffect, play it on the player
        if (currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

            //and spawn a copy of the projectile
            Projectile prefab = Instantiate(currentStats.projectilePrefab,owner.transform.position + (Vector3)spawnOffset, Quaternion.identity);
        prefab.owner = owner; //set ourselves to be the owner

        //flip the projectile's sprite
        if(spawnDir < 0)
        {
            prefab.transform.localScale = new Vector3(-Mathf.Abs(prefab.transform.localScale.x), prefab.transform.localScale.y, prefab.transform.localScale.z);
        }


        //assign the stats
        prefab.weapon = this;
        ActivateCooldown(true);
        attackCount--;

        //determine where the next projectile should spawn
        currentSpawnCount++;
        if(currentSpawnCount > 1 && currentSpawnCount % 2 == 0)
        {
            currentSpawnYOffset += 1;
        }

        //do we perform another attack?
        if(attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = ((WeaponData)data).baseStats.projectileInterval;
        }

        return true;

    }
}

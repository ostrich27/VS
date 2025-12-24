using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Component that you attach to all projectile prefabs. all spawned projectiles will fly in the direction
/// they are facing and deal damage when they hit an object.
/// </summary>
/// 

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : WeaponEffect
{
    public enum DamageSource {projectile,owner }
    public DamageSource damageSource = DamageSource.projectile;
    public bool hasAutoAim = false;
    public Vector3 rotationSpeed = new Vector3(0,0,0);

    protected Rigidbody2D rb;
    protected int piercing;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Weapon.Stats stats = weapon.GetStats();
        if(rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.angularVelocity = rotationSpeed.z;
            rb.velocity = transform.right * stats.speed * weapon.Owner.Stats.speed;
        }

        //prevents the area from being 0, as it hides projectile.
        float area = weapon.GetArea();
        if (area <= 0) area = 1;
        transform.localScale = new Vector3(area * Mathf.Sign(transform.localScale.x), area * Mathf.Sign(transform.localScale.y), 1);

        //set how much piercing this object has
        piercing = stats.piercing;

        //destroy projectile after lifespan expires
        if(stats.lifespan > 0) { Destroy(gameObject, stats.lifespan); }

        //if the projectile is auto-aiming, automatically find suitable enemy
        if (hasAutoAim) { AcquireAutoAimFacing(); }
    }

    // if the projectile is homing, it will automatically find a suitable target to move towards
    public virtual void AcquireAutoAimFacing()
    {
        float aimAngle; //we need to determine where to aim

        //find all enemies on the screen
        EnemyStats[] targets = FindObjectsOfType<EnemyStats>();

        // select a random enemy(if there is at least 1)
        //otherwise, pick a random angle
        if(targets.Length > 0)
        {
            EnemyStats selectedTarget = targets[Random.Range(0, targets.Length)];
            Vector2 difference = selectedTarget.transform.position - transform.position;
            aimAngle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        }
        else
        {
            aimAngle = Random.Range(0f, 360f);
        }

        transform.rotation = Quaternion.Euler(0, 0, aimAngle);
    }

    protected virtual void FixedUpdate()
    {

        //only drive movement ourselves if this is a kinematic
        if(rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Weapon.Stats stats = weapon.GetStats();
            transform.position += transform.right * stats.speed *weapon.Owner.Stats.speed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position);
            transform.Rotate(rotationSpeed * Time.fixedDeltaTime);
        }
    }


    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        EnemyStats es = other.GetComponent<EnemyStats>();
        BreakableProps p = other.GetComponent<BreakableProps>();

        //only collide  with enemies or breakable stuff
        if (es)
        {
            //if there is an owner, and the damage source is set to owner,
            //we will calculate knockback using the owner instead of the projectile
            Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;

            //deals damage
            es.TakeDamage(GetDamage(), source);

            //get the weapon's stats
            Weapon.Stats stats = weapon.GetStats();

            weapon.ApplyBuffs(es); // apply all assigned buffs to the target
            piercing--;
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity), 5f);
            }
        }
        else if (p)
        {
            p.TakeDamage(GetDamage());
            piercing--;

            Weapon.Stats stats = weapon.GetStats() ;
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity), 5f);
            }
        }

        if(piercing <= 0) Destroy(gameObject);
    }
}

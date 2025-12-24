using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyMovement : Sortable
{
    protected EnemyStats stats;
    protected Transform player;
    protected Rigidbody2D rb;

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOfFrameAction {none, respawnAtEdge, despawn }
    public OutOfFrameAction outOfFrameAction = OutOfFrameAction.respawnAtEdge;

    [System.Flags]
    public enum KnockbackVariance { duration = 1, velocity = 2 }
    public KnockbackVariance knockbackVariance = KnockbackVariance.velocity;

    protected bool spawnedOutOfFrame = false;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        spawnedOutOfFrame = !SpawnManager.IsWithinBoundaries(transform);
        stats = GetComponent<EnemyStats>();

        //picks a random player on the screen, instead of always picking the 1st player
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
    }

    protected virtual void Update()
    {
        // if we are currently being knocked back, then process knockback
        if (knockbackDuration > 0)
        {
            transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            Move();
            HandleOutOfFrameAction();
        }
    }


    //if the nemy falls outside of the frame,handle it.
    protected virtual void HandleOutOfFrameAction()
    {
        if (!SpawnManager.IsWithinBoundaries(transform))
        {
            switch (outOfFrameAction)
            {
                case OutOfFrameAction.none: default:
                    break;
                    case OutOfFrameAction.respawnAtEdge:
                    //if the enemy is outside the camera frame, teleport it back to the edge of the frame.
                    transform.position = SpawnManager.GeneratePosition();
                    break;
                case OutOfFrameAction.despawn:
                    //don't destroy if it is spawned outside the frame.
                    if (!spawnedOutOfFrame)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }else spawnedOutOfFrame = false;
    }

    public virtual void Knockback(Vector2 velocity, float duration)
    {
        // ignore the knockback if the duration is greather than 0
        if(knockbackDuration > 0) return;

        //ignore knockback if the knockback type is set to none
        if (knockbackVariance == 0) return;

        //only change the factor if the multiplier is not 0 or 1
        float pow = 1;
        bool reducesVelocity = (knockbackVariance & KnockbackVariance.velocity) > 0,
             reducesDuration = (knockbackVariance & KnockbackVariance.duration) > 0;

        if (reducesVelocity && reducesDuration) pow = 0.5f;

        //check which knockback values to affect
        knockbackVelocity = velocity * (reducesVelocity ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
        knockbackDuration = duration * (reducesDuration ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);

    }

    public virtual void Move()
    {
        //if there is a rigidbody, use it to move instead of moving position directly. this optimizes performance
        if (rb)
        {
            rb.MovePosition(Vector2.MoveTowards(
                rb.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime)
                );
        }
        else
        {
            //constantly move the enemy towards the player
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime
                );
        }
    }
}

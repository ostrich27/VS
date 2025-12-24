using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : Sortable
{

    public float lifespan = 0.5f; //in seconds
    protected PlayerStats target; // if the pickup has a target move towards the target
    protected float speed; //the speed at which the pickup travels
    Vector2 initialPosition;
    float initialOffset;

    //to represent the bobbing animation of the object
    [System.Serializable]
    public struct BobbingAnimation
    {
        public float frequency;
        public Vector2 direction;
    }
    public BobbingAnimation bobbingAnimation = new BobbingAnimation()
    {
        frequency = 2f,
        direction = new Vector2(0f, 0.3f)
    };

    [Header("Bonuses")]
    public int experience;
    public int health;

    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        initialOffset = Random.Range(0, bobbingAnimation.frequency);
    }
    public virtual void Update()
    {
        if (target)
        {
            //move towards the player
            Vector2 distance = target.transform.position - transform.position;
            if (distance.sqrMagnitude > speed * speed * Time.deltaTime)
            {
                transform.position += (Vector3)distance.normalized * speed * Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            //handle animation of object
            transform.position = initialPosition + bobbingAnimation.direction * Mathf.Sin((Time.time + initialOffset) * bobbingAnimation.frequency);
        }
    }


    public virtual bool Collect(PlayerStats target, float speed, float lifespan = 0f)
    {
        if(!this.target)
        {
            this.target = target;
            this.speed = speed;
            if(lifespan > 0f) this.lifespan = lifespan;
            Destroy(gameObject,Mathf.Max(0.01f, this.lifespan));
            return true;
        }
        return false;
    }

    protected virtual void OnDestroy()
    {
        if(!target) return;
        if(experience != 0) target.IncreaseExperience(experience);
        if(health != 0) target.RestoreHealth(health);
    }
}

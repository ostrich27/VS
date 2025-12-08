using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D detector;

    public float pullSpeed;


    void Start()
    {
        player = GetComponentInParent<PlayerStats>();
    }

    public void SetRadius(float r)
    {
        if(!detector) detector = GetComponent<CircleCollider2D>();
        detector.radius = r;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //check if other gameobject is a Pickup
        if (col.gameObject.TryGetComponent(out Pickup p))
        {
            p.Collect(player,pullSpeed);
        }
    }
}

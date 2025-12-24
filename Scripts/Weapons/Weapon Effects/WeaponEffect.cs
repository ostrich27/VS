using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// replaces WeaponBehaviour.cs
/// A gameobject that is spawned as an effect of a weapon firing, e.g. projectiles, auras, pulses.
/// </summary>
public abstract class WeaponEffect : MonoBehaviour
{
    [HideInInspector] public PlayerStats owner;
    [HideInInspector] public Weapon weapon;

    public PlayerStats Owner { get { return owner; } }

    public float GetDamage()
    {
        return weapon.GetDamage();
    }
}

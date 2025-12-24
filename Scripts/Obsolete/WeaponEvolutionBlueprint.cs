using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Obsolete("obsolete")]

[CreateAssetMenu(fileName = "WeaponEvolutionBlueprint", menuName = "ScriptableObjects/Weapon Evolution Blueprint")]
public class WeaponEvolutionBlueprint : ScriptableObject
{

    public WeaponScripcableObject baseWeaponData;
    public PassiveItemsScriptableObject catalystPassiveItemData;
    public WeaponScripcableObject evolvedWeaponData;
    public GameObject evolvedWeapon;
}

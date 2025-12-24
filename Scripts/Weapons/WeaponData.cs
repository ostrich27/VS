using UnityEngine;
/// <summary>
/// replacement for WeaponScriptableObject class. the idea is we want to store all weapon evolution
/// data in one single object, instead of having multiple scriptable objects to store a weapon, which is 
/// what we had to do if continued using WeaponScriptableObject.
/// </summary>

[CreateAssetMenu(fileName = "Weapon Data", menuName = "2D Top-Down Rogue-Like/Weapon Data")]
public class WeaponData : ItemData
{

    [HideInInspector] public string behaviour;
    public Weapon.Stats baseStats;
    public Weapon.Stats[] linearGrowth;
    public Weapon.Stats[] randomGrowth;

    // gives us the stat growth/ description of the next level
    public override Item.LevelData GetLevelData(int level)
    {
        if (level <= 1) return baseStats;

        //pick the stats from next level
        if (level - 2 < linearGrowth.Length)
        {
            return linearGrowth[level - 2];
        }
        //otherwise, pick one of the stats from the random growth array
        if (randomGrowth.Length > 0) return randomGrowth[Random.Range(0, randomGrowth.Length)];

        //return an empty value and waarning
        Debug.LogWarning(string.Format("weapon doesn't have its level up stats configured for level {0}",level));
        return new Weapon.Stats();
    }
}

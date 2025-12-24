using UnityEngine;
/// <summary>
/// replacement for the PassiveItemScriptableObject class. the idea is that we want to store all
/// passive item level data in a single object, instead of having multiple objects to store 
/// a single passive item, which is what we had to do if we continued using PassiveItemScriptableObject
/// </summary>
/// 

[CreateAssetMenu(fileName = "Passive Data", menuName = "2D Top-Down Rogue-Like/Passive Data")]
public class PassiveData : ItemData
{
    public Passive.Modifier baseStats;
    public Passive.Modifier[] growth;

    public override Item.LevelData GetLevelData(int level)
    {
        if(level <= 1) return baseStats;
        if (level - 2 < growth.Length)
            return growth[level - 2];

        //return an empty value and warning
        Debug.LogWarning(string.Format("passive doesn't have its level up stats configured for level {0}",level));
        return new Passive.Modifier();
    }
}

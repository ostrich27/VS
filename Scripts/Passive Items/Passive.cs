using UnityEngine;

/// <summary>
/// a class that takes PassiveData and is used to increment a player's stats when received
/// </summary>
public class Passive : Item
{
    [SerializeField] CharacterData.Stats currentBoosts;


    [System.Serializable]

    public class Modifier : LevelData
    {
        public CharacterData.Stats boosts;
    }

    // for dynamically created passives, call initialise to set everything up
    public virtual void Initialise(PassiveData passiveData)
    {
        base.Initialise(passiveData);
        this.data = passiveData;
        currentBoosts = passiveData.baseStats.boosts;
    }

    public virtual CharacterData.Stats GetBoosts()
    {
        return currentBoosts;
    }

    //levels up weapon by 1 and calculates the corresponding stats
    public override bool DoLevelUp(bool updateUI = true)
    {
        base.DoLevelUp(updateUI);

        //prevents level up if we already at max level
        if (!CanLevelUp())
        {
            Debug.LogWarning(string.Format("cannot level up {0} to level {1}, max level {2} already reached", name, currentLevel, data.maxLevel));
            return false;
        }

        //otherwise add stats of next level to our weapon
        currentBoosts += ((Modifier)data.GetLevelData(++currentLevel)).boosts;
        return true;
    }
}

using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles permanent, account-wide stat upgrades that are bought in the main menu
/// and applied on top of CharacterData stats for every run.
/// </summary>
public static class MetaUpgradeManager
{
    public enum UpgradeType
    {
        MaxHealth,
        Recovery,
        Armor,
        MoveSpeed,
        Might,
        area,
        speed,
        duration,
        ammount,
        cooldown,
        luck,
        growth,
        greed,
        curse,
        Magnet,
        revival
    }

    // ---- CONFIG ----

    // Base cost for the first level of each upgrade.
    private const int BASE_COST = 100;

    // Additional cost per already-owned level (simple linear scaling).
    private const int COST_PER_LEVEL = 200;

    // How much each level gives in terms of character stats.
    private const float HEALTH_PER_LEVEL   = 10f;
    private const float RECOVERY_PER_LEVEL = 0.2f;
    private const float ARMOR_PER_LEVEL    = 0.1f;
    private const float MOVE_SPEED_PER_LEVEL = 0.1f;
    private const float MIGHT_PER_LEVEL    = 0.5f;
    private const float AREA_PER_LEVEL = 0.1f;
    private const float SPEED_PER_LEVEL = 0.1f;
    private const float DURATION_PER_LEVEL = 0.1f;
    private const int AMMOUNT_PER_LEVEL = 1;
    private const float COOLDOWN_PER_LEVEL = 0.1f;
    private const float LUCK_PER_LEVEL = 0.1f;
    private const float GROWTH_PER_LEVEL = 0.1f;
    private const float GREED_PER_LEVEL = 0.1f;
    private const float CURSE_PER_LEVEL = 0.1f;
    private const float MAGNET_PER_LEVEL   = 0.5f;
    private const int Revival_PER_LEVEL = 1;


    private const int MAX_HEALTH_MAX_LEVEL = 15;
    private const int RECOVERY_MAX_LEVEL = 5;
    private const int ARMOR_MAX_LEVEL = 10;
    private const int MOVE_SPEED_MAX_LEVEL = 10;
    private const int MIGHT_MAX_LEVEL = 10;

    private const int AREA_MAX_LEVEL = 10;
    private const int SPEED_MAX_LEVEL = 10;
    private const int DURATION_MAX_LEVEL = 10;
    private const int AMMOUNT_MAX_LEVEL = 10;
    private const int COOLDOWN_MAX_LEVEL = 10;
    private const int LUCK_MAX_LEVEL = 10;
    private const int GROWTH_MAX_LEVEL = 10;
    private const int GREED_MAX_LEVEL = 10;
    private const int CURSE_MAX_LEVEL = 10;

    private const int MAGNET_MAX_LEVEL = 3;
    private const int REVIVAL_MAX_LEVEL = 1;
    // ---- PUBLIC API ----

    public static int GetLevel(UpgradeType type)
    {
        var data = SaveManager.LastLoadedGameData;
        switch (type)
        {
            case UpgradeType.MaxHealth: return data.maxHealthLevel;
            case UpgradeType.Recovery:  return data.recoveryLevel;
            case UpgradeType.Armor:     return data.armorLevel;
            case UpgradeType.MoveSpeed: return data.moveSpeedLevel;
            case UpgradeType.Might:     return data.mightLevel;
            case UpgradeType.area:      return data.areaLevel;
            case UpgradeType.speed:     return data.speedLevel;
            case UpgradeType.duration:  return data.durationLevel;
            case UpgradeType.ammount:   return data.ammountLevel;
            case UpgradeType.cooldown:  return data.cooldownLevel;
            case UpgradeType.luck:      return data.luckLevel;
            case UpgradeType.growth:    return data.growthLevel;
            case UpgradeType.greed:     return data.greedLevel;
            case UpgradeType.curse:     return data.curseLevel;
            case UpgradeType.Magnet:    return data.magnetLevel;
            case UpgradeType.revival:   return data.revivalLevel;
            default:                    return 0;
        }
    }

    public static float GetUpgradeValue(UpgradeType type)
    {
        var data = SaveManager.LastLoadedGameData;
        switch (type)
        {
            case UpgradeType.MaxHealth: return data.maxHealthLevel * HEALTH_PER_LEVEL;
            case UpgradeType.Recovery:  return data.recoveryLevel * RECOVERY_PER_LEVEL;
            case UpgradeType.Armor:     return data.armorLevel * ARMOR_PER_LEVEL;
            case UpgradeType.MoveSpeed: return data.moveSpeedLevel * MOVE_SPEED_PER_LEVEL;
            case UpgradeType.Might:     return data.mightLevel * MIGHT_PER_LEVEL;
            case UpgradeType.area:      return data.areaLevel * AREA_PER_LEVEL;
            case UpgradeType.speed:     return data.speedLevel * SPEED_PER_LEVEL;
            case UpgradeType.duration:  return data.durationLevel * DURATION_PER_LEVEL;
            case UpgradeType.ammount:   return data.ammountLevel * AMMOUNT_PER_LEVEL;
            case UpgradeType.cooldown:  return data.cooldownLevel * COOLDOWN_PER_LEVEL;
            case UpgradeType.luck:      return data.luckLevel * LUCK_PER_LEVEL;
            case UpgradeType.growth:    return data.growthLevel * GROWTH_PER_LEVEL;
            case UpgradeType.greed:     return data.greedLevel * GREED_PER_LEVEL;
            case UpgradeType.curse:     return data.curseLevel * CURSE_PER_LEVEL;
            case UpgradeType.Magnet:    return data.magnetLevel * MAGNET_PER_LEVEL;
            case UpgradeType.revival:   return data.revivalLevel * Revival_PER_LEVEL;
            default:                    return 0f;
        }
    }

    public static int GetMaxLevel(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.MaxHealth: return MAX_HEALTH_MAX_LEVEL;
            case UpgradeType.Recovery:  return RECOVERY_MAX_LEVEL;
            case UpgradeType.Armor:     return ARMOR_MAX_LEVEL;
            case UpgradeType.MoveSpeed: return MOVE_SPEED_MAX_LEVEL;
            case UpgradeType.Might:     return MIGHT_MAX_LEVEL;
            case UpgradeType.area:      return AREA_MAX_LEVEL;
            case UpgradeType.speed:     return SPEED_MAX_LEVEL;
            case UpgradeType.duration:  return DURATION_MAX_LEVEL;
            case UpgradeType.ammount:   return AMMOUNT_MAX_LEVEL;
            case UpgradeType.cooldown:  return COOLDOWN_MAX_LEVEL;
            case UpgradeType.luck:      return LUCK_MAX_LEVEL;
            case UpgradeType.growth:    return GROWTH_MAX_LEVEL;
            case UpgradeType.greed:     return GREED_MAX_LEVEL;
            case UpgradeType.curse:     return CURSE_MAX_LEVEL;
            case UpgradeType.Magnet:    return MAGNET_MAX_LEVEL;
            case UpgradeType.revival:   return REVIVAL_MAX_LEVEL;
            default: return 0;
        }
    }

    public static int GetNextLevelCost(UpgradeType type)
    {
        int currentLevel = GetLevel(type);
        return BASE_COST + currentLevel * COST_PER_LEVEL;
    }

    /// <summary>
    /// Attempts to purchase an upgrade level using saved coins.
    /// Returns true if the purchase succeeded.
    /// </summary>
    public static bool TryPurchaseUpgrade(UpgradeType type)
    {
        var data = SaveManager.LastLoadedGameData;
        int cost = GetNextLevelCost(type);

        // cap check
        if (GetLevel(type) >= GetMaxLevel(type))
            return false;

        if (data.coins < cost)
            return false;

        data.coins -= cost;

        switch (type)
        {
            case UpgradeType.MaxHealth:
                data.maxHealthLevel++;
                break;
            case UpgradeType.Recovery:
                data.recoveryLevel++;
                break;
            case UpgradeType.Armor:
                data.armorLevel++;
                break;
            case UpgradeType.MoveSpeed:
                data.moveSpeedLevel++;
                break;
            case UpgradeType.Might:
                data.mightLevel++;
                break;
            case UpgradeType.area:
                data.areaLevel++;
                break;
            case UpgradeType.speed:
                data.speedLevel++;
                break;
            case UpgradeType.duration:
                data.durationLevel++;
                break;
            case UpgradeType.ammount:
                data.ammountLevel++;
                break;
            case UpgradeType.cooldown:
                data.cooldownLevel++;
                break;
            case UpgradeType.luck:
                data.luckLevel++;
                break;
            case UpgradeType.growth:
                data.growthLevel++;
                break;
            case UpgradeType.greed:
                data.greedLevel++;
                break;
            case UpgradeType.curse:
                data.curseLevel++;
                break;
            case UpgradeType.Magnet:
                data.magnetLevel++;
                break;
            case UpgradeType.revival:
                data.revivalLevel++;
                break;
        }

        SaveManager.Save(data);
        return true;
    }

    /// <summary>
    /// Converts all meta-upgrade levels into a CharacterData.Stats bonus
    /// that can be added on top of a character's base stats.
    /// </summary>
    public static CharacterData.Stats GetMetaStats()
    {
        var data = SaveManager.LastLoadedGameData;
        CharacterData.Stats s = new CharacterData.Stats();

        if (data.maxHealthLevel > 0)
            s.maxHealth += data.maxHealthLevel * HEALTH_PER_LEVEL;

        if (data.recoveryLevel > 0)
            s.recovery += data.recoveryLevel * RECOVERY_PER_LEVEL;

        if (data.armorLevel > 0)
            s.armor += data.armorLevel * ARMOR_PER_LEVEL;

        if (data.moveSpeedLevel > 0)
            s.moveSpeed += data.moveSpeedLevel * MOVE_SPEED_PER_LEVEL;

        if (data.mightLevel > 0)
            s.might += data.mightLevel * MIGHT_PER_LEVEL;

        if (data.areaLevel > 0)
            s.area += data.areaLevel * AREA_PER_LEVEL;

        if (data.speedLevel > 0)
            s.speed += data.speedLevel * SPEED_PER_LEVEL;

        if (data.durationLevel > 0)
            s.duration += data.durationLevel * DURATION_PER_LEVEL;

        if (data.ammountLevel > 0)
            s.ammount += data.ammountLevel * AMMOUNT_PER_LEVEL;

        if (data.cooldownLevel > 0)
            s.cooldown += data.cooldownLevel * COOLDOWN_PER_LEVEL;

        if (data.luckLevel > 0)
            s.luck += data.luckLevel * LUCK_PER_LEVEL;

        if (data.growthLevel > 0)
            s.growth += data.growthLevel * GROWTH_PER_LEVEL;

        if (data.greedLevel > 0)
            s.greed += data.greedLevel * GREED_PER_LEVEL;

        if (data.curseLevel > 0)
            s.curse += data.curseLevel * CURSE_PER_LEVEL;

        if (data.magnetLevel > 0)
            s.magnet += data.magnetLevel * MAGNET_PER_LEVEL;

        if (data.revivalLevel > 0)
            s.revival += data.revivalLevel * Revival_PER_LEVEL;

        return s;
    }
}



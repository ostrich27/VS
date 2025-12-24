using TMPro;
using UnityEngine;

/// <summary>
/// Simple main-menu UI for buying permanent stat upgrades
/// using coins stored in SaveManager.
/// Wire button OnClick events to the public methods here.
/// </summary>
public class UIMetaUpgradeMenu : MonoBehaviour
{
    [Header("Level Labels")]
    public TextMeshProUGUI maxHealthLevelText;
    public TextMeshProUGUI recoveryLevelText;
    public TextMeshProUGUI armorLevelText;
    public TextMeshProUGUI moveSpeedLevelText;
    public TextMeshProUGUI mightLevelText;
    public TextMeshProUGUI areaLevelText;
    public TextMeshProUGUI speedLevelText;
    public TextMeshProUGUI durationLevelText;
    public TextMeshProUGUI ammountLevelText;
    public TextMeshProUGUI cooldownLevelText;
    public TextMeshProUGUI luckLevelText;
    public TextMeshProUGUI growthLevelText;
    public TextMeshProUGUI greedLevelText;
    public TextMeshProUGUI curseLevelText;
    public TextMeshProUGUI magnetLevelText;
    public TextMeshProUGUI revivalLevelText;

    [Header("Upgrade Value Labels")]
    public TextMeshProUGUI maxHealthValueText;
    public TextMeshProUGUI recoveryValueText;
    public TextMeshProUGUI armorValueText;
    public TextMeshProUGUI moveSpeedValueText;
    public TextMeshProUGUI mightValueText;
    public TextMeshProUGUI areaValueText;
    public TextMeshProUGUI speedValueText;
    public TextMeshProUGUI durationValueText;
    public TextMeshProUGUI ammountValueText;
    public TextMeshProUGUI cooldownValueText;
    public TextMeshProUGUI luckValueText;
    public TextMeshProUGUI growthValueText;
    public TextMeshProUGUI greedValueText;
    public TextMeshProUGUI curseValueText;
    public TextMeshProUGUI magnetValueText;
    public TextMeshProUGUI revivalValueText;

    [Header("Cost Labels")]
    public TextMeshProUGUI maxHealthCostText;
    public TextMeshProUGUI recoveryCostText;
    public TextMeshProUGUI armorCostText;
    public TextMeshProUGUI moveSpeedCostText;
    public TextMeshProUGUI mightCostText;
    public TextMeshProUGUI areaCostText;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI durationCostText;
    public TextMeshProUGUI ammountCostText;
    public TextMeshProUGUI cooldownCostText;
    public TextMeshProUGUI luckCostText;
    public TextMeshProUGUI growthCostText;
    public TextMeshProUGUI greedCostText;
    public TextMeshProUGUI curseCostText;
    public TextMeshProUGUI magnetCostText;
    public TextMeshProUGUI revivalCostText;

    [Header("Coins Display (optional override)")]
    public TextMeshProUGUI coinsText;

    private void OnEnable()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        RefreshLevels();
        RefreshCosts();
        RefreshCoins();
        RefreshUpgradeValue();
    }

    void RefreshLevels()
    {
        if (maxHealthLevelText)
            maxHealthLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.MaxHealth).ToString();
        if (recoveryLevelText)
            recoveryLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.Recovery).ToString();
        if (armorLevelText)
            armorLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.Armor).ToString();
        if (moveSpeedLevelText)
            moveSpeedLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.MoveSpeed).ToString();
        if (mightLevelText)
            mightLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.Might).ToString();
        if (areaLevelText)
            areaLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.area).ToString();
        if (speedLevelText)
            speedLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.speed).ToString();
        if (durationLevelText)
            durationLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.duration).ToString();
        if (ammountLevelText)
            ammountLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.ammount).ToString();
        if (cooldownLevelText)
            cooldownLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.cooldown).ToString();
        if (luckLevelText)
            luckLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.luck).ToString();
        if (growthLevelText)
            growthLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.growth).ToString();
        if (greedLevelText)
            greedLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.greed).ToString();
        if (curseLevelText)
            curseLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.curse).ToString();
        if (magnetLevelText)
            magnetLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.Magnet).ToString();
        if (revivalLevelText)
            revivalLevelText.text = "Lvl: " + MetaUpgradeManager.GetLevel(MetaUpgradeManager.UpgradeType.revival).ToString();
    }

    void RefreshUpgradeValue()
    {
        if (maxHealthValueText)
            maxHealthValueText.text = "HP: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.MaxHealth).ToString("0.#");
        if (recoveryValueText)
            recoveryValueText.text = "Recovery: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.Recovery).ToString("0.#");
        if (armorValueText)
            armorValueText.text = "Armor: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.Armor).ToString("0.#");
        if (moveSpeedValueText)
            moveSpeedValueText.text = "Move Speed: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.MoveSpeed).ToString("0.#");
        if (mightValueText)
            mightValueText.text = "Might: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.Might).ToString("0.#");
        if (areaValueText)
            areaValueText.text = "Area: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.area).ToString("0.#");
        if (speedValueText)
            speedValueText.text = "Speed: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.speed).ToString("0.#");
        if (durationValueText)
            durationValueText.text = "Duration: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.duration).ToString("0.#");
        if (ammountValueText)
            ammountValueText.text = "Ammount: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.ammount).ToString("0.#");
        if (cooldownValueText)
            cooldownValueText.text = "Cooldown: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.cooldown).ToString("0.#");
        if (luckValueText)
            luckValueText.text = "Luck: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.luck).ToString("0.#");
        if (growthValueText)
            growthValueText.text = "Growth: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.growth).ToString("0.#");
        if (greedValueText)
            greedValueText.text = "Greed: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.greed).ToString("0.#");
        if (curseValueText)
            curseValueText.text = "Curse: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.curse).ToString("0.#");
        if (magnetValueText)
            magnetValueText.text = "Magnet: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.Magnet).ToString("0.#");
        if (revivalValueText)
            revivalValueText.text = "Revival: + " + MetaUpgradeManager.GetUpgradeValue(MetaUpgradeManager.UpgradeType.revival).ToString("0.#");
    }

    void RefreshCosts()
    {
        if (maxHealthCostText)
            maxHealthCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.MaxHealth).ToString();
        if (recoveryCostText)
            recoveryCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.Recovery).ToString();
        if (armorCostText)
            armorCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.Armor).ToString();
        if (moveSpeedCostText)
            moveSpeedCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.MoveSpeed).ToString();
        if (mightCostText)
            mightCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.Might).ToString();
        if (areaCostText)
            areaCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.area).ToString();
        if (speedCostText)
            speedCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.speed).ToString();
        if (durationCostText)
            durationCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.duration).ToString();
        if (ammountCostText)
            ammountCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.ammount).ToString();
        if (cooldownCostText)
            cooldownCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.cooldown).ToString();
        if (luckCostText)
            luckCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.luck).ToString();
        if (growthCostText)
            growthCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.growth).ToString();
        if (greedCostText)
            greedCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.greed).ToString();
        if (curseCostText)
            curseCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.curse).ToString();
        if (magnetCostText)
            magnetCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.Magnet).ToString();
        if (revivalCostText)
            revivalCostText.text = MetaUpgradeManager.GetNextLevelCost(MetaUpgradeManager.UpgradeType.revival).ToString();
    }

    void RefreshCoins()
    {
        if (!coinsText) return;

        float coins = SaveManager.LastLoadedGameData.coins;
        coinsText.text = Mathf.RoundToInt(coins).ToString();
    }

    public void OnUpgradeMaxHealth()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.MaxHealth))
            RefreshAll();
    }

    public void OnUpgradeRecovery()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.Recovery))
            RefreshAll();
    }

    public void OnUpgradeArmor()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.Armor))
            RefreshAll();
    }

    public void OnUpgradeMoveSpeed()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.MoveSpeed))
            RefreshAll();
    }

    public void OnUpgradeMight()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.Might))
            RefreshAll();
    }

    public void OnUpgradeArea()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.area))
            RefreshAll();
    }

    public void OnUpgradeSpeed()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.speed))
            RefreshAll();
    }

    public void OnUpgradeDuration()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.duration))
            RefreshAll();
    }

    public void OnUpgradeAmmount()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.ammount))
            RefreshAll();
    }

    public void OnUpgradeCooldown()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.cooldown))
            RefreshAll();
    }

    public void OnUpgradeLuck()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.luck))
            RefreshAll();
    }

    public void OnUpgradeGrowth()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.growth))
            RefreshAll();
    }

    public void OnUpgradeGreed()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.greed))
            RefreshAll();
    }

    public void OnUpgradeCurse()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.curse))
            RefreshAll();
    }

    public void OnUpgradeMagnet()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.Magnet))
            RefreshAll();
    }

    public void OnUpgradeRevival()
    {
        if (MetaUpgradeManager.TryPurchaseUpgrade(MetaUpgradeManager.UpgradeType.revival))
            RefreshAll();
    }
}



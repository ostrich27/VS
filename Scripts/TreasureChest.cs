using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [System.Flags]
    public enum DropType
    {
        NewPassive = 1, NewWeapon = 2, UpgradePassive = 4,
        UpgradeWeapon = 8, Evolution = 16
    }
    public DropType possibleDrops = (DropType)~0;

    public enum DropCountType { sequential, random }
    public DropCountType dropCountType = DropCountType.sequential;
    public TreasureChestDropProfile[] dropProfiles;
    public static int totalPickups = 0;
    int currentDropProfileIndex = 0;
    public Sprite defaultDropSprite;

    PlayerInventory recipient;


    public TreasureChestDropProfile GetCurrentDropProfile()
    {
        return dropProfiles[currentDropProfileIndex];
    }

    //get a drop profile from a list of drop profiles assigned to the treasure chest
    public TreasureChestDropProfile GetNextDropProfile()
    {
        if (dropProfiles == null || dropProfiles.Length == 0)
        {
            Debug.LogWarning("Drop profiles not set.");
            return null;
        }

        switch (dropCountType)
        {
            case DropCountType.sequential:
                currentDropProfileIndex = Mathf.Clamp(
                    totalPickups, 0,
                    dropProfiles.Length - 1
                    );
                break;

            case DropCountType.random:
                float playerLuck = recipient.GetComponentInChildren<PlayerStats>().Actual.luck;

                //build list of profiles with computed weight
                List<(int index, TreasureChestDropProfile profile, float weight)> weightedProfiles = new List<(int, TreasureChestDropProfile, float)>();
                for (int i = 0; i < dropProfiles.Length; i++)
                {
                    float weight = dropProfiles[i].baseDropChance * (1 + dropProfiles[i].luckScaling * (playerLuck - 1));
                    weightedProfiles.Add((i, dropProfiles[i], weight));
                }

                //sort by weight ascending (smallest first)
                weightedProfiles.Sort((a, b) => a.weight.CompareTo(b.weight));


                //compute total weight
                float totalWeight = 0f;
                foreach (var entry in weightedProfiles)
                    totalWeight += entry.weight;


                //random roll and cumulative selection
                float r = Random.Range(0, totalWeight);
                float cumulative = 0f;
                foreach (var entry in weightedProfiles)
                {
                    cumulative += entry.weight;
                    if (r < cumulative)
                    {
                        currentDropProfileIndex = entry.index;
                        return entry.profile;
                    }
                }
                break;
        }
        return GetCurrentDropProfile();
    }

    //get the number of rewards the treasure chest provides, retrieved from the assigned drop profiles
    private int GetRewardCount()
    {
        TreasureChestDropProfile dp = GetNextDropProfile();
        if (dp) return dp.noOfItems;
        return 1;
    }

    //try to evolve a random item in the inventory
    T TryEvolve<T>(PlayerInventory inventory, bool updateUI = true) where T : Item
    {
        //loop trough every evolvable item
        T[] evolvables = inventory.GetEvolvables<T>();
        foreach (Item i in evolvables)
        {
            //get all the evolutions that are possible
            ItemData.Evolution[] possibleEvolutions = i.CanEvolve(0);
            foreach (ItemData.Evolution e in possibleEvolutions)
            {
                //attempt the evolution and notify the UI if successful
                if (i.AttemptEvolution(e, 0, updateUI))
                {
                    UITreasureChest.NotifyItemReceived(e.outcome.itemType.icon);
                    return i as T;
                }
            }
        }
        return null;
    }

    //try to upgrade a random item in the inventory
    T TryUpgrade<T>(PlayerInventory inventory, bool updateUI = true) where T : Item
    {
        //gets all weapons in the inventory that can still level up
        T[] upgradables = inventory.GetUpgradables<T>();
        if (upgradables.Length == 0) return null; // terminate if no weapons

        //do the level up, and tell the treaasure chest which item is levelled
        T t = upgradables[Random.Range(0, upgradables.Length)];
        inventory.LevelUp(t, updateUI);
        UITreasureChest.NotifyItemReceived(t.data.icon);
        return t;
    }

    //try to give a new item to the inventory
    //T here represents an ItemData type (WeaponData, PassiveData, etc.)
    T TryGive<T>(PlayerInventory inventory, bool updateUI = true) where T : ItemData
    {
        //cannot give new item if slots are full
        if (inventory.GetSlotsLeftFor<T>() <= 0) return null;
        //get all new item possibilities
        T[] possibilities = inventory.GetUnowned<T>();
        if (possibilities.Length == 0) return null;

        //add a random possibility
        T t = possibilities[Random.Range(0, possibilities.Length)];
        inventory.Add(t, updateUI); // uses PlayerInventory.Add(ItemData)
        UITreasureChest.NotifyItemReceived(t.icon);
        return t;
    }

    //function for UITreasureChest to call when animation is complete
    public void NotifyComplete()
    {
        recipient.weaponUI.Refresh();
        recipient.passiveUI.Refresh();
    }

    // For each reward slot: pick a random action from the allowed types and try it.
    // If the chosen action isn't possible, remove it and try another until one succeeds or none are left.
    // After a successful action, rebuild the full action set for the next slot (so earlier failures can succeed later).

    void Open(PlayerInventory inventory)
    {
        if (inventory == null) return;

        // Build the base list of action factories (we will re-create candidates for each slot)
        var baseActionFactories = new List<System.Func<System.Func<bool>>>();

        if (possibleDrops.HasFlag(DropType.Evolution))
        {
            baseActionFactories.Add(() => () => TryEvolve<Weapon>(inventory, false) != null);
        }
        if (possibleDrops.HasFlag(DropType.UpgradeWeapon))
        {
            baseActionFactories.Add(() => () => TryUpgrade<Weapon>(inventory, false) != null);
        }
        if (possibleDrops.HasFlag(DropType.UpgradePassive))
        {
            baseActionFactories.Add(() => () => TryUpgrade<Passive>(inventory, false) != null);
        }
        if (possibleDrops.HasFlag(DropType.NewWeapon))
        {
            baseActionFactories.Add(() => () => TryGive<WeaponData>(inventory, false) != null);
        }
        if (possibleDrops.HasFlag(DropType.NewPassive))
        {
            baseActionFactories.Add(() => () => TryGive<PassiveData>(inventory, false) != null);
        }

        // If there are no possible action types, fallback immediately
        if (baseActionFactories.Count == 0)
        {
            if (defaultDropSprite) UITreasureChest.NotifyItemReceived(defaultDropSprite);
            return;
        }

        // For each reward slot, attempt a random action
        int rewardCount = GetRewardCount();
        for (int slot = 0; slot < rewardCount; slot++)
        {
            // recreate the candidate action list for this slot (so previously-failed actions are re-eligible next slot)
            var candidates = new List<System.Func<bool>>();
            foreach (var factory in baseActionFactories) candidates.Add(factory());

            bool slotSucceeded = false;
            while (candidates.Count > 0)
            {
                int pick = Random.Range(0, candidates.Count);
                var action = candidates[pick];
                bool succeeded = false;
                try
                {
                    succeeded = action();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("TreasureChest action threw: " + ex.Message);
                    succeeded = false;
                }

                if (succeeded)
                {
                    slotSucceeded = true;
                    break; // move to next reward slot
                }

                // remove failed candidate and try another
                candidates.RemoveAt(pick);
            }

            if (!slotSucceeded)
            {
                // nothing could be given for this slot
                if (defaultDropSprite) UITreasureChest.NotifyItemReceived(defaultDropSprite);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out PlayerInventory p))
        {
            //save the recipient and start up the ui
            recipient = p;

            //rewards will be given first
            int rewardCount = GetRewardCount();
            for (int i = 0; i < rewardCount; i++)
            {
                Open(p);
            }
            gameObject.SetActive(false);

            UITreasureChest.Activate(p.GetComponentInChildren<PlayerCollector>(), this);

            //increment first, then wrap around if necessary
            totalPickups = (totalPickups + 1) % (dropProfiles.Length + 1);
        }
    }
}
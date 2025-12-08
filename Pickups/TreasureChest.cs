using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        PlayerInventory p = col.GetComponent<PlayerInventory>();
        if (p)
        {
            bool randomBool = Random.Range(0, 2) == 0;
            OpenTreasureChest(p, randomBool);
            Destroy(gameObject);
        }
    }

    public void OpenTreasureChest(PlayerInventory inventory, bool isHigherTier)
    {
        //loop trough every weapon to check whether it can evolve   
        foreach(PlayerInventory.Slot s in inventory.weaponSlots)
        {
            Weapon w = s.item as Weapon;
            if (w.data.evolutionData == null) continue; //ignore weapon if it cannot evolve

            //loop trough every possible evolution of the weapon
            foreach(ItemData.Evolution e in w.data.evolutionData)
            {
                //only attempt to evolve weapons via treasure chest evolution
                if(e.condition == ItemData.Evolution.Condition.treasureChest)
                {
                    bool attempt = w.AttemptEvolution(e, 0);
                    if (attempt) return; // if evolution suceeds, stop.
                }
            }
        }
    }
}

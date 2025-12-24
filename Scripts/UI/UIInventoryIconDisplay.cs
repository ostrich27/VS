using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LayoutGroup))]
public class UIInventoryIconDisplay : MonoBehaviour
{
    public GameObject slotTemplate;
    public uint maxSlots = 6;
    public bool showLevels = true;
    public PlayerInventory inventory;

    public GameObject[] slots;

    [Header("Paths")]
    public string iconPath;
    public string levelTextPath;
    [HideInInspector] public string targetedItemList;

    private void Reset()
    {
        slotTemplate = transform.GetChild(0).gameObject;
        inventory = FindObjectOfType<PlayerInventory>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    //this will read the inventory and see if there are any new updates
    //to the items on the PlayerCharacter

    public void Refresh()
    {
        if (!inventory) Debug.LogWarning("no inventory attached to the UI icon display");

        //figure out which inventory i want
        Type t = typeof(PlayerInventory);
        FieldInfo field = t.GetField(targetedItemList, BindingFlags.Public | BindingFlags.Instance);

        //if the given field is not found, then show a warning
        if (field == null)
        {
            Debug.LogWarning("the list in the inventory is not found");
            return;
        }

        //get the list of inventory slots
        List<PlayerInventory.Slot> items = (List<PlayerInventory.Slot>)field.GetValue(inventory);

        //start populating the icons
        for (int i = 0; i < items.Count; i++)
        {
            //check if we have enough slots for the item
            //otherwise let's pring a warning so that our users set this component up properly
            if (i >= slots.Length)
            {
                Debug.LogWarning(string.Format("you have {0} inventory slots, but only {1} slots on the UI.",items.Count, slots.Length));
                break;
            }

            //get the item data
            Item item = items[i].item;

            Transform iconObj = slots[i].transform.Find(iconPath);
            if (iconObj)
            {
                Image icon = iconObj.GetComponentInChildren<Image>();

                //if the item doesn't exist, make the icon transparent
                if (!item) icon.color = new Color(1, 1, 1, 0);
                else
                {
                    //otherwise make it visible and update the icon
                    icon.color = new Color(1, 1, 1, 1);
                    if (icon) icon.sprite = item.data.icon;
                }
            }

            //set the level as well
            Transform levelObj = slots[i].transform.Find(levelTextPath);
            if (levelObj)
            {
                //find the text component and put the level inside
                TextMeshProUGUI levelTxt = levelObj.GetComponentInChildren<TextMeshProUGUI>();
                if (levelTxt)
                {
                    if (!item || !showLevels) levelTxt.text = "";
                    else levelTxt.text = item.currentLevel.ToString();
                }
            }
        }
    }
}

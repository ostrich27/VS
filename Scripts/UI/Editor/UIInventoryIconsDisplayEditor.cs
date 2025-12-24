using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

[CustomEditor(typeof(UIInventoryIconDisplay))]
public class UIInventoryIconDisplayEditor : Editor
{
    UIInventoryIconDisplay display;
    int targetedItemListIndex = 0;
    string[] itemListOptions;

    //this fires whenever we select a GameObject containing the UIInventoryIconDisplay component
    //the function scans the PlayerInventory script to find all variable of the type List<PlayerInventory.slot>
    private void OnEnable()
    {
        //get access to the component, as we will need to set the targetedItemList variable on it
        display = target as UIInventoryIconDisplay;

        //get the type object for the PlayerInventory class
        Type playerInventoryType = typeof(PlayerInventory);

        //get all fields of the PlayerInventory class
        FieldInfo[] fields = playerInventoryType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //list to store variables of type List<PlayerInventory.Slot>
        //use LINQ to filter fields of type List<PlayerInventory.Slot> and select their names
        List<string> slotListNames = fields
            .Where(field => field.FieldType.IsGenericType && 
            field.FieldType.GetGenericTypeDefinition() == typeof(List<>) && 
            field.FieldType.GetGenericArguments()[0] == typeof(PlayerInventory.Slot))
            .Select(field => field.Name)
            .ToList();

        slotListNames.Insert(0, "None");
        itemListOptions = slotListNames.ToArray();

        //ensure that we are using the correct weapon subtype
        targetedItemListIndex = math.max(0, Array.IndexOf(itemListOptions, display.targetedItemList));
    }

    //this function draws the inspector
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck(); //begin checking for changes

        //draw a dropdown in the inspector
        targetedItemListIndex = EditorGUILayout.Popup("Targeted Item List", Mathf.Max(0, targetedItemListIndex), itemListOptions);

        if(EditorGUI.EndChangeCheck() )
        {
            display.targetedItemList = itemListOptions[targetedItemListIndex].ToString();
            EditorUtility.SetDirty(display); // marks the object to save
        }

        if (GUILayout.Button("Generate Icons")) RegenerateIcons();
    }

    //regenerate the icons based on the slotTemplate
    //fires when the Generate Icons button is cliced on the inspector
    void RegenerateIcons()
    {
        display = target as UIInventoryIconDisplay;

        //register the entire function call as undoable
        Undo.RegisterCompleteObjectUndo(display, "Regenerate Icons");

        if(display.slots.Length > 0 )
        {
            //destroy all the children in the previous slots
            foreach(GameObject g in  display.slots)
            {
                if(!g) continue;
                
                //otherwise remove it and record it as an undoable action
                if(g != display.slotTemplate) Undo.DestroyObjectImmediate(g);
            }
        }

        //destroy all other children except for the slot template
        for (int i = 0; i < display.transform.childCount; i++)
        {
            if (display.transform.GetChild(i).gameObject == display.slotTemplate) continue;
            Undo.DestroyObjectImmediate(display.transform.GetChild(i).gameObject);
            i--;
        }

        if (display.maxSlots <= 0) return; //terminate if there are no slots

        //create all the new children
        display.slots = new GameObject[display.maxSlots];
        display.slots[0] = display.slotTemplate;
        for (int i = 1;i < display.slots.Length; i++)
        {
            display.slots[i] = Instantiate(display.slotTemplate, display.transform);
            display.slots[i].name = display.slotTemplate.name;
        }
    }
}

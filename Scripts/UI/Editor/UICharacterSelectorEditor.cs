using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[CustomEditor(typeof(UICharacterSelector))]
public class UICharacterSelectorEditor : Editor
{
    UICharacterSelector selector;

    private void OnEnable()
    {
        //point to the UICharacterSelector when it's in the inspector so its variables can be acessed
        selector = target as UICharacterSelector;

    }

    public override void OnInspectorGUI()
    {
        //create a button in the inspector with the name, that creates the toggle templates when clicked
        base.OnInspectorGUI();
        if(GUILayout.Button("Generate Selectable Characters"))
        {
            CreateTogglesForCharacterData();
        }
    }
    public void CreateTogglesForCharacterData()
    {
        //if the toggle template is not assigned, leave a warning and abort
        if (!selector.toggleTemplate)
        {
            Debug.LogWarning("please assign a toggle template for the UI Character Selector First.");
                return;
        }

        //loop trough all the children of the parent of the toggle template,
        //and deleting everything under it except the template
        for(int i = selector.toggleTemplate.transform.parent.childCount -1; i >= 0; i--)
        {
            Toggle tog = selector.toggleTemplate.transform.parent.GetChild(i).GetComponent<Toggle>();
            if(tog == selector.toggleTemplate) continue;
            Undo.DestroyObjectImmediate(tog.gameObject); // record the action so we can undo
        }

        //record the changes made to the UICharacterSelector component as undoable and clears the toggle list
        Undo.RecordObject(selector, "Updates to UICharacterSelector.");
        selector.selectableToggles.Clear();
        CharacterData[] characters = UICharacterSelector.GetAllCharacterDataAssets();

        //for every character data asset in the project, we create a toggle for them in the character selector
        for(int i =0; i<characters.Length; i++)
        {
            Toggle tog;
            if(i == 0)
            {
                tog = selector.toggleTemplate;
                Undo.RecordObject(tog, "Modifying the template");
            }
            else
            {
                tog = Instantiate(selector.toggleTemplate, selector.toggleTemplate.transform.parent); //create a toggle of the current character as a child of the original template's parent
                Undo.RegisterCreatedObjectUndo(tog.gameObject, "Created a new toggle.");
            }

            //finding the character name, icon and weapon icon to assign
            Transform characterName = tog.transform.Find(selector.characterNamePath);
            if(characterName && characterName.TryGetComponent(out TextMeshProUGUI tmp))
            {
                tmp.text = tog.gameObject.name = characters[i].name;
            }

            Transform characterIcon = tog.transform.Find(selector.characterIconPath);
            if(characterIcon && characterIcon.TryGetComponent(out Image chrIcon))
                chrIcon.sprite = characters[i].Icon;

            Transform weaponIcon = tog.transform.Find(selector.weaponIconPath);
            if(weaponIcon && weaponIcon.TryGetComponent(out Image wpnIcon))
                wpnIcon.sprite = characters[i].StartingWeapon.icon;

            selector.selectableToggles.Add(tog);

            //remove all select events and add our own event that checks which character toggle was clicked
            for(int j = 0; j < tog.onValueChanged.GetPersistentEventCount(); j++)
            {
                if(tog.onValueChanged.GetPersistentMethodName(j) == "Select")
                {
                    UnityEventTools.RemovePersistentListener(tog.onValueChanged, j);
                }
            }
            UnityEventTools.AddObjectPersistentListener(tog.onValueChanged, selector.Select, characters[i]);
        }

        //registers the changes to be saved when done.
        EditorUtility.SetDirty(selector);
    }
}

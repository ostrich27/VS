using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    WeaponData weaponData;
    string[] weaponSybtypes;
    int selectedWeaponSubtype;

    private void OnEnable()
    {
        //cache the weapon data
        weaponData = (WeaponData)target;

        //retrieve all the weapon sybtypes and cache it
        System.Type baseType = typeof(Weapon);
        List<System.Type> subtypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && p != baseType)
            .ToList();

        // add a none option in front
        List<string> subTypesString = subtypes.Select(t => t.Name).ToList();
        subTypesString.Insert(0, "None");
        weaponSybtypes = subTypesString.ToArray();

        //ensure that we are using the correct weapon subtype
        selectedWeaponSubtype = Math.Max(0, Array.IndexOf(weaponSybtypes, weaponData.behaviour));
    }

    public override void OnInspectorGUI()
    {
        //draw a dropdown in inspector
        selectedWeaponSubtype = EditorGUILayout.Popup("Behaviour", Math.Max(0, selectedWeaponSubtype), weaponSybtypes);

        if(selectedWeaponSubtype > 0)
        {
            //updates the behaviour field
            weaponData.behaviour = weaponSybtypes[selectedWeaponSubtype].ToString();
            EditorUtility.SetDirty(weaponData); // marks the object to save
            DrawDefaultInspector(); //draw the default inspector elements
        }
        else
        {
            weaponData.behaviour = null;
        }
    }
}

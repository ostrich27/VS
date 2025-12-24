using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class UICharacterSelector : MonoBehaviour
{
    public CharacterData defaultCharacter;
    public static CharacterData selected;
    public UIStatsDisplay statsUI;

    [Header("Template")]
    public Toggle toggleTemplate;
    public string characterNamePath = "Character Name";
    public string weaponIconPath = "Weapon Icon";
    public string characterIconPath = "Character Icon";
    public List <Toggle> selectableToggles = new List<Toggle>();

    [Header("DescriptionBox")]
    public TextMeshProUGUI characterFullName;
    public TextMeshProUGUI characterDescription;
    public Image selectedCharacterIcon;
    public Image selectedCharacterWeapon;

    private void Start()
    {
        //if there is a default character assigned, select it on scene load
        if (defaultCharacter) Select(defaultCharacter);
    }

    public static CharacterData[] GetAllCharacterDataAssets()
    {
        List<CharacterData> characters = new List<CharacterData>();

        //populate the list with all characterData assets (editor only)
#if UNITY_EDITOR

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".asset"))
            {
                CharacterData characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                if(characterData != null)
                {
                    characters.Add(characterData);
                }
            }
        }
#else
        Debug.LogWarning("this function cannot be called on builds.");
#endif
        return characters.ToArray();
    }

    public static CharacterData GetData()
    {
        //use the selected character chosen in the select() function
        if (selected)
            return selected;
        else
        {
            //randomly pick a character if we are playing from the editor
            CharacterData[] characters = GetAllCharacterDataAssets();
            if(characters.Length > 0) return characters[Random.Range(0, characters.Length)];
        }
        return null;
    }

    public void Select(CharacterData character)
    {
        //update stat fields in character select screen
        selected = statsUI.character = character;
        statsUI.UpdateFields();

        //update description box contents
        characterFullName.text = character.FullName;
        characterDescription.text = character.CharacterDescription;
        selectedCharacterIcon.sprite = character.Icon;
        selectedCharacterWeapon.sprite = character.StartingWeapon.icon;
    }

}

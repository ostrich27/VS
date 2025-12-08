using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector instance;
    public CharacterData characterData;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Extra " + this + " Deleted");
            Destroy(gameObject);
        }
    }


    public static CharacterData GetData()
    {
        if(instance && instance.characterData)
        {
            return instance.characterData;
        }
        else
        {
            //randomly pick a character if we are playing from the editor
#if UNITY_EDITOR

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            List<CharacterData> characters = new List<CharacterData>();
            foreach(string assetPath in allAssetPaths)
            {
                CharacterData characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                if(characterData != null )
                {
                    characters.Add(characterData);
                }
            }

            //pick a random character if we found any characters
            if(characters.Count > 0) return characters[Random.Range(0, characters.Count)];
#endif
        }
        return null;
    }

    public void SelectCharacter(CharacterData character)
    {
        characterData = character;
    }

    public void DestroySingleton()
    {
        instance = null;
        Destroy(gameObject);
    }
}

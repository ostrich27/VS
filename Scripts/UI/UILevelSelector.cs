using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Reflection;

public class UILevelSelector : MonoBehaviour
{
    public UISceneDataDisplay statsUI;

    public static int selectedLevel = -1;
    public static SceneData currentLevel;
    public List<SceneData> levels = new List<SceneData>();

    [Header("Template")]
    public Toggle toggleTemplate;
    public string LevelNamePath = "Level Name";
    public string LevelNumberPath = "Level Number";
    public string LevelDescriptionPath = "Level Description";
    public string LevelImagePath = "Level Image";
    public List<Toggle> selectableToggles = new List<Toggle>();

    //the level modifiers  will be applied to players and enemies using a buff
    //the buff data is stored in this static variable
    public static BuffData globalBuff;

    //whenever a globalBuff is applied, we will check whether the buff has any
    //effect on the player or enemies and record them here. if there isn't,
    //we don't apply the buff to save overhead
    public static bool globalBuffAffectsPlayer = false, globalBuffAffectsEnemies = false;

    //this is the regex which is used to identify which maps are level maps
    public const string MAP_NAME_FORMAT = "^(Level .*?) ?- ?(.*)$";

    [System.Serializable]
    public class SceneData
    {
        public SceneAsset scene;

        [Header("UI Display")]
        public string displayName;
        public string label;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Modifiers")]
        public CharacterData.Stats playerModifier;
        public EnemyStats.Stats enemyModifier;
        [Min(-1)] public float timeLimit = 0f, clockSpeed = 1f;
        [TextArea] public string extraNotes = "--";
    }


    public static SceneAsset[] GetAllMaps()
    {
        List<SceneAsset> maps = new List<SceneAsset>();

        //populate the list with all scenes starting with "Level -" (Editor only)
#if UNITY_EDITOR
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".unity"))
            {
                SceneAsset map = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                if(map != null && Regex.IsMatch(map.name, MAP_NAME_FORMAT))
                {
                    maps.Add(map);
                }
            }
        }
#else
Debug.LogWarning("This function cannot be called on builds.");
#endif
        maps.Reverse();
        return maps.ToArray();
    }






    //for normal scene changes
    public void SceneChange(string name)
    {
        SceneManager.LoadScene(name);
        Time.timeScale = 1;
    }

    //to load a level from the level select screen
    public void LoadSelectedLevel()
    {
        if(selectedLevel >= 0)
        {
            SceneManager.LoadScene(levels[selectedLevel].scene.name);
            currentLevel = levels[selectedLevel];
            selectedLevel = -1;
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogWarning("No level was selected!");
        }
    }

    //selects a scene that will be loaded with LoadSelectedLevel()
    //also creates the buff that will be applied on that level, and checks if
    //the modifier variables are empty (which are used by PlayerStats and EnemyStats)
    public void Select(int sceneIndex)
    {
        selectedLevel = sceneIndex;
        statsUI.UpdateFields();
        globalBuff = GenerateGlobalBuffData();
        globalBuffAffectsPlayer = globalBuff && IsModifierEmpty(globalBuff.variations[0].playerModifier);
        globalBuffAffectsEnemies = globalBuff && IsModifierEmpty(globalBuff.variations[0].enemyModifier);
    }

    //generate a BuffData object to wrap around the playerModifier and enemyModifier variables
    public BuffData GenerateGlobalBuffData()
    {
        BuffData bd = ScriptableObject.CreateInstance<BuffData>();
        bd.name = "Global Level Buff";
        bd.variations[0].damagePerSecond = 0;
        bd.variations[0].duration = 0;
        bd.variations[0].playerModifier = levels[selectedLevel].playerModifier;
        bd.variations[0].enemyModifier = levels[selectedLevel].enemyModifier;
        return bd;
    }

    //used to check if the playerModifier or enemyModifier of the global buff is empty
    private static bool IsModifierEmpty(object obj)
    {
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields();
        float sum = 0;
        foreach ( FieldInfo f in fields )
        {
            object val = f.GetValue(obj);
            if (val is int) sum += (int)val;
            else if (val is float) sum += (float)val;
        }
        return Mathf.Approximately(sum, 0);
    }
}

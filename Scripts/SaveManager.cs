using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;

/// <summary>
/// A simple SaveManager designed to save the total number of coins the player has.
/// in later parts, this will be used to store all the player's save data, bu we are
/// keeping it simple for now
/// </summary>

public class SaveManager
{
    public class GameData
    {
        // Currency
        public float coins;

        // Persistent meta-upgrade levels (applied on top of character stats)
        // You can freely tweak which stats you expose here.
        public int maxHealthLevel;
        public int recoveryLevel;
        public int armorLevel;
        public int moveSpeedLevel;
        public int mightLevel;
        public int areaLevel;
        public int speedLevel;
        public int durationLevel;
        public int ammountLevel;
        public int cooldownLevel;
        public int luckLevel;
        public int growthLevel;
        public int greedLevel;
        public int curseLevel;
        public int magnetLevel;
        public int revivalLevel;
    }

    const string SAVE_FILE_NAME = "SaveData.json";

    static GameData lastLoadedGameData;
    public static GameData LastLoadedGameData
    {
        get
        {
            if (lastLoadedGameData == null) Load();
                return lastLoadedGameData;
        }
    }

    public static string GetSavePath()
    {
        return string.Format("{0}/{1}", Application.persistentDataPath, SAVE_FILE_NAME);
    }

    /// <summary>
    /// this function, when called without an argument, will save into the last loaded
    /// game file (this is how you should be calling Save() 99% of the time)
    /// but you can optionally also provide an argument to it to if you want to overwrite the save completely
    ///</summary>

    public static void Save(GameData data = null)
    {
        //ensures that the save always works
        if(data == null)
        {
            //if there is no last loaded game, we load the game to populate
            //lastLoadedGameData first, then we save
            if (LastLoadedGameData == null) Load();
            data = LastLoadedGameData;
        }
        File.WriteAllText(GetSavePath(), JsonUtility.ToJson(data));
    }

    public static GameData Load(bool usePreviousLoadIfAvailable = false)
    {
        //usePreviousLoadIfAvailable is meant to speed up load calls,
        //since we don't need to read the save file every time we want to access data
        if(usePreviousLoadIfAvailable && lastLoadedGameData != null)
            return lastLoadedGameData;

        //retrieve the load in the hard drive
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            lastLoadedGameData = JsonUtility.FromJson<GameData>(json);
            if(lastLoadedGameData == null) lastLoadedGameData = new GameData();
        }
        else
        {
            lastLoadedGameData = new GameData();
        }
        return lastLoadedGameData;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Obsolete("passive item obsolete", false)]


[CreateAssetMenu(fileName ="PassiveItemsScriptableObject",menuName ="ScriptableObjects/Passive Items")]
public class PassiveItemsScriptableObject : ScriptableObject
{

    [SerializeField]
    float multipler;
    public float Multipler { get => multipler; private set => multipler = value; }


    [SerializeField]
    int level;   // not meant to be modified in the game only in editor
    public int Level { get => level; private set => level = value; }


    [SerializeField]
    GameObject nextLevelPrefab;   // the prefab of next level i.e what object becomes when it levels up
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField]
    new string name;
    public string Name { get => name; private set => name = value; }

    [SerializeField]
    string description; // what is the description of this passiveItem? [if this passiveItem is and upgrade, place the description of the upgrades]
    public string Description { get => description; private set => description = value; }


    [SerializeField]
    Sprite icon; // not meant to be modified in game[only in editor]
    public Sprite Icon { get => icon; private set => icon = value; }

}

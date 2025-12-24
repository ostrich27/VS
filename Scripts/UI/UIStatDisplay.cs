using System.Reflection;
using UnityEngine;
using TMPro;
using System.Text;

public class UIStatsDisplay : UIPropertyDisplay
{
    public PlayerStats player;
    public CharacterData character; // Display the stats from a character's data instead
    public bool displayCurrentHealth = false;

    public override object GetReadObject()
    {
        //returns player stats in a game scene, returns character stats (plus meta-upgrades)
        //in character select scene as there is no assigned 'player' variable
        if (player) return player.Stats;
        else if(character)
        {
            return character.stats + MetaUpgradeManager.GetMetaStats();
        }
        return new CharacterData.Stats();
    }


    public override void UpdateFields()
    {
        if(!player && !character) return;

        StringBuilder[] allStats = GetProperties(
            BindingFlags.Public | BindingFlags.Instance,
            "CharacterData+Stats"
            );

        //get a reference to both Text objects to render stat names and stat values
        if (!propertyNames) propertyNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if(!propertyValues) propertyValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        //add the current health to the stat box
        if (displayCurrentHealth)
        {
            allStats[0].Insert(0, "Health\n");
            allStats[1].Insert(0, player.CurrentHealth + "\n");
        }

        //updates the fields with the strings we built
        if(propertyNames) propertyNames.text = allStats[0].ToString();
        if (propertyValues) propertyValues.text = allStats[1].ToString();
        propertyValues.fontSize = propertyNames.fontSize;
    }

    private void Reset()
    {
        player = FindObjectOfType<PlayerStats>();
    }

    private void OnEnable()
    {
        GetReadObject();
        UpdateFields();
    }
}

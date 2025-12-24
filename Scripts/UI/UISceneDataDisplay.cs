using System.Text;
using System;
using System.Reflection;
using UnityEngine;
using TMPro;

public class UISceneDataDisplay : UIPropertyDisplay
{
    public UILevelSelector levelSelector;
    TextMeshProUGUI extraStageInfo;

    public override object GetReadObject()
    {
        if(levelSelector && UILevelSelector.selectedLevel >= 0)
            return levelSelector.levels[UILevelSelector.selectedLevel];
        return new UILevelSelector.SceneData();
    }

    //this function is a bit more complicated that the one in UIStatDisplay,
    //because it displays its own variables, plus some of the ones found in playerModifier
    //and enemyModifier. we artificially add these stats by calling ProcessName() and ProcessValue()
    //to add them to the StringBuilders output by GetProperties() below

    public override void UpdateFields()
    {
        //get a reference to both Text objects to render stat names and stat values
        if (!propertyNames) propertyNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!propertyValues) propertyValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (!extraStageInfo) extraStageInfo = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        //get the strings for all the properties
        StringBuilder[] allStats = GetProperties(
            BindingFlags.Public | BindingFlags.Instance,
            "UILevelSelector+SceneData"
            );

        //extract the user and enemy stat modifiers
        //we want to add some of these values to this stat display
        UILevelSelector.SceneData dat = (UILevelSelector.SceneData)GetReadObject();

        allStats[0].AppendLine("Move Speed").AppendLine("Gold Bonus").AppendLine("Luck Bonus").AppendLine("XP Bonus").AppendLine("Enemy Health");

        //add the other bonuses
        Type characterDataStats = typeof(CharacterData.Stats);
        ProcessValue(dat.playerModifier.moveSpeed, allStats[1], characterDataStats.GetField("moveSpeed"));
        ProcessValue(dat.playerModifier.greed, allStats[1], characterDataStats.GetField("greed"));
        ProcessValue(dat.playerModifier.luck, allStats[1], characterDataStats.GetField("luck"));
        ProcessValue(dat.playerModifier.growth, allStats[1], characterDataStats.GetField("growth"));

        Type enemyStats = typeof(EnemyStats.Stats);
        ProcessValue(dat.enemyModifier.maxHealth, allStats[1], enemyStats.GetField("maxHealth"));

        //updates the fields with the strings we built
        if(propertyNames) propertyNames.text = allStats[0].ToString();
        if(propertyValues) propertyValues.text = allStats[1].ToString();
    }

    //overridden as we want to define the list of properties that are shown by this
    protected override bool IsFieldShown(FieldInfo field)
    {
        switch (field.Name)
        {
            default:
                return false;
            case "timeLimit":
            case "clockSpeed":
            case "moveSpeed":
            case "greed":
            case "luck":
            case "growth":
            case "maxHealth":
                return true;
        }
    }

    //don't display the name field
    protected override StringBuilder ProcessName(string name, StringBuilder output, FieldInfo field)
    {
        if(field.Name == "extraNotes") return output;
        return base.ProcessName(name, output, field);
    }

    protected override StringBuilder ProcessValue(object value, StringBuilder output, FieldInfo field)
    {
        //if there are fields specific to this element we must process,
        //we process them without handing it over to the parent
        float fval;
        switch (field.Name)
        {
            case "timeLimit":
                fval = value is int ? (int)value : (float)value;
                if(fval == 0)
                {
                    output.Append(DASH).Append('\n');
                }
                else
                {
                    string minutes = Mathf.FloorToInt(fval / 60).ToString();
                    string seconds = (fval % 60).ToString();
                    if(fval % 60 < 10)
                    {
                        seconds += "0";
                    } 
                    output.Append(minutes).Append(":").Append(seconds).Append('\n');
                }
                return output;

            case "clockSPeed":
                fval = value is int ? (int)value: (float)value;
                output.Append(fval).Append("x").Append('\n');
                return output;

            case "maxHealth":
            case "moveSpeed":
            case "greed":
            case "luck":
            case "growth":
                fval = value is int ? (int)value: (float) value;
                float percentage = Mathf.Round(fval * 100);

                //if  the stat value is 0, just put a dash.
                if(Mathf.Approximately(percentage, 0))
                {
                    output.Append(DASH).Append('\n');
                }
                else
                {
                    if (percentage > 0)
                        output.Append('+');
                    output.Append(percentage).Append('%').Append('\n');
                }
                return output;

            case "extraNotes":
                if(value == null) return output;
                string msg = value.ToString();
                extraStageInfo.text = string.IsNullOrWhiteSpace(msg) ? DASH : msg;
                return output;
        }

        //hand the process over to the parent
        return base.ProcessValue(value, output, field);
    }

    private void Reset()
    {
        levelSelector = FindObjectOfType<UILevelSelector>();
    }
}

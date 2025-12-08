using System.Reflection;
using UnityEngine;
using TMPro;
using System.Text;

public class UIStatsDisplay : MonoBehaviour
{
    public PlayerStats player;
    public bool displayCurrentHealth = false;
    TextMeshProUGUI statNames, statValues;

    private void OnEnable()
    {
        UpdateStatFields();
    }

    public void UpdateStatFields()
    {
        if(!player) return;

        // get refferences to both text objects to render stat names and values

        if(!statNames) statNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if(!statValues) statValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        //render stat names and values
        //use StringBuilders so that string manipulation runs faster
        StringBuilder names = new StringBuilder();
        StringBuilder values = new StringBuilder();

        //add the current health to the stat box
        if(displayCurrentHealth)
        {
            names.AppendLine("Health");
            values.AppendLine(player.CurrentHealth.ToString());
        }

        FieldInfo[] fields = typeof(CharacterData.Stats).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            //render stat names
            names.AppendLine(field.Name);

            //get the stat value
            object val = field.GetValue(player.Stats);
            float fval = val is int? (int)val : (float)val;
            values.Append(fval).Append('\n');

            //print it as a percentage if it has an attribute assigned and is a float
            PropertyAttribute attribute = (PropertyAttribute)PropertyAttribute.GetCustomAttribute(field, typeof(PropertyAttribute));
            if (attribute != null && field.FieldType == typeof(float))
            {
                float percentage = Mathf.Round(fval * 100 - 100);

                //if stat value is 0, just put dash
                if(Mathf.Approximately(percentage, 0))
                {
                    values.Append('-').Append('\n');
                }
                else
                {
                    if (percentage > 0)
                    {
                        values.Append('+');
                        values.Append(percentage).Append('%').Append('\n');
                    }
                }
            }

            //update the fields with the string we built
            statNames.text = PrettifyNames(names);
            statValues.text = values.ToString();

        }
    }

    public static string PrettifyNames(StringBuilder input)
    {
        //return an empty string if stringbuilder is empty
        if (input.Length <= 0) return string.Empty;

        StringBuilder result = new StringBuilder();
        char last = '\0';

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // check when to uppercase or add space to the character
            if (last == '\0' || char.IsWhiteSpace(last))
            {
                c = char.ToUpper(c);
            } else if (char.IsUpper(c))
            {
                result.Append(' '); //insert space before capital letter
            }
            result.Append(c);

            last = c;
        } 
        return result.ToString();
    }
}

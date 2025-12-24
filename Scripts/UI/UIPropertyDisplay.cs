using UnityEngine;
using TMPro;
using System.Reflection;
using System.Text;

public abstract class UIPropertyDisplay : MonoBehaviour
{
    public bool updateInEditor = false;
    protected TextMeshProUGUI propertyNames, propertyValues;
    public const string DASH = "-";

    //update this stat display whenever it is set to be active
    protected virtual void onEnable() { UpdateFields(); }
    protected virtual void OnDrawGizmosSelected() { if (updateInEditor) UpdateFields(); }

    //since every property display class will define its own variables to store the
    //object(s) it is reading, each class will override this function to define the read object
    public abstract object GetReadObject();

    //determines whether to process and show a field or not
    protected virtual bool IsFieldShown(FieldInfo field) { return true; }

    //processes the list of names given to the StringBuilder output
    protected virtual StringBuilder ProcessName(string name, StringBuilder output, FieldInfo field)
    {
        if(!IsFieldShown(field)) return output;
        return output.AppendLine(name);
    }

    //by default, this function only processes integers and floats.
    //we can override this function to make it process other types of variables as well, such as strings
    protected virtual StringBuilder ProcessValue(object value, StringBuilder output, FieldInfo field)
    {
        if (!IsFieldShown(field)) return output;
        float fval = value is int ? (int) value : value is float ? (float) value : 0;

        //print it as a percentage if it has a [Range] or [Min] attribute assigned and is a float
        PropertyAttribute attribute = (PropertyAttribute)field.GetCustomAttribute<RangeAttribute>() ?? field.GetCustomAttribute<MinAttribute>();
        if(attribute != null && field.FieldType == typeof(float))
        {
            float percentage = Mathf.Round(fval * 100 - 100);

            //if the stat value is 0, just put a dash
            if(Mathf.Approximately(percentage, 0))
            {
                output.Append(DASH).Append('\n');
            }
            else
            {
                //always append a value line — include sign for positives
                if(percentage > 0)
                    output.Append('+');
                output.Append(percentage).Append('%').Append('\n');
            }
        }
        else
        {
            output.Append(value).Append('\n');
        }
        return output;
    }

    //returns an array of 2 StringBuilders that will be used to populate the 2 different text boxes in our display classes
    protected virtual StringBuilder[] GetProperties(BindingFlags flags, string targetedType)
    {
        //render all stat names and values.
        //use StringBuilders so that the string manipulation runs faster
        StringBuilder names = new StringBuilder();
        StringBuilder values = new StringBuilder();

        FieldInfo[] fields = System.Type.GetType(targetedType).GetFields(flags);
        foreach (FieldInfo field in fields)
        {
            //render stat names
            ProcessName(field.Name, names, field);
            ProcessValue(field.GetValue(GetReadObject()), values, field);
        }

        //updates the field with the strings we built
        return new StringBuilder[2] { PrettifyNames(names), values };
    }

    //overriden by child classes to print the StringBuilders onto the text boxes
    public abstract void UpdateFields();

    public static StringBuilder PrettifyNames(StringBuilder input)
    {
        //return an empty string if StringBuilder is empty
        if (input.Length <= 0) return null;

        StringBuilder result = new StringBuilder();
        char last = '\0';
        for(int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            //check when to uppercase or add spaces to a character
            if(last == '\0' || char.IsWhiteSpace(last))
            {
                c = char.ToUpper(c);
            }
            else if (char.IsUpper(c))
            {
                result.Append(' '); //insert space before capital letter
            }
            result.Append(c);

            last = c;
        }
        return result;
    }
}

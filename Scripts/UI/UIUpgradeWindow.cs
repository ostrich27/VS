using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// we require VerticalLayoutGroup  on the GameObject this is attached to, because it uses the components
/// to make sure the buttons are evenly spaced out
/// </summary>
/// 
[RequireComponent(typeof(VerticalLayoutGroup))]
public class UIUpgradeWindow : MonoBehaviour
{
    //we will need to access the padding / spacing attributes on the layout
    VerticalLayoutGroup verticalLayout;

    //the button and tooltip template GameObjects we have to assign
    public RectTransform upgradeOptionTemplate;
    public TextMeshProUGUI tooltipTemplate;

    [Header("Settings")]
    public int maxOptions = 4; // we cannot show more options than this
    public string newText = "New!"; // the text that shows when a new upgrade is shown

    //color of the "New!" text and regular text
    public Color newTextColor = Color.yellow,levelTextColor = Color.white;

    //these are the paths to the different UI elements in the <upgradeOptionTemplate>
    [Header("Paths")]
    public string iconPath = "Icon/Item Icon";
    public string namePath = "Name", descriptionPath = "Description", buttonPath = "Button", levelPath = "Level";

    //these are private variables that are used by the functions to track the status of different things in the UIUpgradeWindow
    RectTransform rectTransform; // the rectTransform of this element for easy reference
    float optionHeight; // the default height of the upgradeOptionTemplate
    int activeOptions; // tracks the number of options that are active currently

    //this is a list of all the upgrade buttons on the window
    List<RectTransform> upgradeOptions = new List<RectTransform>();

    //this is used to track the screen width/height of the last frame
    //to detect screen size changes, so we know when we have to recalculate the size

    Vector2 lastScreen;

    ///<summary>
    ///this is the main function that we will be calling on this script
    ///you need to specify which <inventory> to add the item to, and a list of all
    ///<possibleUpgrades> to show. it will select <pick> number of upgrades and show them.
    ///finally, if you specify a <toolTip>, then some text will appear at the bottom of the window
    ///</summary>

    public void SetUpgrades(PlayerInventory inventory, List<ItemData> possibleUpgrades, int pick = 3, string toolTip = "")
    {
        pick = Mathf.Min(maxOptions, pick);

        //if we don't have enough upgrade option boxes, create them
        if(maxOptions > upgradeOptions.Count)
        {
            for(int i = upgradeOptions.Count; i < pick; i++)
            {
                GameObject go = Instantiate(upgradeOptionTemplate.gameObject, transform);
                upgradeOptions.Add((RectTransform)go.transform);
            }
        }

        //if a string is provided, turn on the tooltip
        tooltipTemplate.text = toolTip;
        tooltipTemplate.gameObject.SetActive(toolTip.Trim() != "");

        //activate only the number of upgrade options we need, and arm the buttons and the different attributes like descriptions, etc.
        activeOptions = 0;
        int totalPossibleUpgrades = possibleUpgrades.Count;  // how many upgrades do we have to choose from?
        foreach(RectTransform r in upgradeOptions)
        {
            if(activeOptions < pick && activeOptions < totalPossibleUpgrades)
            {
                r.gameObject.SetActive(true);

                //select one of the possible upgrades, then remove it from the list
                ItemData selected = possibleUpgrades[Random.Range(0, possibleUpgrades.Count)];
                possibleUpgrades.Remove(selected);
                Item item = inventory.Get(selected);

                //insert the name of the item
                TextMeshProUGUI name = r.Find(namePath).GetComponent<TextMeshProUGUI>();
                if (name)
                {
                    name.text = selected.name;
                }

                //insert the current level of the item, or a "New!" text if it is a new weapon
                TextMeshProUGUI level = r.Find(levelPath).GetComponent<TextMeshProUGUI>();
                if(level)
                {
                    if (item)
                    {
                        if(item.currentLevel >= item.maxLevel)
                        {
                            level.text = "Max!";
                            level.color = newTextColor;
                        }
                        else
                        {
                            level.text = selected.GetLevelData(item.currentLevel + 1).name;
                            level.color = levelTextColor;
                        }
                    }
                    else
                    {
                        level.text = newText;
                        level.color = newTextColor;
                    }
                }

                //insert the description of the item
                TextMeshProUGUI desc = r.Find(descriptionPath).GetComponent<TextMeshProUGUI>();
                if(desc)
                {
                    if (item)
                    {
                        desc.text = selected.GetLevelData(item.currentLevel + 1).description;
                    }
                    else
                    {
                        desc.text = selected.GetLevelData(1).description;
                    }
                }

                //insert the icon of the item
                Image icon = r.Find(iconPath).GetComponent<Image>();
                if(icon)
                {
                    icon.sprite = selected.icon;
                }

                //insert the button action binding
                Button b = r.Find(buttonPath).GetComponent<Button>();
                if (b)
                {
                    b.onClick.RemoveAllListeners();
                    if(item) 
                        b.onClick.AddListener(() => { inventory.LevelUp(item); });
                    else 
                        b.onClick.AddListener(() => { inventory.Add(selected); });
                }

                activeOptions++;
            }
            else r.gameObject.SetActive(false);
        }

        //sizes all the elements so they do not exceed the size of the box
        RecalculateLayout();

    }

    /// <summary>
    /// recalculates the heights of all elements.
    /// called whenever the size of the window changes.
    /// we are doing this manually because the VerticalLayoutGroup doesn't allways space all the elements evenly
    /// </summary>
    void RecalculateLayout()
    {
        //calculates the total available height for all options, then divides it by the number of options
        optionHeight = (rectTransform.rect.height - verticalLayout.padding.top - verticalLayout.padding.bottom - (maxOptions - 1) * verticalLayout.spacing);
        if (activeOptions == maxOptions && tooltipTemplate.gameObject.activeSelf)
            optionHeight /= maxOptions + 1;
        else
            optionHeight /= maxOptions;

        //recalculates the height of the tooltip as well af it is currently active
        if (tooltipTemplate.gameObject.activeSelf)
        {
            RectTransform tooltipRect = (RectTransform)tooltipTemplate.transform;
            tooltipTemplate.gameObject.SetActive(true);
            tooltipRect.sizeDelta = new Vector2(tooltipRect.sizeDelta.x, optionHeight);
            tooltipTemplate.transform.SetAsLastSibling();
        }

        //sets the height of every active upgrade option button
        foreach(RectTransform r in upgradeOptions)
        {
            if(!r.gameObject.activeSelf) continue;
            r.sizeDelta = new Vector2(r.sizeDelta.x, optionHeight);
        }
    }

    /// <summary>
    ///  this function just checks if the last screen width/height is the same as the current one
    ///  if not, the screen has changed sizes and we will call RecalculateLayout to update the height of our buttons
    ///  </summary>
    ///  
    void Update()
    {
        if(lastScreen.x != Screen.width || lastScreen.y != Screen.height)
        {
            RecalculateLayout();
            lastScreen = new Vector2(Screen.width, Screen.height);
        }
    }

    void Awake()
    {
        //populates all our important variables
        verticalLayout = GetComponentInChildren<VerticalLayoutGroup>();
        if(tooltipTemplate) tooltipTemplate.gameObject.SetActive(false);
        if(upgradeOptionTemplate) upgradeOptions.Add(upgradeOptionTemplate);

        //get the RectTransform of this object for height calculations
        rectTransform = (RectTransform)transform;
    }

    /// <summary>
    /// just a convenience function to automatically populate our variable.
    /// it will automatically search for a GameObject called "Upgrade Option" and assign
    /// it as the upgradeOptionTemplate, then search for a GameObject "Tooltip" to be assigned as the tooltipTemplate
    /// </summary>
    /// 

    void Reset()
    {
        upgradeOptionTemplate = (RectTransform)transform.Find("Upgrade Option");
        tooltipTemplate = transform.Find("Tooltip").GetComponentInChildren<TextMeshProUGUI>();
    }

}

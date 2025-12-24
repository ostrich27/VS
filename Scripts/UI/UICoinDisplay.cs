using UnityEngine;
using TMPro;


/// <summary>
/// component that is attached to GameObject to make it display the player's coins
/// either in-game, or the total number of coins player has, depending whether
/// the collector variable is set.
/// </summary>

public class UICoinDisplay : MonoBehaviour
{
    TextMeshProUGUI displayTarget;
    public PlayerCollector collector;

    // Start is called before the first frame update
    void Start()
    {
        displayTarget = GetComponentInChildren<TextMeshProUGUI>();
        UpdateDisplay();
        if(collector != null) collector.onCoinCollected += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        //if a collector is assigned, we will display the number of coins the collector has.
        if(collector != null)
        {
            displayTarget.text = Mathf.RoundToInt(collector.GetCoins()).ToString();
        }
        else
        {
            //if not, we will get the current number of coins that are saved
            float coins = SaveManager.LastLoadedGameData.coins;
            displayTarget.text = Mathf.RoundToInt(coins).ToString();
        }
    }
}

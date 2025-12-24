using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Obsolete("passive item obsolete", false)]

public class PassiveItem : MonoBehaviour
{
    protected PlayerStats player;
    public PassiveItemsScriptableObject passiveItemData;


    protected virtual void ApplyModifier()
    {
        // apply the modifier to the appropriate class in the child classes
    }

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerStats>();
        ApplyModifier();
    }
}

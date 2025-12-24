using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropRateManager : MonoBehaviour
{
    [System.Serializable]
    public class Drops
    {
        public string name;
        public GameObject itemPrefab;
        public float dropRate;
    }
    public bool active = false;
    public List<Drops> drops;

    private void OnDestroy()
    {
        if(!active) return; // prevents spawns from happening if inactive
        if (!gameObject.scene.isLoaded) 
        {
            return;
        }
        float randomNumber = Random.Range(0f, 100f);
        List<Drops> possibleDrops = new List<Drops>();

        foreach (Drops rate in drops)
        {
            if (randomNumber <= rate.dropRate)
            { 
                possibleDrops.Add(rate);
            }
        }

        // check if there is possible drops
        if (possibleDrops.Count > 0)
        {
            Drops drops = possibleDrops[Random.Range(0, possibleDrops.Count)];
            Instantiate(drops.itemPrefab, transform.position, Quaternion.identity);
        }

    }       
}

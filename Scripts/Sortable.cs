using UnityEngine;
/// <summary>
/// this is a class that can be subclassed by any other class to make the sprites 
/// of the class acutomatically sort themselves by the y-axis
/// </summary>
/// 

[RequireComponent (typeof(SpriteRenderer))]
public abstract class Sortable : MonoBehaviour
{
    SpriteRenderer sorted;
    public bool sortingActive = true; // allows us to deactivate this on certain objects
    public const float MIN_DISTANCE = 0.2f; // minimum distance before the sorting value updates
    int lastSortOrder = 0;


    protected virtual void Start()
    {
        sorted = GetComponent<SpriteRenderer>();
    }

    protected virtual void LateUpdate()
    {
        if (!sorted) return;
        int newSortOrder = (int)(-transform.position.y / MIN_DISTANCE);
        if (lastSortOrder != newSortOrder)
        {
            lastSortOrder = sorted.sortingOrder;
            sorted.sortingOrder = newSortOrder;
        }
    }
}

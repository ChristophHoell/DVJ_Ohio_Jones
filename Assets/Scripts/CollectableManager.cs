using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public enum CollectableType
    {
        Collectable,
        ESquare,
        Triangle
    }

    public CollectableType collectableType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collectableType == CollectableType.Collectable)
            {
                GameManager.instance.AddPoints(1); // Add 1 point
            }
            else if (collectableType == CollectableType.ESquare)
            {
                GameManager.instance.AddPoints(5); // Add 5 points
            }
            else if (collectableType == CollectableType.Triangle)
            {
                GameManager.instance.AddPoints(10); // Add 10 points
            }
        }
    }
}

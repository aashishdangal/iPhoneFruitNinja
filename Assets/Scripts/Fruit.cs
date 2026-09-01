
using UnityEngine;

public class Fruit : MonoBehaviour
{
    private bool sliced = false;

    public void Slice()
    {
        // Prevent the same fruit from being sliced more than once
        if (sliced)
            return;

        sliced = true;

        Debug.Log("🍎 FRUIT SLICED!");

        // Destroy the fruit
        Destroy(gameObject);
    }
}
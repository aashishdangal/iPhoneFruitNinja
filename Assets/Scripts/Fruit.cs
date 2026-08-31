using UnityEngine;

public class Fruit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something touched the fruit: " + other.gameObject.name);

        if (other.GetComponent<SwordController>() != null)
        {
            Debug.Log("SLICE!");
            Destroy(gameObject);
        }
    }
}
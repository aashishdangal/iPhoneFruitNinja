using UnityEngine;

public class Bomb : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SwordSlicer>() != null)
        {
            Debug.Log("BOMB HIT");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}
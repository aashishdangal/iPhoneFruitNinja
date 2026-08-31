using UnityEngine;

public class SwordSlicer : MonoBehaviour
{
    private Vector3 previousPosition;

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;

        if (Physics.Linecast(previousPosition, currentPosition, out RaycastHit hit))
        {
            if (hit.collider.GetComponent<Fruit>() != null)
            {
                Debug.Log("🍉 SLICED BY SWORD TRAIL!");

                Destroy(hit.collider.gameObject);
            }
        }

        previousPosition = currentPosition;
    }
}
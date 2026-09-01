using UnityEngine;

public class SwordSlicer : MonoBehaviour
{
    public Transform swordBase;

    private Vector3 previousTipPosition;
    private Vector3 previousBasePosition;

    void Start()
    {
        previousTipPosition = transform.position;
        previousBasePosition = swordBase.position;
    }

    void Update()
    {
        Vector3 currentTipPosition = transform.position;
        Vector3 currentBasePosition = swordBase.position;

        CheckBlade(previousTipPosition, currentTipPosition);
        CheckBlade(previousBasePosition, currentBasePosition);
        CheckBlade(previousTipPosition, currentBasePosition);
        CheckBlade(previousBasePosition, currentTipPosition);

        previousTipPosition = currentTipPosition;
        previousBasePosition = currentBasePosition;
    }

    void CheckBlade(Vector3 start, Vector3 end)
    {
        if (Physics.Linecast(start, end, out RaycastHit hit))
        {
            Fruit fruit = hit.collider.GetComponent<Fruit>();

            if (fruit != null)
            {
                Debug.Log("🍉 SLICED!");

                Destroy(fruit.gameObject);
            }
        }
    }
}
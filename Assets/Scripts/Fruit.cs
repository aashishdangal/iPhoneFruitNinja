using UnityEngine;

public class Fruit : MonoBehaviour
{
    private bool sliced = false;
    private bool missed = false;

    [Header("Miss Detection")]
    public float missY = -6f;

    [Header("Slice Effect")]
    public GameObject halfPrefab;
    public float separationSpeed = 2f;
    public float upwardSpeed = 1.5f;
    public float tumbleSpeed = 3f;
    public float halfLifetime = 3f;

    public void Slice()
    {
        if (sliced || missed)
            return;

        sliced = true;

        if (halfPrefab != null)
        {
            Rigidbody fruitBody = GetComponent<Rigidbody>();

            Vector3 inheritedVelocity = fruitBody != null
                ? fruitBody.linearVelocity
                : Vector3.zero;

            SpawnHalf(-1f, inheritedVelocity);
            SpawnHalf(1f, inheritedVelocity);
        }

        Destroy(gameObject);
    }

    private void SpawnHalf(float side, Vector3 inheritedVelocity)
    {
        // Start with the half prefab's configured orientation.
        Quaternion appleDefaultRotation = Quaternion.Euler(-90f, 0f, 0f);

Quaternion rotation =
    transform.rotation *
    Quaternion.Inverse(appleDefaultRotation) *
    halfPrefab.transform.rotation;

        // Turn the second copy around to show the opposite side.
        if (side > 0f)
        {
            Vector3 appleUp = transform.rotation *
    Quaternion.Inverse(appleDefaultRotation) *
    Vector3.up;

rotation =
    Quaternion.AngleAxis(180f, appleUp) * rotation;
        }

        Vector3 position =
            transform.position + Vector3.right * side * 0.08f;

        GameObject half = Instantiate(
            halfPrefab,
            position,
            rotation
        );

        half.SetActive(true);

        // These pieces are visual debris, not sliceable fruits.
        foreach (Collider pieceCollider
                 in half.GetComponentsInChildren<Collider>())
        {
            pieceCollider.enabled = false;
        }

        Rigidbody body = half.GetComponent<Rigidbody>();

        if (body != null)
        {
            body.isKinematic = false;
            body.useGravity = true;
            body.constraints = RigidbodyConstraints.FreezePositionZ;

            body.linearVelocity = new Vector3(
                inheritedVelocity.x + side * separationSpeed,
                inheritedVelocity.y + upwardSpeed,
                0f
            );

            body.angularVelocity =
                new Vector3(0f, side * tumbleSpeed, side * tumbleSpeed);
        }

        Destroy(half, halfLifetime);
    }

    void Update()
    {
        if (sliced || missed)
            return;

        if (transform.position.y <= missY)
        {
            missed = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.FruitMissed();
            }

            Destroy(gameObject);
        }
    }
}
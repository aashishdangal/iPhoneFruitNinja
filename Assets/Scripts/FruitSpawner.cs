using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public GameObject fruitPrefab;
    public float spawnInterval = 1.2f;
    public float launchForce = 7f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnFruit), 1f, spawnInterval);
    }

    void SpawnFruit()
    {
        float randomX = Random.Range(-4f, 4f);

        Vector3 spawnPosition = new Vector3(
            randomX,
            -4f,
            5f
        );

        GameObject fruit = Instantiate(
            fruitPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody rb = fruit.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(
                Random.Range(-1.5f, 1.5f),
                launchForce,
                0f
            );
        }
    }
}
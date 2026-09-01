using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Fruit")]
    public GameObject fruitPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.2f;
    public float spawnXRange = 4.5f;
    public float spawnY = -4.5f;
    public float spawnZ = 5f;

    [Header("Launch Settings")]
    public float minUpwardForce = 8f;
    public float maxUpwardForce = 10f;
    public float horizontalForce = 2.5f;

    [Header("Group Settings")]
    public int minFruitsPerGroup = 1;
    public int maxFruitsPerGroup = 3;
    public float groupSpawnDelay = 0.12f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnGroup), 1f, spawnInterval);
    }

    void SpawnGroup()
    {
        int fruitCount = Random.Range(
            minFruitsPerGroup,
            maxFruitsPerGroup + 1
        );

        StartCoroutine(SpawnFruits(fruitCount));
    }

    System.Collections.IEnumerator SpawnFruits(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnFruit();

            yield return new WaitForSeconds(groupSpawnDelay);
        }
    }

    void SpawnFruit()
    {
        float randomX = Random.Range(-spawnXRange, spawnXRange);

        Vector3 spawnPosition = new Vector3(
            randomX,
            spawnY,
            spawnZ
        );

        GameObject fruit = Instantiate(
            fruitPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody rb = fruit.GetComponent<Rigidbody>();

        if (rb != null)
        {
            float horizontalVelocity =
                Random.Range(-horizontalForce, horizontalForce);

            float upwardVelocity =
                Random.Range(minUpwardForce, maxUpwardForce);

            rb.linearVelocity = new Vector3(
                horizontalVelocity,
                upwardVelocity,
                0f
            );
        }
    }
}
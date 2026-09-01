using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject fruitPrefab;
    public GameObject bombPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.8f;
    public float spawnXRange = 4.5f;
    public float spawnY = -4.5f;
    public float spawnZ = 5f;

    [Header("Launch Settings")]
    public float minUpwardForce = 8f;
    public float maxUpwardForce = 10f;
    public float horizontalForce = 2.5f;

    [Header("Group Settings")]
    public int minFruitsPerGroup = 1;
    public int maxFruitsPerGroup = 2;
    public float groupSpawnDelay = 0.15f;

    [Header("Bomb Settings")]
    [Range(0f, 1f)]
    public float bombChance = 0.1f;

    private bool spawning = true;

    void Start()
    {
        InvokeRepeating(nameof(SpawnGroup), 1f, spawnInterval);
    }

    void SpawnGroup()
    {
        if (!spawning)
            return;

        int objectCount = Random.Range(
            minFruitsPerGroup,
            maxFruitsPerGroup + 1
        );

        StartCoroutine(SpawnObjects(objectCount));
    }

    System.Collections.IEnumerator SpawnObjects(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!spawning)
                yield break;

            SpawnObject();

            yield return new WaitForSeconds(groupSpawnDelay);
        }
    }

    void SpawnObject()
    {
        float randomX = Random.Range(-spawnXRange, spawnXRange);

        Vector3 spawnPosition = new Vector3(
            randomX,
            spawnY,
            spawnZ
        );

        GameObject prefabToSpawn;

        if (Random.value < bombChance && bombPrefab != null)
        {
            prefabToSpawn = bombPrefab;
        }
        else
        {
            prefabToSpawn = fruitPrefab;
        }

        GameObject spawnedObject = Instantiate(
            prefabToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();

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

    public void StopSpawning()
    {
        spawning = false;
        CancelInvoke(nameof(SpawnGroup));
    }
}
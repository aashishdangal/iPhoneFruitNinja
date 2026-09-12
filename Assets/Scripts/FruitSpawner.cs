using System.Collections;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Fruit Prefabs")]
    public GameObject[] fruitPrefabs;

    [Header("Bomb")]
    public GameObject bombPrefab;

    [Header("Spawn Area")]
    public float minX = -5f;
    public float maxX = 5f;
    public float spawnY = -5f;
    public float spawnZ = 0f;

    [Header("Launch Settings")]
    public float minForce = 8f;
    public float maxForce = 12f;
    public float minHorizontalForce = -2f;
    public float maxHorizontalForce = 2f;

    [Header("Wave Settings")]
    public float minSpawnDelay = 1.5f;
    public float maxSpawnDelay = 2.2f;
    public int minFruitsPerWave = 1;
    public int maxFruitsPerWave = 3;
    public float fruitSpacing = 0.12f;

    [Header("Bomb Settings")]
    [Range(0f, 1f)]
    public float bombChance = 0.12f;

    [Header("Bomb Drop Settings")]
    public float bombSpawnY = 7f;
    public float bombFallSpeed = 3f;

    [Header("Difficulty")]
    public float difficultyIncreaseTime = 20f;
    public float forceIncreasePerLevel = 0.5f;

    private bool spawning = true;
    private float gameTime = 0f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (spawning)
        {
            gameTime += Time.deltaTime;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            float delay = Random.Range(
                minSpawnDelay,
                maxSpawnDelay
            );

            yield return new WaitForSeconds(delay);

            if (!spawning)
                yield break;

            SpawnWave();
        }
    }

    void SpawnWave()
    {
        int difficultyLevel = difficultyIncreaseTime > 0f
            ? Mathf.FloorToInt(gameTime / difficultyIncreaseTime)
            : 0;

        int fruitCount = Random.Range(
            minFruitsPerWave,
            maxFruitsPerWave + 1
        );

        if (Random.value < bombChance)
        {
            SpawnBomb();
        }

        StartCoroutine(
            SpawnFruitWave(fruitCount, difficultyLevel)
        );
    }

    IEnumerator SpawnFruitWave(
        int fruitCount,
        int difficultyLevel
    )
    {
        for (int i = 0; i < fruitCount; i++)
        {
            if (!spawning)
                yield break;

            SpawnFruit(difficultyLevel);

            yield return new WaitForSeconds(fruitSpacing);
        }
    }

    void SpawnFruit(int difficultyLevel)
    {
        if (fruitPrefabs == null || fruitPrefabs.Length == 0)
        {
            Debug.LogError("No fruit prefabs assigned!");
            return;
        }

        GameObject fruitPrefab = fruitPrefabs[
            Random.Range(0, fruitPrefabs.Length)
        ];

        if (fruitPrefab == null)
        {
            Debug.LogError("Fruit prefab slot is empty!");
            return;
        }

        Vector3 spawnPosition = new Vector3(
            Random.Range(minX, maxX),
            spawnY,
            spawnZ
        );

        GameObject fruit = Instantiate(
            fruitPrefab,
            spawnPosition,
            fruitPrefab.transform.rotation
        );

        Rigidbody rb = fruit.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Fruit has no Rigidbody!");
            return;
        }

        float verticalForce =
            Random.Range(minForce, maxForce) +
            difficultyLevel * forceIncreasePerLevel;

        float horizontalForce = Random.Range(
            minHorizontalForce,
            maxHorizontalForce
        );

        rb.isKinematic = false;
        rb.useGravity = true;

        // Keep fruits on the gameplay plane and steady.
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(
            new Vector3(horizontalForce, verticalForce, 0f),
            ForceMode.Impulse
        );
    }

    void SpawnBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogWarning("Bomb prefab not assigned!");
            return;
        }

        Vector3 spawnPosition = new Vector3(
            Random.Range(minX, maxX),
            bombSpawnY,
            spawnZ
        );

        // Preserve the nose-down rotation saved in the prefab.
        GameObject bomb = Instantiate(
            bombPrefab,
            spawnPosition,
            bombPrefab.transform.rotation
        );

        Rigidbody rb = bomb.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Bomb has no Rigidbody!");
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotation;

        rb.angularVelocity = Vector3.zero;

        // Start downward; gravity accelerates the fall.
        rb.linearVelocity =
            Vector3.down * Mathf.Max(0f, bombFallSpeed);
    }

    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
    }
}